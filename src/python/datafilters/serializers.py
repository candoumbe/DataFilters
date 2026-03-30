"""Serialization utilities for DataFilters expressions."""

import json

from .expressions import IFilter


def to_dict(filter: IFilter) -> dict:
    """Serialize an IFilter to a plain Python dictionary.

    Args:
        filter: The filter expression to serialize.

    Returns:
        A dictionary representation of the filter.
    """
    return filter.to_dict()


def to_json(filter: IFilter, **kwargs) -> str:
    """Serialize an IFilter to a JSON string.

    Args:
        filter: The filter expression to serialize.
        **kwargs: Additional keyword arguments forwarded to ``json.dumps``.

    Returns:
        A JSON string representation of the filter.
    """
    return json.dumps(to_dict(filter), **kwargs)


def from_dict(data: dict) -> IFilter:
    """Deserialize a dictionary into an IFilter.

    Args:
        data: A dictionary produced by :func:`to_dict`.

    Returns:
        The reconstructed IFilter.

    Raises:
        ValueError: If the dictionary does not represent a known filter type.
    """
    from .expressions import (  # noqa: PLC0415 (local import to avoid circular refs)
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

    if "logic" in data:
        logic = data["logic"]
        children = [from_dict(f) for f in data.get("filters", [])]

        if logic == "and":
            result = AndFilter(children)
        elif logic == "or":
            result = OrFilter(children)
        elif logic == "not":
            result = NotFilter(children[0])
        else:
            raise ValueError(f"Unknown logic operator: '{logic}'")

        return result

    op_map = {
        "eq": EqualsFilter,
        "contains": ContainsFilter,
        "startswith": StartsWithFilter,
        "endswith": EndsWithFilter,
        "gt": GreaterThanFilter,
        "gte": GreaterThanOrEqualFilter,
        "lt": LessThanFilter,
        "lte": LessThanOrEqualFilter,
    }

    op = data.get("op")
    if op not in op_map:
        raise ValueError(f"Unknown filter operator: '{op}'")

    return op_map[op](data["field"], data["value"])
