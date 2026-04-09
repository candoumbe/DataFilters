/**
 * Parser for DataFilters query syntax strings.
 *
 * Supported syntax:
 *   - `field=value`       → EqualsFilter
 *   - `field=value*`      → StartsWithFilter
 *   - `field=*value`      → EndsWithFilter
 *   - `field=*value*`     → ContainsFilter
 *   - `field=!value`      → NotFilter(EqualsFilter)
 *   - `field=[min TO max]`  → AndFilter(gte, lte)
 *   - `field=[min TO *[`    → GreaterThanOrEqualFilter
 *   - `field=]* TO max]`    → LessThanOrEqualFilter
 *   - `field=]min TO max[`  → AndFilter(gt, lt)
 *   - `field=expr1,expr2`       → AndFilter(expr1, expr2)
 *   - `field=expr1|expr2`       → OrFilter(expr1, expr2)
 */

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
  OrFilter,
  StartsWithFilter,
} from "./expressions";
import { FilterLogic, FilterOptions } from "./filterOptions";

const RANGE_PATTERN =
  /^(?<open>[[\]])(?<min>.*?)\s*TO\s*(?<max>.*?)(?<close>[[\]])$/;
const WILDCARD_CHAR = "*";
const NEGATION_CHAR = "!";
const ESCAPE_CHAR = "\\";
const MIN_WILDCARD_LENGTH = 2;

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

function unescapeValue(value: string): string {
  let result = "";

  for (let i = 0; i < value.length; i++) {
    const char = value[i];

    if (char === ESCAPE_CHAR) {
      const nextChar = value[i + 1];
      if (nextChar !== undefined) {
        result += nextChar;
        i++;
      } else {
        result += char;
      }
    } else {
      result += char;
    }
  }

  return result;
}

function isEscaped(value: string, index: number): boolean {
  let backslashCount = 0;
  let cursor = index - 1;

  while (cursor >= 0 && value[cursor] === ESCAPE_CHAR) {
    backslashCount++;
    cursor--;
  }

  return backslashCount % 2 === 1;
}

function findFirstUnescaped(value: string, target: string): number {
  let escaped = false;
  let result = -1;

  for (let i = 0; i < value.length && result === -1; i++) {
    const char = value[i];

    if (escaped) {
      escaped = false;
    } else if (char === ESCAPE_CHAR) {
      escaped = true;
    } else if (char === target) {
      result = i;
    }
  }

  return result;
}

function splitByUnescapedSeparator(
  expression: string,
  separator: string,
): string[] {
  const parts: string[] = [];
  let depth = 0;
  let escaped = false;
  let current = "";

  for (const char of expression) {
    if (escaped) {
      current += char;
      escaped = false;
    } else if (char === ESCAPE_CHAR) {
      current += char;
      escaped = true;
    } else if (char === "(" || char === "[") {
      depth++;
      current += char;
    } else if (char === ")" || char === "]") {
      depth--;
      current += char;
    } else if (char === separator && depth === 0) {
      parts.push(current);
      current = "";
    } else {
      current += char;
    }
  }

  if (current) {
    parts.push(current);
  }

  return parts;
}

function hasLeadingWildcard(value: string): boolean {
  return value.startsWith(WILDCARD_CHAR);
}

function hasTrailingWildcard(value: string): boolean {
  const lastIndex = value.length - 1;
  const hasWildcard = value.endsWith(WILDCARD_CHAR);
  let result = false;

  if (hasWildcard && lastIndex >= 0) {
    result = !isEscaped(value, lastIndex);
  }

  return result;
}

