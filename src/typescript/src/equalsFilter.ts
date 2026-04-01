import { IFilter } from './iFilter';

/** Matches records where a property equals a specific value. */
export class EqualsFilter implements IFilter {
  public constructor(public readonly field: string, public readonly value: unknown) {}

  public toDict(): Record<string, unknown> {
    return { field: this.field, op: 'eq', value: this.value };
  }
}
