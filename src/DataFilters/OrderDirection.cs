namespace DataFilters;

/// <summary>
/// Enumeration of direction that can be associated to a <see cref="Order{T}"/> instance
/// </summary>
public enum OrderDirection : short
{
    /// <summary>
    /// Order elements from the lowest to the highest.
    /// </summary>
    Ascending,
    /// <summary>
    /// Order eleemnts from the highest to the lowest.
    /// </summary>
    Descending
}
