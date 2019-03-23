using System;

namespace DataFilters
{
    /// <summary>
    /// Marker interface
    /// </summary>
    /// <typeparam name="T">Type of the element the sort will be applied onto</typeparam>
    public interface ISort<T> : IEquatable<ISort<T>>
    {

    }
}
