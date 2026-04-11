import { IFilter } from "./iFilter";

export class OrFilter implements IFilter {
  public constructor(
    public readonly left: IFilter,
    public readonly right: IFilter,
  ) {}

  public toDict(): Record<string, unknown> {
    return { logic: "or", filters: [this.left.toDict(), this.right.toDict()] };
  }
}
