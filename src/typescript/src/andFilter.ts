import { IFilter } from './iFilter';

/** Combines multiple filters with a logical AND. */
export class AndFilter implements IFilter {
  public constructor(public readonly filters: IFilter[]) {}

  public toDict(): Record<string, unknown> {
    return { logic: 'and', filters: this.filters.map((f) => f.toDict()) };
  }
}
