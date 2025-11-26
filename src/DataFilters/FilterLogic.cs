namespace DataFilters;

/// <summary>
/// Logic that can be apply when combining several <see cref="Filter"/>s together.
/// </summary>
/// <see cref="MultiFilter.Logic"/>
public enum FilterLogic
{
    /// <summary>
    /// Logical AND operator will be applied to all
    /// </summary>
    And,

    /// <summary>
    /// Logical OR operator will be applied
    /// </summary>
    Or
}