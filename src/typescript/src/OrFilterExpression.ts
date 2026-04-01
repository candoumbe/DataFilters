import { IFilter } from './IFilter';

/** Combines multiple filters with a logical OR. */
export class OrFilterExpression implements IFilter {
  public constructor(public readonly filters: IFilter[]) {}

  public toDict(): Record<string, unknown> {
    return { logic: 'or', filters: this.filters.map((f) => f.toDict()) };
  }
}