function normalizeExpression(expression: string): string {
  const withoutPrefix = expression.startsWith("?")
    ? expression.slice(1)
    : expression;
  const queryParts = splitByUnescapedSeparator(withoutPrefix, "&")
    .map((part) => part.trim())
    .filter((part) => part.length > 0);

  let result: string;

  if (queryParts.length <= 1) {
    result = decodeQueryComponent(withoutPrefix);
  } else {
    const normalizedParts = queryParts.map((part) => {
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

function parseRange(field: string, value: string): IFilter {
  const match = RANGE_PATTERN.exec(value);
  if (!match || !match.groups) {
    throw new Error(`Invalid range expression: '${value}'`);
  }

  const openBracket = match.groups["open"];
  const closeBracket = match.groups["close"];
  const minVal = match.groups["min"];
  const maxVal = match.groups["max"];

  const filters: IFilter[] = [];
  const trimmedMin = minVal?.trim() ?? "";
  const trimmedMax = maxVal?.trim() ?? "";
  const hasMin = trimmedMin !== "" && trimmedMin !== WILDCARD_CHAR;
  const hasMax = trimmedMax !== "" && trimmedMax !== WILDCARD_CHAR;

  if (hasMin) {
    if (openBracket === "[") {
      filters.push(new GreaterThanOrEqualFilter(field, trimmedMin));
    } else {
      filters.push(new GreaterThanFilter(field, trimmedMin));
    }
  }

  if (hasMax) {
    if (closeBracket === "]") {
      filters.push(new LessThanOrEqualFilter(field, trimmedMax));
    } else {
      filters.push(new LessThanFilter(field, trimmedMax));
    }
  }

  if (filters.length === 0) {
    throw new Error(
      `Range expression must have at least one bound: '${value}'`,
    );
  }

  const result: IFilter =
    filters.length === 1 ? filters[0] : new AndFilter(filters);
  return result;
}

function parseValueExpression(field: string, value: string): IFilter {
  const negated = value.startsWith(NEGATION_CHAR);
  const actualValue = negated ? value.slice(1) : value;
  const startsWithWildcard = hasLeadingWildcard(actualValue);
  const endsWithWildcard = hasTrailingWildcard(actualValue);

  let result: IFilter;

  if (RANGE_PATTERN.test(actualValue)) {
    result = parseRange(field, actualValue);
  } else if (
    startsWithWildcard &&
    endsWithWildcard &&
    actualValue.length > MIN_WILDCARD_LENGTH
  ) {
    result = new ContainsFilter(field, unescapeValue(actualValue.slice(1, -1)));
  } else if (startsWithWildcard) {
    result = new EndsWithFilter(field, unescapeValue(actualValue.slice(1)));
  } else if (endsWithWildcard) {
    result = new StartsWithFilter(
      field,
      unescapeValue(actualValue.slice(0, -1)),
    );
  } else {
    result = new EqualsFilter(field, unescapeValue(actualValue));
  }

  return negated ? new NotFilter(result) : result;
}

function parseSingle(expression: string): IFilter {
  const eqIndex = findFirstUnescaped(expression, "=");
  if (eqIndex === -1) {
    throw new Error(
      `Invalid filter expression: '${expression}'. Expected 'field=value'.`,
    );
  }

  const field = unescapeValue(expression.slice(0, eqIndex).trim());
  const value = expression.slice(eqIndex + 1).trim();

  if (!field) {
    throw new Error(
      `Field name must not be empty in expression: '${expression}'`,
    );
  }

  const orParts = splitByUnescapedSeparator(value, "|");
  const result: IFilter =
    orParts.length > 1
      ? new OrFilter(orParts.map((v) => parseValueExpression(field, v.trim())))
      : parseValueExpression(field, value);
  return result;
}

function splitAndParts(expression: string): string[] {
  return splitByUnescapedSeparator(expression, ",");
}

function parseAndExpression(
  expression: string,
  options?: FilterOptions,
): IFilter {
  const parts = splitAndParts(expression);
  let result: IFilter;

  if (parts.length <= 1) {
    result = parseSingle(expression);
  } else {
    let inheritedField: string | null = null;
    const filters: IFilter[] = parts.map((p) => {
      const trimmed = p.trim();
      const eqIndex = findFirstUnescaped(trimmed, "=");
      let parsedFilter: IFilter;

      if (eqIndex !== -1) {
        inheritedField = trimmed.slice(0, eqIndex).trim();
        parsedFilter = parseSingle(trimmed);
      } else {
        if (!inheritedField) {
          throw new Error(
            `Cannot determine field for expression part: '${trimmed}'`,
          );
        }

        parsedFilter = parseValueExpression(inheritedField, trimmed);
      }

      return parsedFilter;
    });

    const logic = options?.logic ?? FilterLogic.And;
    result =
      logic === FilterLogic.Or ? new OrFilter(filters) : new AndFilter(filters);
  }

  return result;
}

/**
 * Parse a DataFilters query string into an IFilter.
 *
 * @param expression - A filter expression string, e.g. `"name=*bat*` or `"name=*bat*&age=[18 TO *["`.
 * @param options - Optional parsing options that control how multiple criteria are combined.
 * @returns An IFilter representing the parsed expression.
 * @throws {Error} If the expression cannot be parsed.
 */
export function parse(expression: string, options?: FilterOptions): IFilter {
  if (!expression || !expression.trim()) {
    throw new Error("Expression must not be empty");
  }

  const trimmed = expression.trim();
  const normalizedExpression = normalizeExpression(trimmed);
  return parseAndExpression(normalizedExpression, options);
}
