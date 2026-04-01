import { IFilter } from './iFilter';

/** Matches records where a property is greater than or equal to a value. */
export class GreaterThanOrEqualFilterExpression implements IFilter {
  public constructor(public readonly field: string, public readonly value: unknown) {}

  public toDict(): Record<string, unknown> {
    return { field: this.field, op: 'gte', value: this.value };
  }
}
