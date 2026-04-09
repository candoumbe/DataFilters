import { IFilter } from "./iFilter";

/** Negates a filter expression. */
export class NotFilter implements IFilter {
  public constructor(public readonly filter: IFilter) {}

  public toDict(): Record<string, unknown> {
    return { logic: "not", filters: [this.filter.toDict()] };
  }
}
