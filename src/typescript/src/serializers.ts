/**
 * Serialization utilities for DataFilters expressions.
 */

import {
  AndFilterExpression,
  ContainsFilterExpression,
  EndsWithFilterExpression,
  EqualsFilterExpression,
  GreaterThanFilterExpression,
  GreaterThanOrEqualFilterExpression,
  IFilter,
  LessThanFilterExpression,
  LessThanOrEqualFilterExpression,
  NotFilterExpression,
  OrFilterExpression,
  StartsWithFilterExpression,
} from './expressions';

/** Serialize an IFilter to a plain JavaScript object. */
export function toDict(filter: IFilter): Record<string, unknown> {
  return filter.toDict();
}

/** Serialize an IFilter to a JSON string. */
export function toJson(filter: IFilter, space?: number): string {
  return JSON.stringify(toDict(filter), null, space);
}

const OP_MAP: Record<string, new (field: string, value: unknown) => IFilter> = {
  eq: EqualsFilterExpression,
  contains: ContainsFilterExpression as new (field: string, value: unknown) => IFilter,
  startswith: StartsWithFilterExpression as new (field: string, value: unknown) => IFilter,
  endswith: EndsWithFilterExpression as new (field: string, value: unknown) => IFilter,
  gt: GreaterThanFilterExpression,
  gte: GreaterThanOrEqualFilterExpression,
  lt: LessThanFilterExpression,
  lte: LessThanOrEqualFilterExpression,
};

/**
 * Deserialize a plain object into an IFilter.
 *
 * @param data - An object produced by {@link toDict}.
 * @returns The reconstructed IFilter.
 * @throws {Error} If the object does not represent a known filter type.
 */
export function fromDict(data: Record<string, unknown>): IFilter {
  let result: IFilter;

  if ('logic' in data) {
    const logic = data['logic'] as string;
    const children = (data['filters'] as Record<string, unknown>[]).map(fromDict);

    if (logic === 'and') {
      result = new AndFilterExpression(children);
    } else if (logic === 'or') {
      result = new OrFilterExpression(children);
    } else if (logic === 'not') {
      result = new NotFilterExpression(children[0]);
    } else {
      throw new Error(`Unknown logic operator: '${logic}'`);
    }
  } else {
    const op = data['op'] as string;
    const Ctor = OP_MAP[op];

    if (!Ctor) {
      throw new Error(`Unknown filter operator: '${op}'`);
    }

    result = new Ctor(data['field'] as string, data['value']);
  }

  return result;
}
