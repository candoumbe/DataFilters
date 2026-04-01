/** Base interface for all filter expressions. */
export interface IFilter {
  toDict(): Record<string, unknown>;
}
