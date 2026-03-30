"""Tests for the DataFilters query string parser."""

import pytest

from datafilters.expressions import (
    AndFilter,
    ContainsFilter,
    EndsWithFilter,
    EqualsFilter,
    GreaterThanFilter,
    GreaterThanOrEqualFilter,
    LessThanFilter,
    LessThanOrEqualFilter,
    NotFilter,
    OrFilter,
    StartsWithFilter,
)
from datafilters.parser import parse


class TestParseEquals:
    def test_simple_equals(self):
        result = parse("name=Batman")
        assert isinstance(result, EqualsFilter)
        assert result.field == "name"
        assert result.value == "Batman"


class TestParseContains:
    def test_contains(self):
        result = parse("name=*bat*")
        assert isinstance(result, ContainsFilter)
        assert result.field == "name"
        assert result.value == "bat"


class TestParseStartsWith:
    def test_starts_with(self):
        result = parse("name=Bat*")
        assert isinstance(result, StartsWithFilter)
        assert result.value == "Bat"


class TestParseEndsWith:
    def test_ends_with(self):
        result = parse("name=*man")
        assert isinstance(result, EndsWithFilter)
        assert result.value == "man"


class TestParseNegation:
    def test_not_equals(self):
        result = parse("name=!Batman")
        assert isinstance(result, NotFilter)
        assert isinstance(result.filter, EqualsFilter)
        assert result.filter.value == "Batman"


class TestParseRange:
    def test_closed_range(self):
        result = parse("age=[18,65]")
        assert isinstance(result, AndFilter)
        assert isinstance(result.filters[0], GreaterThanOrEqualFilter)
        assert isinstance(result.filters[1], LessThanOrEqualFilter)
        assert result.filters[0].value == "18"
        assert result.filters[1].value == "65"

    def test_open_range(self):
        result = parse("age=(18,65)")
        assert isinstance(result, AndFilter)
        assert isinstance(result.filters[0], GreaterThanFilter)
        assert isinstance(result.filters[1], LessThanFilter)

    def test_half_open_min(self):
        result = parse("age=[18,]")
        assert isinstance(result, GreaterThanOrEqualFilter)
        assert result.value == "18"

    def test_half_open_max(self):
        result = parse("age=[,65]")
        assert isinstance(result, LessThanOrEqualFilter)
        assert result.value == "65"


class TestParseAnd:
    def test_and_combination(self):
        result = parse("name=*bat*,age=[18,]")
        assert isinstance(result, AndFilter)
        assert len(result.filters) == 2


class TestParseOr:
    def test_or_combination(self):
        result = parse("status=active|status=pending")
        assert isinstance(result, OrFilter)
        assert len(result.filters) == 2


class TestParseErrors:
    def test_empty_expression_raises(self):
        with pytest.raises(ValueError):
            parse("")

    def test_whitespace_expression_raises(self):
        with pytest.raises(ValueError):
            parse("   ")

    def test_missing_field_raises(self):
        with pytest.raises(ValueError):
            parse("=value")

    def test_missing_equals_raises(self):
        with pytest.raises(ValueError):
            parse("noequals")


@pytest.mark.parametrize(
    "expression, expected_type",
    [
        ("name=Batman", EqualsFilter),
        ("name=*bat*", ContainsFilter),
        ("name=Bat*", StartsWithFilter),
        ("name=*man", EndsWithFilter),
        ("name=!Batman", NotFilter),
    ],
)
def test_parse_single_expressions(expression, expected_type):
    result = parse(expression)
    assert isinstance(result, expected_type)
