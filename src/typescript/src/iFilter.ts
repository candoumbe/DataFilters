/** Base interface for all filter expressions. */
export interface IFilter {
  /** Converts the filter to a dictionary representation. */
  toDict(): Record<string, unknown>;
}
