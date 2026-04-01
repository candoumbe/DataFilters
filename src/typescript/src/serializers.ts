/**
 * Serialization utilities for DataFilters expressions.
 */

import {
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
  eq: EqualsFilter,
  contains: ContainsFilter as new (field: string, value: unknown) => IFilter,
  startswith: StartsWithFilter as new (field: string, value: unknown) => IFilter,
  endswith: EndsWithFilter as new (field: string, value: unknown) => IFilter,
  gt: GreaterThanFilter,
  gte: GreaterThanOrEqualFilter,
  lt: LessThanFilter,
  lte: LessThanOrEqualFilter,
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
    const logic = data['logic'];
    if (typeof logic !== 'string') {
      throw new Error(`Invalid logic type: expected string, got ${typeof logic}`);
    }

    const filtersData = data['filters'];
    if (!Array.isArray(filtersData)) {
      throw new Error(`Invalid filters type: expected array, got ${typeof filtersData}`);
    }

    const children = filtersData.map(fromDict);

    if (logic === 'and') {
      result = new AndFilter(children);
    } else if (logic === 'or') {
      result = new OrFilter(children);
    } else if (logic === 'not') {
      if (children.length === 0) {
        throw new Error('Not filter requires at least one child filter');
      }
      result = new NotFilter(children[0]);
    } else {
      throw new Error(`Unknown logic operator: '${logic}'`);
    }
  } else {
    const op = data['op'];
    if (typeof op !== 'string') {
      throw new Error(`Invalid op type: expected string, got ${typeof op}`);
    }

    const Ctor = OP_MAP[op];
    if (!Ctor) {
      throw new Error(`Unknown filter operator: '${op}'`);
    }

    const field = data['field'];
    if (typeof field !== 'string') {
      throw new Error(`Invalid field type: expected string, got ${typeof field}`);
    }

    result = new Ctor(field, data['value']);
  }

  return result;
}
