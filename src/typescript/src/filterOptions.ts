/**
 * Logic applied when combining multiple filter criteria.
 * Mirrors the C# `FilterLogic` enum.
 */
export enum FilterLogic {
    And = "and",
    Or = "or",
}

/**
 * Options for customizing filter parsing behaviour.
 *
 * Mirrors the C# `FilterOptions` record, providing the same resilience /
 * handling strategy at parse time.
 *
 * @example
 * ```typescript
 * // comma-separated parts will be combined with OR instead of AND
 * const options = new FilterOptions({ logic: FilterLogic.Or });
 * const filter = parse("name=Batman,age=30", options);
 * // → OrFilter([EqualsFilter("name","Batman"), EqualsFilter("age","30")])
 * ```
 */
export class FilterOptions {
    /**
     * Logic to apply when combining multiple comma-separated filter criteria.
     *
     * Defaults to `FilterLogic.And`.
     */
    readonly logic: FilterLogic;

    constructor({ logic = FilterLogic.And }: { logic?: FilterLogic } = {}) {
        this.logic = logic;
    }
}
