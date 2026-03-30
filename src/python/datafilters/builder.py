"""Fluent builder API for constructing DataFilters filter expressions."""

from typing import Any, List

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


class FilterBuilder:
    """Fluent builder for constructing compound filter expressions.

    Example usage::

        builder = FilterBuilder()
        f = (builder
             .where("name").contains("bat")
             .and_where("age").gte(18)
             .build())
    """

    def __init__(self) -> None:
        self._filters: List[IFilter] = []

    def where(self, field: str) -> "_FieldBuilder":
        """Start a filter condition for the given field.

        Args:
            field: The name of the field to filter on.

        Returns:
            A ``_FieldBuilder`` scoped to *field*.
        """
        return _FieldBuilder(self, field)

    def _add(self, filter: IFilter) -> "FilterBuilder":
        """Append a filter to the current list."""
        self._filters.append(filter)
        return self

    def and_where(self, field: str) -> "_FieldBuilder":
        """Chain an additional AND condition on a new field.

        Args:
            field: The name of the field for the next condition.

        Returns:
            A ``_FieldBuilder`` scoped to *field*.
        """
        return _FieldBuilder(self, field)

    def build(self) -> IFilter:
        """Construct and return the final IFilter.

        Returns:
            A single IFilter or an AndFilter combining all accumulated conditions.

        Raises:
            ValueError: If no conditions have been added.
        """
        if not self._filters:
            raise ValueError("No filter conditions have been added.")

        if len(self._filters) == 1:
            result = self._filters[0]
        else:
            result = AndFilter(self._filters)

        return result


class _FieldBuilder:
    """Internal helper that provides filter-condition methods for a single field."""

    def __init__(self, parent: FilterBuilder, field: str) -> None:
        self._parent = parent
        self._field = field

    def eq(self, value: Any) -> FilterBuilder:
        """Add an equals condition."""
        return self._parent._add(EqualsFilter(self._field, value))

    def contains(self, value: str) -> FilterBuilder:
        """Add a contains condition."""
        return self._parent._add(ContainsFilter(self._field, value))

    def starts_with(self, value: str) -> FilterBuilder:
        """Add a starts-with condition."""
        return self._parent._add(StartsWithFilter(self._field, value))

    def ends_with(self, value: str) -> FilterBuilder:
        """Add an ends-with condition."""
        return self._parent._add(EndsWithFilter(self._field, value))

    def gt(self, value: Any) -> FilterBuilder:
        """Add a greater-than condition."""
        return self._parent._add(GreaterThanFilter(self._field, value))

    def gte(self, value: Any) -> FilterBuilder:
        """Add a greater-than-or-equal condition."""
        return self._parent._add(GreaterThanOrEqualFilter(self._field, value))

    def lt(self, value: Any) -> FilterBuilder:
        """Add a less-than condition."""
        return self._parent._add(LessThanFilter(self._field, value))

    def lte(self, value: Any) -> FilterBuilder:
        """Add a less-than-or-equal condition."""
        return self._parent._add(LessThanOrEqualFilter(self._field, value))

    def not_(self, value: Any) -> FilterBuilder:
        """Add a not-equals condition."""
        return self._parent._add(NotFilter(EqualsFilter(self._field, value)))

    def or_(self, *values: Any) -> FilterBuilder:
        """Add an OR condition matching any of the given values."""
        or_filters: List[IFilter] = [EqualsFilter(self._field, v) for v in values]
        return self._parent._add(OrFilter(or_filters))
