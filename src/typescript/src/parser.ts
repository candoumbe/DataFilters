/**
 * Parser for DataFilters query syntax strings.
 *
 * Supported syntax:
 *   - `field=value`       → EqualsFilter
 *   - `field=value*`      → StartsWithFilter
 *   - `field=*value`      → EndsWithFilter
 *   - `field=*value*`     → ContainsFilter
 *   - `field=!value`      → NotFilter(EqualsFilter)
 *   - `field=[min,max]`   → AndFilter(gte, lte)
 *   - `field=[min,]`      → GreaterThanOrEqualFilter
 *   - `field=[,max]`      → LessThanOrEqualFilter
 *   - `field=(min,max)`   → AndFilter(gt, lt)
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
} from './expressions';

const RANGE_PATTERN = /^(?<open>[[(])(?<min>[^,]*),(?<max>[^)\]]*)?(?<close>[)\]])$/;

function parseRange(field: string, value: string): IFilter {
  const match = RANGE_PATTERN.exec(value);
  if (!match || !match.groups) {
    throw new Error(`Invalid range expression: '${value}'`);
  }

  const openBracket = match.groups['open'];
  const closeBracket = match.groups['close'];
  const minVal = match.groups['min'] || null;
  const maxVal = match.groups['max'] || null;

  const filters: IFilter[] = [];

  if (minVal !== null && minVal !== '') {
    if (openBracket === '[') {
      filters.push(new GreaterThanOrEqualFilter(field, minVal));
    } else {
      filters.push(new GreaterThanFilter(field, minVal));
    }
  }

  if (maxVal !== null && maxVal !== '') {
    if (closeBracket === ']') {
      filters.push(new LessThanOrEqualFilter(field, maxVal));
    } else {
      filters.push(new LessThanFilter(field, maxVal));
    }
  }

  if (filters.length === 1) {
    return filters[0];
  }

  return new AndFilter(filters);
}

function parseValueExpression(field: string, value: string): IFilter {
  const negated = value.startsWith('!');
  const actualValue = negated ? value.slice(1) : value;

  let result: IFilter;

  if (RANGE_PATTERN.test(actualValue)) {
    result = parseRange(field, actualValue);
  } else if (actualValue.startsWith('*') && actualValue.endsWith('*') && actualValue.length > 1) {
    result = new ContainsFilter(field, actualValue.slice(1, -1));
  } else if (actualValue.startsWith('*')) {
    result = new EndsWithFilter(field, actualValue.slice(1));
  } else if (actualValue.endsWith('*')) {
    result = new StartsWithFilter(field, actualValue.slice(0, -1));
  } else {
    result = new EqualsFilter(field, actualValue);
  }

  return negated ? new NotFilter(result) : result;
}

function parseSingle(expression: string): IFilter {
  const eqIndex = expression.indexOf('=');
  if (eqIndex === -1) {
    throw new Error(`Invalid filter expression: '${expression}'. Expected 'field=value'.`);
  }

  const field = expression.slice(0, eqIndex).trim();
  const value = expression.slice(eqIndex + 1).trim();

  if (!field) {
    throw new Error(`Field name must not be empty in expression: '${expression}'`);
  }

  return parseValueExpression(field, value);
}

function splitAndParts(expression: string): string[] {
  const parts: string[] = [];
  let depth = 0;
  let current = '';

  for (const char of expression) {
    if (char === '(' || char === '[') {
      depth++;
      current += char;
    } else if (char === ')' || char === ']') {
      depth--;
      current += char;
    } else if (char === ',' && depth === 0) {
      parts.push(current);
      current = '';
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
  if (parts.length > 1) {
    return new AndFilter(parts.map((p) => parseSingle(p.trim())));
  }

  return parseSingle(expression);
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
    throw new Error('Expression must not be empty');
  }

  const trimmed = expression.trim();
  const orParts = trimmed.split('|');

  if (orParts.length > 1) {
    return new OrFilter(orParts.map((p) => parseAndExpression(p.trim())));
  }

  return parseAndExpression(trimmed);
}
