/**
 * Filter expression types compatible with the DataFilters C# library.
 */

/** Base interface for all filter expressions. */
export interface IFilter {
  toDict(): Record<string, unknown>;
}

/** Matches records where a property equals a specific value. */
export class EqualsFilter implements IFilter {
  constructor(public readonly field: string, public readonly value: unknown) {}

  toDict(): Record<string, unknown> {
    return { field: this.field, op: 'eq', value: this.value };
  }
}

/** Matches records where a property contains a substring. */
export class ContainsFilter implements IFilter {
  constructor(public readonly field: string, public readonly value: string) {}

  toDict(): Record<string, unknown> {
    return { field: this.field, op: 'contains', value: this.value };
  }
}

/** Matches records where a property starts with a specific string. */
export class StartsWithFilter implements IFilter {
  constructor(public readonly field: string, public readonly value: string) {}

  toDict(): Record<string, unknown> {
    return { field: this.field, op: 'startswith', value: this.value };
  }
}

/** Matches records where a property ends with a specific string. */
export class EndsWithFilter implements IFilter {
  constructor(public readonly field: string, public readonly value: string) {}

  toDict(): Record<string, unknown> {
    return { field: this.field, op: 'endswith', value: this.value };
  }
}

/** Matches records where a property is greater than a value. */
export class GreaterThanFilter implements IFilter {
  constructor(public readonly field: string, public readonly value: unknown) {}

  toDict(): Record<string, unknown> {
    return { field: this.field, op: 'gt', value: this.value };
  }
}

/** Matches records where a property is greater than or equal to a value. */
export class GreaterThanOrEqualFilter implements IFilter {
  constructor(public readonly field: string, public readonly value: unknown) {}

  toDict(): Record<string, unknown> {
    return { field: this.field, op: 'gte', value: this.value };
  }
}

/** Matches records where a property is less than a value. */
export class LessThanFilter implements IFilter {
  constructor(public readonly field: string, public readonly value: unknown) {}

  toDict(): Record<string, unknown> {
    return { field: this.field, op: 'lt', value: this.value };
  }
}

/** Matches records where a property is less than or equal to a value. */
export class LessThanOrEqualFilter implements IFilter {
  constructor(public readonly field: string, public readonly value: unknown) {}

  toDict(): Record<string, unknown> {
    return { field: this.field, op: 'lte', value: this.value };
  }
}

/** Combines multiple filters with a logical AND. */
export class AndFilter implements IFilter {
  constructor(public readonly filters: IFilter[]) {}

  toDict(): Record<string, unknown> {
    return { logic: 'and', filters: this.filters.map((f) => f.toDict()) };
  }
}

/** Combines multiple filters with a logical OR. */
export class OrFilter implements IFilter {
  constructor(public readonly filters: IFilter[]) {}

  toDict(): Record<string, unknown> {
    return { logic: 'or', filters: this.filters.map((f) => f.toDict()) };
  }
}

/** Negates a filter expression. */
export class NotFilter implements IFilter {
  constructor(public readonly filter: IFilter) {}

  toDict(): Record<string, unknown> {
    return { logic: 'not', filters: [this.filter.toDict()] };
  }
}
