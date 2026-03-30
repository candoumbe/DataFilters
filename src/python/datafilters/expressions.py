"""Filter expression classes compatible with the DataFilters C# library."""

from abc import ABC, abstractmethod
from typing import Any, List


class IFilter(ABC):
    """Base interface for all filter expressions."""

    @abstractmethod
    def to_dict(self) -> dict:
        """Serialize the filter to a dictionary representation."""


class EqualsFilter(IFilter):
    """Matches records where a property equals a specific value."""

    def __init__(self, field: str, value: Any) -> None:
        self.field = field
        self.value = value

    def to_dict(self) -> dict:
        return {"field": self.field, "op": "eq", "value": self.value}


class ContainsFilter(IFilter):
    """Matches records where a property contains a substring."""

    def __init__(self, field: str, value: str) -> None:
        self.field = field
        self.value = value

    def to_dict(self) -> dict:
        return {"field": self.field, "op": "contains", "value": self.value}


class StartsWithFilter(IFilter):
    """Matches records where a property starts with a specific string."""

    def __init__(self, field: str, value: str) -> None:
        self.field = field
        self.value = value

    def to_dict(self) -> dict:
        return {"field": self.field, "op": "startswith", "value": self.value}


class EndsWithFilter(IFilter):
    """Matches records where a property ends with a specific string."""

    def __init__(self, field: str, value: str) -> None:
        self.field = field
        self.value = value

    def to_dict(self) -> dict:
        return {"field": self.field, "op": "endswith", "value": self.value}


class GreaterThanFilter(IFilter):
    """Matches records where a property is greater than a value."""

    def __init__(self, field: str, value: Any) -> None:
        self.field = field
        self.value = value

    def to_dict(self) -> dict:
        return {"field": self.field, "op": "gt", "value": self.value}


class GreaterThanOrEqualFilter(IFilter):
    """Matches records where a property is greater than or equal to a value."""

    def __init__(self, field: str, value: Any) -> None:
        self.field = field
        self.value = value

    def to_dict(self) -> dict:
        return {"field": self.field, "op": "gte", "value": self.value}


class LessThanFilter(IFilter):
    """Matches records where a property is less than a value."""

    def __init__(self, field: str, value: Any) -> None:
        self.field = field
        self.value = value

    def to_dict(self) -> dict:
        return {"field": self.field, "op": "lt", "value": self.value}


class LessThanOrEqualFilter(IFilter):
    """Matches records where a property is less than or equal to a value."""

    def __init__(self, field: str, value: Any) -> None:
        self.field = field
        self.value = value

    def to_dict(self) -> dict:
        return {"field": self.field, "op": "lte", "value": self.value}


class AndFilter(IFilter):
    """Combines multiple filters with a logical AND."""

    def __init__(self, filters: List[IFilter]) -> None:
        self.filters = filters

    def to_dict(self) -> dict:
        return {"logic": "and", "filters": [f.to_dict() for f in self.filters]}


class OrFilter(IFilter):
    """Combines multiple filters with a logical OR."""

    def __init__(self, filters: List[IFilter]) -> None:
        self.filters = filters

    def to_dict(self) -> dict:
        return {"logic": "or", "filters": [f.to_dict() for f in self.filters]}


class NotFilter(IFilter):
    """Negates a filter expression."""

    def __init__(self, filter: IFilter) -> None:
        self.filter = filter

    def to_dict(self) -> dict:
        return {"logic": "not", "filters": [self.filter.to_dict()]}
