"""Tests for the FilterBuilder fluent API."""

import pytest

from datafilters.builder import FilterBuilder
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


class TestFilterBuilderSingleCondition:
    def test_eq(self):
        result = FilterBuilder().where("name").eq("Batman").build()
        assert isinstance(result, EqualsFilter)
        assert result.field == "name"
        assert result.value == "Batman"

    def test_contains(self):
        result = FilterBuilder().where("name").contains("bat").build()
        assert isinstance(result, ContainsFilter)
        assert result.value == "bat"

    def test_starts_with(self):
        result = FilterBuilder().where("name").starts_with("Bat").build()
        assert isinstance(result, StartsWithFilter)

    def test_ends_with(self):
        result = FilterBuilder().where("name").ends_with("man").build()
        assert isinstance(result, EndsWithFilter)

    def test_gt(self):
        result = FilterBuilder().where("age").gt(18).build()
        assert isinstance(result, GreaterThanFilter)
        assert result.value == 18

    def test_gte(self):
        result = FilterBuilder().where("age").gte(18).build()
        assert isinstance(result, GreaterThanOrEqualFilter)

    def test_lt(self):
        result = FilterBuilder().where("age").lt(65).build()
        assert isinstance(result, LessThanFilter)

    def test_lte(self):
        result = FilterBuilder().where("age").lte(65).build()
        assert isinstance(result, LessThanOrEqualFilter)

    def test_not(self):
        result = FilterBuilder().where("deleted").not_(True).build()
        assert isinstance(result, NotFilter)
        assert isinstance(result.filter, EqualsFilter)


class TestFilterBuilderMultipleConditions:
    def test_two_conditions_produce_and_filter(self):
        result = (
            FilterBuilder()
            .where("name").contains("bat")
            .and_where("age").gte(18)
            .build()
        )
        assert isinstance(result, AndFilter)
        assert len(result.filters) == 2

    def test_or_condition(self):
        result = FilterBuilder().where("status").or_("active", "pending").build()
        assert isinstance(result, OrFilter)
        assert len(result.filters) == 2


class TestFilterBuilderErrors:
    def test_build_without_conditions_raises(self):
        with pytest.raises(ValueError):
            FilterBuilder().build()
