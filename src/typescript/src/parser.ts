/**
 * Parser for DataFilters query syntax strings.
 *
 * Uses Chevrotain for lexer and parser implementation,
 * mirroring the C# FilterTokenParser grammar.
 *
 * Supported syntax:
 *   - `field=value`            → EqualsFilter
 *   - `field=value*`           → StartsWithFilter
 *   - `field=*value`           → EndsWithFilter
 *   - `field=*value*`          → ContainsFilter
 *   - `field=!value`           → NotFilter(EqualsFilter)
 *   - `field=[min TO max]`     → AndFilter(gte, lte)
 *   - `field=[min TO *[`       → GreaterThanOrEqualFilter
 *   - `field=]* TO max]`       → LessThanOrEqualFilter
 *   - `field=]min TO max[`     → AndFilter(gt, lt)
 *   - `field=expr1,expr2`      → AndFilter (same field)
 *   - `field=expr1|expr2`      → OrFilter
 *   - `field1=v1&field2=v2`    → AndFilter or OrFilter based on options
 */

import {
  createToken,
  Lexer,
  EmbeddedActionsParser,
  IToken,
  TokenType,
} from "chevrotain";

import {
  AndFilter,
  ContainsFilter,
  EndsWithFilter,
  EqualsFilter,
  GreaterThanFilter,
  GreaterThanOrEqualFilter,
  IFilter,
  LessThanFilter,
  LessThanOrEqualFilter,
  NotFilter,
  OneOfFilter,
  OrFilter,
  StartsWithFilter,
} from "./expressions";
import { FilterLogic, FilterOptions } from "./filterOptions";

// ===== Token Definitions =====
//
// Text must be defined before TO so that longer_alt can reference it.
// The token array order governs tie-breaking: TO is listed before Text so
// that the exact string "TO" tokenises as the keyword. For longer strings
// such as "TORONTO", longer_alt ensures Text wins via maximum munch.

/** Backslash followed by any character — the leading backslash is stripped at parse time. */
const Escaped = createToken({ name: "Escaped", pattern: /\\[\s\S]/ });

/** Any run of non-reserved characters. Excludes: \\ = , | ! * [ ] and whitespace. */
const Text = createToken({ name: "Text", pattern: /[^\\=,|!*[\]\s]+/ });

/** Range separator keyword. longer_alt ensures "TORONTO" stays a single Text token. */
const TO = createToken({ name: "TO", pattern: /TO/, longer_alt: [Text] });

const Comma = createToken({ name: "Comma", pattern: /,/ });
const Pipe = createToken({ name: "Pipe", pattern: /\|/ });
const Bang = createToken({ name: "Bang", pattern: /!/ });
const Asterisk = createToken({ name: "Asterisk", pattern: /\*/ });
const LeftSquare = createToken({ name: "LeftSquare", pattern: /\[/ });
const RightSquare = createToken({ name: "RightSquare", pattern: /\]/ });
const Equals = createToken({ name: "Equals", pattern: /=/ });

/** Whitespace is silently skipped everywhere, including around TO in ranges. */
const Whitespace = createToken({
  name: "Whitespace",
  pattern: /\s+/,
  group: Lexer.SKIPPED,
});

const ALL_TOKENS: TokenType[] = [
  Escaped,
  TO,
  Comma,
  Pipe,
  Bang,
  Asterisk,
  LeftSquare,
  RightSquare,
  Equals,
  Whitespace,
  Text,
];

const FILTER_LEXER = new Lexer(ALL_TOKENS);

// ===== Helpers =====

/** Return the semantic value of a token, stripping the leading backslash from Escaped tokens. */
function tokenValue(token: IToken): string {
  let result: string;
  if (token.tokenType === Escaped) {
    result = token.image.slice(1);
  } else {
    result = token.image;
  }
  return result;
}

// ===== Pre-processing: URL-decode and normalise =====

function decodeQueryComponent(value: string): string {
  const normalized = value.replace(/\+/g, " ");
  let result: string;
  try {
    result = decodeURIComponent(normalized);
  } catch {
    result = normalized;
  }
  return result;
}

