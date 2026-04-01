import { IFilter } from './IFilter';

/** Combines multiple filters with a logical AND. */
export class AndFilterExpression implements IFilter {
  public constructor(public readonly filters: IFilter[]) {}

  public toDict(): Record<string, unknown> {
    return { logic: 'and', filters: this.filters.map((f) => f.toDict()) };
  }
}
