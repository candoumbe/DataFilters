using System;

namespace DataFilters
{
    /// <summary>
    /// Marker interface
    /// </summary>
    /// <typeparam name="T">Type of the element the sort will be applied onto</typeparam>
    public interface ISort<T> : IEquatable<ISort<T>>
    {
        /// <summary>
        /// Tests if the current instance is equivalent to <paramref name="other"/>.
        /// </summary>
        /// <param name="other"></param>
        /// <remarks>
        /// Two <see cref="ISort{T}"/> instances are equivalent when, given an expression, one can be swapped for the other without altering the meaning of the whole expression.
        /// </remarks>
        /// <returns><c>true</c> when current instance and <c>other</c> are equivalent and <c>false</c> otherwise.</returns>
#if !(NETSTANDARD1_3 || NETSTANDARD2_0)
        public virtual bool IsEquivalentTo(ISort<T> other) => Equals(other);
#else
        bool IsEquivalentTo(ISort<T> other);
#endif
    }
}
