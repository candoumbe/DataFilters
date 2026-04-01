import { IFilter } from './IFilter';

/** Matches records where a property contains a substring. */
export class ContainsFilterExpression implements IFilter {
  public constructor(public readonly field: string, public readonly value: string) {}

  public toDict(): Record<string, unknown> {
    return { field: this.field, op: 'contains', value: this.value };
  }
}
