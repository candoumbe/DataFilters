using System;

namespace DataFilters
{
    /// <summary>
    /// Marker interface
    /// </summary>
    /// <typeparam name="T">Type of the element the sort will be applied onto</typeparam>
    public interface ISort<T> : IEquatable<ISort<T>>
    {
#if !(NETSTANDARD1_3 || NETSTANDARD2_0)
        public virtual bool IsEquivalentTo(ISort<T> other) => Equals(other);
#else
        bool IsEquivalentTo(ISort<T> other);
#endif
    }
}