/**
 * Split on unencoded ampersands first (so %26 inside values is preserved),
 * URL-decode each field=value segment independently, then rejoin with commas
 * so the Chevrotain grammar only needs a single separator token.
 *
 * Backslash-escaped ampersands (\&) are treated as literal ampersand
 * characters and do NOT trigger a split.  An even number of leading
 * backslashes means the last backslash is itself escaped, so the `&` IS
 * a separator (e.g. "foo\\&bar" → two parts, because `\\` is an escaped
 * backslash and `&` is the field separator).
 */
function normalizeExpression(expression: string): string {
  const withoutPrefix = expression.startsWith("?")
    ? expression.slice(1)
    : expression;

  const rawParts: string[] = [];
  let current = "";

  for (let i = 0; i < withoutPrefix.length; i++) {
    const char = withoutPrefix[i];
    if (char === "&") {
      // Count how many backslashes immediately precede this `&`.
      let backslashCount = 0;
      let j = i - 1;
      while (j >= 0 && withoutPrefix[j] === "\\") {
        backslashCount++;
        j--;
      }
      // An odd number of backslashes means this `&` is escaped.
      if (backslashCount % 2 === 1) {
        current += char;
      } else {
        rawParts.push(current);
        current = "";
      }
    } else {
      current += char;
    }
  }
  rawParts.push(current);

  let result: string;

  if (rawParts.length <= 1) {
    result = decodeQueryComponent(withoutPrefix);
  } else {
    const normalizedParts = rawParts
      .map((part) => part.trim())
      .filter((part) => part.length > 0)
      .map((part) => {
        const eqIndex = part.indexOf("=");
        let normalizedPart: string;
        if (eqIndex === -1) {
          normalizedPart = decodeQueryComponent(part);
        } else {
          const field = decodeQueryComponent(part.slice(0, eqIndex));
          const value = decodeQueryComponent(part.slice(eqIndex + 1));
          normalizedPart = `${field}=${value}`;
        }
        return normalizedPart;
      });

    result = normalizedParts.join(",");
  }

  return result;
}

// ===== Parser =====

interface FilterPart {
  filter: IFilter;
  field: string;
}

class DataFilterParser extends EmbeddedActionsParser {
  /** Field name set by the most recent fieldAssignment rule. */
  private currentField: string = "";
  /** Options supplied for this parse call. */
  private currentOptions: FilterOptions | undefined;
  /** Accumulated (filter, field) pairs, one per comma-separated input part. */
  private parsedParts: FilterPart[] = [];
  /** Every distinct field name explicitly introduced in this parse call. */
  private encounteredFields: Set<string> = new Set();

  public constructor() {
    super(ALL_TOKENS, { recoveryEnabled: false });
    this.performSelfAnalysis();
  }

  // ---------------------------------------------------------------------------
  // Grammar rules
  // ---------------------------------------------------------------------------

  /**
   * Top-level rule: one or more comma-separated field-or-value parts.
   * Returns the combined IFilter.
   */
  public readonly filterInput = this.RULE("filterInput", (): IFilter => {
    this.SUBRULE(this.fieldOrValuePart);

    this.MANY(() => {
      this.CONSUME(Comma);
      this.SUBRULE2(this.fieldOrValuePart);
    });

    return this.ACTION(() => this.buildResult());
  });

  /**
   * Dispatches to fieldAssignment when the upcoming tokens look like
   * "fieldName =", otherwise falls through to inheritedValuePart.
   */
  private readonly fieldOrValuePart = this.RULE(
    "fieldOrValuePart",
    (): void => {
      this.OR([
        {
          GATE: () => this.isFieldAssignmentStart(),
          ALT: () => {
            this.SUBRULE(this.fieldAssignment);
          },
        },
        {
          ALT: () => {
            this.SUBRULE(this.inheritedValuePart);
          },
        },
      ]);
    },
  );

  /** Parses "fieldName = valueExpression" and records the result. */
  private readonly fieldAssignment = this.RULE(
    "fieldAssignment",
    (): void => {
      const field = this.SUBRULE(this.fieldNameRule);
      this.CONSUME(Equals);
      this.ACTION(() => {
        this.currentField = field;
        this.encounteredFields.add(field);
      });
      const filter = this.SUBRULE(this.valueExpression);
      this.ACTION(() => {
        this.parsedParts.push({ filter, field });
      });
    },
  );

