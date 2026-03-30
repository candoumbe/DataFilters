"""Parser for DataFilters query syntax strings.

Parses an Elasticsearch-inspired filter syntax string into IFilter objects.

Supported syntax examples:
  - ``field=value``       → EqualsFilter
  - ``field=value*``      → StartsWithFilter
  - ``field=*value``      → EndsWithFilter
  - ``field=*value*``     → ContainsFilter
  - ``field=!value``      → NotFilter(EqualsFilter)
  - ``field=[min,max]``   → AndFilter(gte, lte)
  - ``field=[min,]``      → GreaterThanOrEqualFilter
  - ``field=[,max]``      → LessThanOrEqualFilter
  - ``field=(min,max)``   → AndFilter(gt, lt)
  - ``expr1,expr2``       → AndFilter of multiple criteria
  - ``expr1|expr2``       → OrFilter of multiple criteria
"""

import re
from typing import List, Optional

from .expressions import (
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
)

_RANGE_PATTERN = re.compile(
    r"^(?P<open>[\[\(])(?P<min>[^,]*),(?P<max>[^)\]]*)?(?P<close>[\]\)])$"
)


def _parse_value_expression(field: str, value: str) -> IFilter:
    """Parse a single field=value expression into the appropriate IFilter."""
    negated = value.startswith("!")
    if negated:
        value = value[1:]

    range_match = _RANGE_PATTERN.match(value)
    if range_match:
        result = _parse_range(field, range_match)
    elif value.startswith("*") and value.endswith("*") and len(value) > 1:
        result = ContainsFilter(field, value[1:-1])
    elif value.startswith("*"):
        result = EndsWithFilter(field, value[1:])
    elif value.endswith("*"):
        result = StartsWithFilter(field, value[:-1])
    else:
        result = EqualsFilter(field, value)

    if negated:
        result = NotFilter(result)

    return result


def _parse_range(field: str, match: re.Match) -> IFilter:
    """Parse a range expression like [min,max], (min,max), [min,], [,max] into IFilter(s)."""
    open_bracket = match.group("open")
    close_bracket = match.group("close")
    min_val: Optional[str] = match.group("min") or None
    max_val: Optional[str] = match.group("max") or None

    filters: List[IFilter] = []

    if min_val is not None:
        if open_bracket == "[":
            filters.append(GreaterThanOrEqualFilter(field, min_val))
        else:
            filters.append(GreaterThanFilter(field, min_val))

    if max_val is not None:
        if close_bracket == "]":
            filters.append(LessThanOrEqualFilter(field, max_val))
        else:
            filters.append(LessThanFilter(field, max_val))

    if len(filters) == 1:
        return filters[0]

    return AndFilter(filters)


def parse(expression: str) -> IFilter:
    """Parse a DataFilters query string into an IFilter.

    Args:
        expression: A filter expression string, e.g. ``"name=*bat*,age=[18,]"``.

    Returns:
        An IFilter representing the parsed expression.

    Raises:
        ValueError: If the expression cannot be parsed.
    """
    if not expression or not expression.strip():
        raise ValueError("Expression must not be empty")

    expression = expression.strip()

    or_parts = expression.split("|")
    if len(or_parts) > 1:
        parsed_parts = [_parse_and_expression(part.strip()) for part in or_parts]
        return OrFilter(parsed_parts)

    return _parse_and_expression(expression)


def _split_and_parts(expression: str) -> list:
    """Split an expression on commas that are not inside brackets."""
    parts = []
    depth = 0
    current: list = []
    for char in expression:
        if char in "([":
            depth += 1
            current.append(char)
        elif char in ")]":
            depth -= 1
            current.append(char)
        elif char == "," and depth == 0:
            parts.append("".join(current))
            current = []
        else:
            current.append(char)
    if current:
        parts.append("".join(current))
    return parts


def _parse_and_expression(expression: str) -> IFilter:
    """Parse a comma-separated AND expression."""
    and_parts = _split_and_parts(expression)
    if len(and_parts) > 1:
        parsed_parts = [_parse_single(part.strip()) for part in and_parts]
        return AndFilter(parsed_parts)

    return _parse_single(expression)


def _parse_single(expression: str) -> IFilter:
    """Parse a single field=value pair."""
    if "=" not in expression:
        raise ValueError(f"Invalid filter expression: '{expression}'. Expected 'field=value'.")

    field, _, value = expression.partition("=")
    field = field.strip()
    value = value.strip()

    if not field:
        raise ValueError(f"Field name must not be empty in expression: '{expression}'")

    return _parse_value_expression(field, value)
