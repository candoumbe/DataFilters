namespace DataFilters
{
    /// <summary>
    /// Defines the basic shape of a filter
    /// </summary>
    public interface IFilter
    {
        /// <summary>
        /// Gets the JSON representation of the filter
        /// </summary>
        /// <returns></returns>
        string ToJson();
    }
}
