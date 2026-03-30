"""Tests for filter expression classes."""

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


class TestEqualsFilter:
    def test_to_dict(self):
        f = EqualsFilter("name", "Batman")
        assert f.to_dict() == {"field": "name", "op": "eq", "value": "Batman"}

    def test_field_and_value(self):
        f = EqualsFilter("age", 30)
        assert f.field == "age"
        assert f.value == 30


class TestContainsFilter:
    def test_to_dict(self):
        f = ContainsFilter("name", "bat")
        assert f.to_dict() == {"field": "name", "op": "contains", "value": "bat"}


class TestStartsWithFilter:
    def test_to_dict(self):
        f = StartsWithFilter("name", "Bat")
        assert f.to_dict() == {"field": "name", "op": "startswith", "value": "Bat"}


class TestEndsWithFilter:
    def test_to_dict(self):
        f = EndsWithFilter("name", "man")
        assert f.to_dict() == {"field": "name", "op": "endswith", "value": "man"}


class TestGreaterThanFilter:
    def test_to_dict(self):
        f = GreaterThanFilter("age", 18)
        assert f.to_dict() == {"field": "age", "op": "gt", "value": 18}


class TestGreaterThanOrEqualFilter:
    def test_to_dict(self):
        f = GreaterThanOrEqualFilter("age", 18)
        assert f.to_dict() == {"field": "age", "op": "gte", "value": 18}


class TestLessThanFilter:
    def test_to_dict(self):
        f = LessThanFilter("age", 65)
        assert f.to_dict() == {"field": "age", "op": "lt", "value": 65}


class TestLessThanOrEqualFilter:
    def test_to_dict(self):
        f = LessThanOrEqualFilter("age", 65)
        assert f.to_dict() == {"field": "age", "op": "lte", "value": 65}


class TestAndFilter:
    def test_to_dict(self):
        f = AndFilter([EqualsFilter("a", 1), EqualsFilter("b", 2)])
        result = f.to_dict()
        assert result["logic"] == "and"
        assert len(result["filters"]) == 2

    def test_nested(self):
        inner = AndFilter([GreaterThanOrEqualFilter("age", 18), LessThanOrEqualFilter("age", 65)])
        result = inner.to_dict()
        assert result["logic"] == "and"
        assert result["filters"][0]["op"] == "gte"
        assert result["filters"][1]["op"] == "lte"


class TestOrFilter:
    def test_to_dict(self):
        f = OrFilter([EqualsFilter("status", "active"), EqualsFilter("status", "pending")])
        result = f.to_dict()
        assert result["logic"] == "or"
        assert len(result["filters"]) == 2


class TestNotFilter:
    def test_to_dict(self):
        f = NotFilter(EqualsFilter("deleted", True))
        result = f.to_dict()
        assert result["logic"] == "not"
        assert len(result["filters"]) == 1
        assert result["filters"][0]["op"] == "eq"

    def test_wraps_filter(self):
        inner = EqualsFilter("active", False)
        f = NotFilter(inner)
        assert f.filter is inner


@pytest.mark.parametrize(
    "filter_instance, expected_op",
    [
        (EqualsFilter("f", "v"), "eq"),
        (ContainsFilter("f", "v"), "contains"),
        (StartsWithFilter("f", "v"), "startswith"),
        (EndsWithFilter("f", "v"), "endswith"),
        (GreaterThanFilter("f", 0), "gt"),
        (GreaterThanOrEqualFilter("f", 0), "gte"),
        (LessThanFilter("f", 0), "lt"),
        (LessThanOrEqualFilter("f", 0), "lte"),
    ],
)
def test_filter_operators(filter_instance, expected_op):
    assert filter_instance.to_dict()["op"] == expected_op
