"""DataFilters Python SDK.

A library to generate filter expressions compatible with the DataFilters C# library,
using an Elasticsearch-inspired query syntax.
"""

from .builder import FilterBuilder
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
from .parser import parse
from .serializers import to_dict, to_json

__all__ = [
    "IFilter",
    "EqualsFilter",
    "ContainsFilter",
    "StartsWithFilter",
    "EndsWithFilter",
    "GreaterThanFilter",
    "GreaterThanOrEqualFilter",
    "LessThanFilter",
    "LessThanOrEqualFilter",
    "AndFilter",
    "OrFilter",
    "NotFilter",
    "FilterBuilder",
    "parse",
    "to_dict",
    "to_json",
]

__version__ = "0.1.0"
