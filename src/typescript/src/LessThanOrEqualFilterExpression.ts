import { IFilter } from './IFilter';

/** Matches records where a property is less than or equal to a value. */
export class LessThanOrEqualFilterExpression implements IFilter {
  public constructor(public readonly field: string, public readonly value: unknown) {}

  public toDict(): Record<string, unknown> {
    return { field: this.field, op: 'lte', value: this.value };
  }
}
