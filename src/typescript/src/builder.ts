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
  public constructor(
    private readonly parent: FilterBuilder,
    private readonly field: string,
  ) {}

  /** Add an equals condition. */
  public eq(value: unknown): FilterBuilder {
    return this.parent.addFilter(new EqualsFilter(this.field, value));
  }

  /** Add a contains condition. */
  public contains(value: string): FilterBuilder {
    return this.parent.addFilter(new ContainsFilter(this.field, value));
  }

  /** Add a starts-with condition. */
  public startsWith(value: string): FilterBuilder {
    return this.parent.addFilter(new StartsWithFilter(this.field, value));
  }

  /** Add an ends-with condition. */
  public endsWith(value: string): FilterBuilder {
    return this.parent.addFilter(new EndsWithFilter(this.field, value));
  }

  /** Add a greater-than condition. */
  public gt(value: unknown): FilterBuilder {
    return this.parent.addFilter(new GreaterThanFilter(this.field, value));
  }

  /** Add a greater-than-or-equal condition. */
  public gte(value: unknown): FilterBuilder {
    return this.parent.addFilter(new GreaterThanOrEqualFilter(this.field, value));
  }

  /** Add a less-than condition. */
  public lt(value: unknown): FilterBuilder {
    return this.parent.addFilter(new LessThanFilter(this.field, value));
  }

  /** Add a less-than-or-equal condition. */
  public lte(value: unknown): FilterBuilder {
    return this.parent.addFilter(new LessThanOrEqualFilter(this.field, value));
  }

  /** Add a not-equals condition. */
  public not(value: unknown): FilterBuilder {
    return this.parent.addFilter(new NotFilter(new EqualsFilter(this.field, value)));
  }

  /** Add an OR condition matching any of the given values. */
  public or(...values: unknown[]): FilterBuilder {
    const orFilters: IFilter[] = values.map((v) => new EqualsFilter(this.field, v));
    return this.parent.addFilter(new OrFilter(orFilters));
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

  /** @internal */
  public addFilter(filter: IFilter): FilterBuilder {
    this.filters.push(filter);
    return this;
  }

  /**
   * Start a filter condition for the given field.
   * @param field - The name of the field to filter on.
   */
  public where(field: string): FieldBuilder {
    return new FieldBuilder(this, field);
  }

  /**
   * Chain an additional AND condition on a new field.
   * @param field - The name of the field for the next condition.
   */
  public andWhere(field: string): FieldBuilder {
    return new FieldBuilder(this, field);
  }

  /**
   * Construct and return the final IFilter.
   *
   * @returns A single IFilter or an AndFilterExpression combining all accumulated conditions.
   * @throws {Error} If no conditions have been added.
   */
  public build(): IFilter {
    if (this.filters.length === 0) {
      throw new Error('No filter conditions have been added.');
    }

    const result: IFilter =
      this.filters.length === 1 ? this.filters[0] : new AndFilter(this.filters);
    return result;
  }
}
