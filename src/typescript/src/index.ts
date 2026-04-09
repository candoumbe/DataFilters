/**
 * DataFilters TypeScript SDK.
 *
 * Generate filter expressions compatible with the DataFilters C# library,
 * using an Elasticsearch-inspired query syntax.
 */

export {
    IFilter,
    EqualsFilter,
    ContainsFilter,
    StartsWithFilter,
    EndsWithFilter,
    GreaterThanFilter,
    GreaterThanOrEqualFilter,
    LessThanFilter,
    LessThanOrEqualFilter,
    AndFilter,
    OrFilter,
    NotFilter,
} from "./expressions";

export { FilterBuilder, FieldBuilder } from "./builder";

export { parse } from "./parser";

export { FilterLogic, FilterOptions } from "./filterOptions";

export { toDict, toJson, fromDict } from "./serializers";
