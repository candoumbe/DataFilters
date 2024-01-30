namespace DataFilters;

using System;

/// <summary>
/// Marker interface
/// </summary>
/// <typeparam name="T">Type of the element the sort will be applied onto</typeparam>
public interface IOrder<T> : IEquatable<IOrder<T>>
{
    /// <summary>
    /// Tests if the current instance is equivalent to <paramref name="other"/>.
    /// </summary>
    /// <param name="other"></param>
    /// <remarks>
    /// Two <see cref="IOrder{T}"/> instances are equivalent when, given an expression, one can be swapped for the other without altering the meaning of the whole expression.
    /// </remarks>
    /// <returns><c>true</c> when current instance and <c>other</c> are equivalent and <c>false</c> otherwise.</returns>
#if !(NETSTANDARD1_3 || NETSTANDARD2_0)
    public virtual bool IsEquivalentTo(IOrder<T> other) => Equals(other);
#else
    bool IsEquivalentTo(IOrder<T> other);
#endif
}
