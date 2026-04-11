import { IFilter } from "./iFilter";

/** Matches records where a property contains a substring. */
export class ContainsFilter implements IFilter {
  public constructor(
    public readonly field: string,
    public readonly value: string,
  ) {}

  public toDict(): Record<string, unknown> {
    return { field: this.field, op: "contains", value: this.value };
  }

  /**
   * Creates a ContainsFilter from a dictionary.
   * @param dict The dictionary to create the filter from. Must have "field", "op", and "value" keys.
   * @returns A new ContainsFilter instance.
   * @throws Error if the dictionary does not have the correct keys or types.
   */
  public static fromDict(dict: Record<string, unknown>): ContainsFilter {
    if (dict["op"] !== "contains") {
      throw new Error(`Invalid operator for ContainsFilter: ${dict["op"]}`);
    }
    if (
      typeof dict["field"] !== "string" ||
      typeof dict["value"] !== "string"
    ) {
      throw new Error("Invalid field or value type for ContainsFilter");
    }
    return new ContainsFilter(dict["field"], dict["value"]);
  }
}
