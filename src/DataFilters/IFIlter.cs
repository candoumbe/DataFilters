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

        /// <summary>
        /// Computes a new <see cref="IFilter"/> instance which is the exact opposite of the current instance.
        /// </summary>
        /// <returns>The exact opposite of the current instance.</returns>
        IFilter Negate();
    }
}
