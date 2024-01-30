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
    /// Logicial OR operatior will be applied
    /// </summary>
    Or
}