  /** Parses a value expression that inherits the field from the last assignment. */
  private readonly inheritedValuePart = this.RULE(
    "inheritedValuePart",
    (): void => {
      const field = this.currentField;
      this.ACTION(() => {
        if (!field) {
          throw new Error(
            "Cannot determine field for expression part: no inherited field available",
          );
        }
      });
      const filter = this.SUBRULE(this.valueExpression);
      this.ACTION(() => {
        this.parsedParts.push({ filter, field });
      });
    },
  );

  /**
   * A value expression: either a range ([min TO max]) or a
   * pipe-separated expression.
   */
  private readonly valueExpression = this.RULE(
    "valueExpression",
    (): IFilter => {
      let result: IFilter;
      this.OR([
        {
          GATE: () => {
            const tt = this.LA(1).tokenType;
            return tt === LeftSquare || tt === RightSquare;
          },
          ALT: () => {
            result = this.SUBRULE(this.rangeExpression);
          },
        },
        {
          ALT: () => {
            result = this.SUBRULE(this.pipeExpression);
          },
        },
      ]);
      return result!;
    },
  );

  /**
   * Pipe-separated alternatives: "a|b" produces OrFilter, "a|b|c" produces
   * OneOfFilter.
   */
  private readonly pipeExpression = this.RULE(
    "pipeExpression",
    (): IFilter => {
      const first = this.SUBRULE(this.negatedExpr);
      const parts: IFilter[] = [first];

      this.MANY(() => {
        this.CONSUME(Pipe);
        parts.push(this.SUBRULE2(this.negatedExpr));
      });

      let result: IFilter;
      if (parts.length === 1) {
        result = parts[0];
      } else if (parts.length === 2) {
        result = new OrFilter(parts[0], parts[1]);
      } else {
        result = new OneOfFilter(parts);
      }
      return result;
    },
  );

  /** Optional "!" negation prefix followed by a primary expression. */
  private readonly negatedExpr = this.RULE("negatedExpr", (): IFilter => {
    let negated = false;
    this.OPTION(() => {
      this.CONSUME(Bang);
      this.ACTION(() => {
        negated = true;
      });
    });
    const inner = this.SUBRULE(this.primaryExpr);
    const result: IFilter = this.ACTION(() =>
      negated ? new NotFilter(inner) : inner,
    );
    return result;
  });

  /**
   * Primary value expression:
   *   - "*val*"  ContainsFilter
   *   - "*val"   EndsWithFilter
   *   - "val*"   StartsWithFilter
   *   - "val"    EqualsFilter
   */
  private readonly primaryExpr = this.RULE("primaryExpr", (): IFilter => {
    const field = this.currentField;
    let hasLeadingAsterisk = false;
    let hasTrailingAsterisk = false;

    this.OPTION(() => {
      this.CONSUME(Asterisk);
      this.ACTION(() => {
        hasLeadingAsterisk = true;
      });
    });

    const val = this.SUBRULE(this.valueTextChunks);

    this.OPTION2(() => {
      this.CONSUME2(Asterisk);
      this.ACTION(() => {
        hasTrailingAsterisk = true;
      });
    });

    const result: IFilter = this.ACTION(() => {
      if (hasLeadingAsterisk && hasTrailingAsterisk) {
        return new ContainsFilter(field, val);
      }
      if (hasLeadingAsterisk) {
        return new EndsWithFilter(field, val);
      }
      if (hasTrailingAsterisk) {
        return new StartsWithFilter(field, val);
      }
      return new EqualsFilter(field, val);
    });

    return result;
  });

