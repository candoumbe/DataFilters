/**
 * Fluent builder API for constructing DataFilters filter expressions.
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

/** Internal helper that provides filter-condition methods for a single field. */
export class FieldBuilder {
  public constructor(
    private readonly parent: FilterBuilder,
    private readonly field: string,
  ) {}

  /** Add an equals condition. */
  public eq(value: unknown): FilterBuilder {
    return this.parent['add'](new EqualsFilterExpression(this.field, value));
  }

  /** Add a contains condition. */
  public contains(value: string): FilterBuilder {
    return this.parent['add'](new ContainsFilterExpression(this.field, value));
  }

  /** Add a starts-with condition. */
  public startsWith(value: string): FilterBuilder {
    return this.parent['add'](new StartsWithFilterExpression(this.field, value));
  }

  /** Add an ends-with condition. */
  public endsWith(value: string): FilterBuilder {
    return this.parent['add'](new EndsWithFilterExpression(this.field, value));
  }

  /** Add a greater-than condition. */
  public gt(value: unknown): FilterBuilder {
    return this.parent['add'](new GreaterThanFilterExpression(this.field, value));
  }

  /** Add a greater-than-or-equal condition. */
  public gte(value: unknown): FilterBuilder {
    return this.parent['add'](new GreaterThanOrEqualFilterExpression(this.field, value));
  }

  /** Add a less-than condition. */
  public lt(value: unknown): FilterBuilder {
    return this.parent['add'](new LessThanFilterExpression(this.field, value));
  }

  /** Add a less-than-or-equal condition. */
  public lte(value: unknown): FilterBuilder {
    return this.parent['add'](new LessThanOrEqualFilterExpression(this.field, value));
  }

  /** Add a not-equals condition. */
  public not(value: unknown): FilterBuilder {
    return this.parent['add'](new NotFilterExpression(new EqualsFilterExpression(this.field, value)));
  }

  /** Add an OR condition matching any of the given values. */
  public or(...values: unknown[]): FilterBuilder {
    const orFilters: IFilter[] = values.map((v) => new EqualsFilterExpression(this.field, v));
    return this.parent['add'](new OrFilterExpression(orFilters));
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
      this.filters.length === 1 ? this.filters[0] : new AndFilterExpression(this.filters);
    return result;
  }
}
