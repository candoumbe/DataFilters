import { IFilter } from './iFilter';

/** Matches records where a property is greater than a value. */
export class GreaterThanFilterExpression implements IFilter {
  public constructor(public readonly field: string, public readonly value: unknown) {}

  public toDict(): Record<string, unknown> {
    return { field: this.field, op: 'gt', value: this.value };
  }
}