  /**
   * Range expression: ([|]) rangeValue TO rangeValue (]|[)
   *
   * Opening "[" means inclusive lower bound; opening "]" means exclusive.
   * Closing "]" means inclusive upper bound; closing "[" means exclusive.
   * "*" as a bound value means unbounded (no filter generated for that side).
   */
  private readonly rangeExpression = this.RULE(
    "rangeExpression",
    (): IFilter => {
      const field = this.currentField;
      let openInclusive = true;

      this.OR([
        {
          ALT: () => {
            this.CONSUME(LeftSquare);
            this.ACTION(() => {
              openInclusive = true;
            });
          },
        },
        {
          ALT: () => {
            this.CONSUME(RightSquare);
            this.ACTION(() => {
              openInclusive = false;
            });
          },
        },
      ]);

      let minValue: string | undefined;
      this.OR2([
        {
          ALT: () => {
            this.CONSUME(Asterisk);
          },
        },
        {
          ALT: () => {
            minValue = this.SUBRULE(this.rangeValueChunks);
          },
        },
      ]);

      this.CONSUME(TO);

      let maxValue: string | undefined;
      this.OR3([
        {
          ALT: () => {
            this.CONSUME2(Asterisk);
          },
        },
        {
          ALT: () => {
            maxValue = this.SUBRULE2(this.rangeValueChunks);
          },
        },
      ]);

      let closeInclusive = true;
      this.OR4([
        {
          ALT: () => {
            this.CONSUME2(RightSquare);
            this.ACTION(() => {
              closeInclusive = true;
            });
          },
        },
        {
          ALT: () => {
            this.CONSUME2(LeftSquare);
            this.ACTION(() => {
              closeInclusive = false;
            });
          },
        },
      ]);

      const result: IFilter = this.ACTION(() => {
        const filters: IFilter[] = [];

        if (minValue !== undefined) {
          filters.push(
            openInclusive
              ? new GreaterThanOrEqualFilter(field, minValue)
              : new GreaterThanFilter(field, minValue),
          );
        }

        if (maxValue !== undefined) {
          filters.push(
            closeInclusive
              ? new LessThanOrEqualFilter(field, maxValue)
              : new LessThanFilter(field, maxValue),
          );
        }

        if (filters.length === 0) {
          throw new Error("Range expression must have at least one bound");
        }

        return filters.length === 1 ? filters[0] : new AndFilter(filters);
      });

      return result;
    },
  );

  /**
   * One or more text/escaped tokens for range bound values.
   * The TO keyword is intentionally excluded so it acts as the separator.
   *
   * NOTE: CONSUME must remain at the top level of each ALT body (not inside
   * ACTION) so that Chevrotain can discover the FIRST sets during the
   * grammar recording phase.
   */
  private readonly rangeValueChunks = this.RULE(
    "rangeValueChunks",
    (): string => {
      let text = "";
      this.AT_LEAST_ONE(() => {
        this.OR([
          {
            ALT: () => {
              text += tokenValue(this.CONSUME(Text));
            },
          },
          {
            ALT: () => {
              text += tokenValue(this.CONSUME(Escaped));
            },
          },
        ]);
      });
      return text;
    },
  );

  /**
   * One or more text/escaped/TO tokens for wildcard and literal values.
   * TO is permitted as literal value text (e.g. "city=TO").
   *
   * NOTE: CONSUME must remain at the top level of each ALT body (not inside
   * ACTION) so that Chevrotain can discover the FIRST sets during the
   * grammar recording phase.
   */
  private readonly valueTextChunks = this.RULE(
    "valueTextChunks",
    (): string => {
      let text = "";
      this.AT_LEAST_ONE(() => {
        this.OR([
          {
            ALT: () => {
              text += tokenValue(this.CONSUME(Text));
            },
          },
          {
            ALT: () => {
              text += tokenValue(this.CONSUME(Escaped));
            },
          },
          {
            ALT: () => {
              text += this.CONSUME(TO).image;
            },
          },
        ]);
      });
      return text;
    },
  );

  /**
   * One or more tokens forming a field name (Text, Escaped, or literal TO).
   *
   * NOTE: CONSUME must remain at the top level of each ALT body (not inside
   * ACTION) so that Chevrotain can discover the FIRST sets during the
   * grammar recording phase.
   */
  private readonly fieldNameRule = this.RULE("fieldNameRule", (): string => {
    let name = "";
    this.AT_LEAST_ONE(() => {
      this.OR([
        {
          ALT: () => {
            name += tokenValue(this.CONSUME(Text));
          },
        },
        {
          ALT: () => {
            name += tokenValue(this.CONSUME(Escaped));
          },
        },
        {
          ALT: () => {
            name += this.CONSUME(TO).image;
          },
        },
      ]);
    });
    return name;
  });

