/**
 * Fluent builder API for constructing DataFilters filter expressions.
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

/** Internal helper that provides filter-condition methods for a single field. */
export class FieldBuilder {
  constructor(
    private readonly parent: FilterBuilder,
    private readonly field: string,
  ) {}

  /** Add an equals condition. */
  eq(value: unknown): FilterBuilder {
    return this.parent['add'](new EqualsFilter(this.field, value));
  }

  /** Add a contains condition. */
  contains(value: string): FilterBuilder {
    return this.parent['add'](new ContainsFilter(this.field, value));
  }

  /** Add a starts-with condition. */
  startsWith(value: string): FilterBuilder {
    return this.parent['add'](new StartsWithFilter(this.field, value));
  }

  /** Add an ends-with condition. */
  endsWith(value: string): FilterBuilder {
    return this.parent['add'](new EndsWithFilter(this.field, value));
  }

  /** Add a greater-than condition. */
  gt(value: unknown): FilterBuilder {
    return this.parent['add'](new GreaterThanFilter(this.field, value));
  }

  /** Add a greater-than-or-equal condition. */
  gte(value: unknown): FilterBuilder {
    return this.parent['add'](new GreaterThanOrEqualFilter(this.field, value));
  }

  /** Add a less-than condition. */
  lt(value: unknown): FilterBuilder {
    return this.parent['add'](new LessThanFilter(this.field, value));
  }

  /** Add a less-than-or-equal condition. */
  lte(value: unknown): FilterBuilder {
    return this.parent['add'](new LessThanOrEqualFilter(this.field, value));
  }

  /** Add a not-equals condition. */
  not(value: unknown): FilterBuilder {
    return this.parent['add'](new NotFilter(new EqualsFilter(this.field, value)));
  }

  /** Add an OR condition matching any of the given values. */
  or(...values: unknown[]): FilterBuilder {
    const orFilters: IFilter[] = values.map((v) => new EqualsFilter(this.field, v));
    return this.parent['add'](new OrFilter(orFilters));
  }
}

/**
 * Fluent builder for constructing compound filter expressions.
 *
 * @example
 * ```ts
 * const filter = new FilterBuilder()
 *   .where('name').contains('bat')
 *   .andWhere('age').gte(18)
 *   .build();
 * ```
 */
export class FilterBuilder {
  private readonly filters: IFilter[] = [];

  private add(filter: IFilter): FilterBuilder {
    this.filters.push(filter);
    return this;
  }

  /**
   * Start a filter condition for the given field.
   * @param field - The name of the field to filter on.
   */
  where(field: string): FieldBuilder {
    return new FieldBuilder(this, field);
  }

  /**
   * Chain an additional AND condition on a new field.
   * @param field - The name of the field for the next condition.
   */
  andWhere(field: string): FieldBuilder {
    return new FieldBuilder(this, field);
  }

  /**
   * Construct and return the final IFilter.
   *
   * @returns A single IFilter or an AndFilter combining all accumulated conditions.
   * @throws {Error} If no conditions have been added.
   */
  build(): IFilter {
    if (this.filters.length === 0) {
      throw new Error('No filter conditions have been added.');
    }

    if (this.filters.length === 1) {
      return this.filters[0];
    }

    return new AndFilter(this.filters);
  }
}
