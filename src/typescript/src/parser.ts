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
 *   - `expr1,expr2`       → AndFilter of multiple criteria
 *   - `expr1|expr2`       → OrFilter of multiple criteria
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

const RANGE_PATTERN =
    /^(?<open>[[\]])(?<min>.*?)\s*TO\s*(?<max>.*?)(?<close>[[\]])$/;
const WILDCARD_CHAR = "*";
const NEGATION_CHAR = "!";
const MIN_WILDCARD_LENGTH = 2;

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
    const trimmedMin = minVal ? minVal.trim() : "";
    const trimmedMax = maxVal ? maxVal.trim() : "";
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

    let result: IFilter;

    if (RANGE_PATTERN.test(actualValue)) {
        result = parseRange(field, actualValue);
    } else if (
        actualValue.startsWith(WILDCARD_CHAR) &&
        actualValue.endsWith(WILDCARD_CHAR) &&
        actualValue.length > MIN_WILDCARD_LENGTH
    ) {
        result = new ContainsFilter(field, actualValue.slice(1, -1));
    } else if (actualValue.startsWith(WILDCARD_CHAR)) {
        result = new EndsWithFilter(field, actualValue.slice(1));
    } else if (actualValue.endsWith(WILDCARD_CHAR)) {
        result = new StartsWithFilter(field, actualValue.slice(0, -1));
    } else {
        result = new EqualsFilter(field, actualValue);
    }

    return negated ? new NotFilter(result) : result;
}

function parseSingle(expression: string): IFilter {
    const eqIndex = expression.indexOf("=");
    if (eqIndex === -1) {
        throw new Error(
            `Invalid filter expression: '${expression}'. Expected 'field=value'.`,
        );
    }

    const field = expression.slice(0, eqIndex).trim();
    const value = expression.slice(eqIndex + 1).trim();

    if (!field) {
        throw new Error(
            `Field name must not be empty in expression: '${expression}'`,
        );
    }

    const orParts = value.split("|");
    const result: IFilter =
        orParts.length > 1
            ? new OrFilter(
                  orParts.map((v) => parseValueExpression(field, v.trim())),
              )
            : parseValueExpression(field, value);
    return result;
}

function splitAndParts(expression: string): string[] {
    const parts: string[] = [];
    let depth = 0;
    let current = "";

    for (const char of expression) {
        if (char === "(" || char === "[") {
            depth++;
            current += char;
        } else if (char === ")" || char === "]") {
            depth--;
            current += char;
        } else if (char === "," && depth === 0) {
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

function parseAndExpression(expression: string): IFilter {
    const parts = splitAndParts(expression);
    const result: IFilter =
        parts.length > 1
            ? new AndFilter(parts.map((p) => parseSingle(p.trim())))
            : parseSingle(expression);
    return result;
}

/**
 * Parse a DataFilters query string into an IFilter.
 *
 * @param expression - A filter expression string, e.g. `"name=*bat*,age=[18,]"`.
 * @returns An IFilter representing the parsed expression.
 * @throws {Error} If the expression cannot be parsed.
 */
export function parse(expression: string): IFilter {
    if (!expression || !expression.trim()) {
        throw new Error("Expression must not be empty");
    }

    const trimmed = expression.trim();
    return parseAndExpression(trimmed);
}
