import { IFilter } from './IFilter';

/** Negates a filter expression. */
export class NotFilterExpression implements IFilter {
  public constructor(public readonly filter: IFilter) {}

  public toDict(): Record<string, unknown> {
    return { logic: 'not', filters: [this.filter.toDict()] };
  }
}