  // ---------------------------------------------------------------------------
  // Lookahead helpers
  // ---------------------------------------------------------------------------

  /**
   * Returns true when the upcoming tokens look like "fieldName =", i.e. at
   * least one Text/Escaped/TO token followed immediately by Equals.
   */
  private isFieldAssignmentStart(): boolean {
    let i = 1;
    let tt = this.LA(i).tokenType;
    let hasFieldToken = false;

    while (tt === Text || tt === TO || tt === Escaped) {
      hasFieldToken = true;
      i++;
      tt = this.LA(i).tokenType;
    }

    return hasFieldToken && tt === Equals;
  }

  // ---------------------------------------------------------------------------
  // Result assembly
  // ---------------------------------------------------------------------------

  /**
   * Combines all accumulated parsedParts into a single IFilter:
   *   - consecutive parts sharing the same field are AND-ed within a group
   *   - groups are then combined with AND or OR per options.logic
   */
  private buildResult(): IFilter {
    if (this.parsedParts.length === 0) {
      throw new Error("No filter parts parsed");
    }

    if (this.parsedParts.length === 1) {
      return this.parsedParts[0].filter;
    }

    // Group consecutive same-field parts.
    const groups: Array<{ filters: IFilter[]; field: string }> = [];
    let currentGroup: { filters: IFilter[]; field: string } | null = null;

    for (const part of this.parsedParts) {
      if (currentGroup === null || part.field !== currentGroup.field) {
        if (currentGroup !== null) {
          groups.push(currentGroup);
        }
        currentGroup = { filters: [part.filter], field: part.field };
      } else {
        currentGroup.filters.push(part.filter);
      }
    }

    if (currentGroup !== null) {
      groups.push(currentGroup);
    }

    // AND-combine filters within each group.
    const groupFilters: IFilter[] = groups.map((g) =>
      g.filters.length === 1 ? g.filters[0] : new AndFilter(g.filters),
    );

    const isSameField = this.encounteredFields.size <= 1;
    const logic = this.currentOptions?.logic ?? FilterLogic.And;

    let result: IFilter;

    if (isSameField || logic === FilterLogic.And) {
      result =
        groupFilters.length === 1
          ? groupFilters[0]
          : new AndFilter(groupFilters);
    } else if (groupFilters.length === 2) {
      result = new OrFilter(groupFilters[0], groupFilters[1]);
    } else {
      result = new OneOfFilter(groupFilters);
    }

    return result;
  }

  // ---------------------------------------------------------------------------
  // Public parsing entry point
  // ---------------------------------------------------------------------------

  /**
   * Reset all per-call state, lex the expression, run the parser and return
   * the assembled IFilter.  Throws on lexer or parser errors.
   */
  public parseExpression(
    expression: string,
    options?: FilterOptions,
  ): IFilter {
    this.currentField = "";
    this.currentOptions = options;
    this.parsedParts = [];
    this.encounteredFields = new Set();

    const lexResult = FILTER_LEXER.tokenize(expression);

    if (lexResult.errors.length > 0) {
      throw new Error(
        `Lexer error at offset ${lexResult.errors[0].offset}: ${lexResult.errors[0].message}`,
      );
    }

    this.input = lexResult.tokens;
    const result = this.filterInput();

    if (this.errors.length > 0) {
      throw new Error(`Parse error: ${this.errors[0].message}`);
    }

    return result;
  }
}

// Single shared instance — performSelfAnalysis is expensive and runs only once.
const PARSER_INSTANCE = new DataFilterParser();

/**
 * Parse a DataFilters query string into an IFilter.
 *
 * @param expression - A filter expression string, e.g. "name=*bat*" or
 *   "name=*bat*&age=[18 TO *[".
 * @param options - Optional parsing options that control how multiple
 *   criteria from different fields are combined.
 * @returns An IFilter representing the parsed expression.
 * @throws {Error} If the expression cannot be parsed.
 */
export function parse(expression: string, options?: FilterOptions): IFilter {
  if (!expression || !expression.trim()) {
    throw new Error("Expression must not be empty");
  }

  const trimmed = expression.trim();
  const normalized = normalizeExpression(trimmed);
  return PARSER_INSTANCE.parseExpression(normalized, options);
}
