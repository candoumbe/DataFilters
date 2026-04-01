/**
 * DataFilters TypeScript SDK.
 *
 * Generate filter expressions compatible with the DataFilters C# library,
 * using an Elasticsearch-inspired query syntax.
 */

export {
  IFilter,
  EqualsFilterExpression,
  ContainsFilterExpression,
  StartsWithFilterExpression,
  EndsWithFilterExpression,
  GreaterThanFilterExpression,
  GreaterThanOrEqualFilterExpression,
  LessThanFilterExpression,
  LessThanOrEqualFilterExpression,
  AndFilter,
  OrFilter,
  NotFilter,
} from './expressions';

export { FilterBuilder, FieldBuilder } from './builder';

export { parse } from './parser';

export { toDict, toJson, fromDict } from './serializers';
