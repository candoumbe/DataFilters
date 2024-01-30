namespace DataFilters;

using System;
using System.Collections.Generic;
using System.Linq;

using Utilities;

/// <summary>
/// Allow to sort by multiple <see cref="Order{T}"/> expression
/// </summary>
/// <typeparam name="T">Type onto which the sort will be applied</typeparam>
/// <remarks>
/// Builds a new <see cref="MultiOrder{T}"/> instance.
/// </remarks>
/// <param name="orders"></param>
public class MultiOrder<T>(params Order<T>[] orders) : IOrder<T>, IEquatable<MultiOrder<T>>
{
    /// <summary>
    /// <see cref="Order{T}"/> items that the current instance carries.
    /// </summary>
    public IEnumerable<Order<T>> Orders => _orders;

    private readonly Order<T>[] _orders = orders.Where(s => s is not null)
                                                              .ToArray();

    private static readonly ArrayEqualityComparer<Order<T>> equalityComparer = new();

    /// <inheritdoc/>
    public bool Equals(IOrder<T> other) => Equals(other as MultiOrder<T>);

    /// <inheritdoc/>
    public bool Equals(MultiOrder<T> other) => other is not null
                                              && equalityComparer.Equals(_orders, other._orders);

    /// <inheritdoc/>
    public override bool Equals(object obj) => Equals(obj as MultiOrder<T>);

    /// <inheritdoc/>
    public bool IsEquivalentTo(IOrder<T> other) => other is MultiOrder<T> otherMultisort
                                                  && Orders.SequenceEqual(otherMultisort.Orders);

    /// <inheritdoc/>
    public override int GetHashCode() => equalityComparer.GetHashCode(_orders);

    /// <inheritdoc/>
    public override string ToString() => $"{nameof(Orders)}:[{string.Join(",", Orders.Select(x => x.ToString()))}]";

    /// <inheritdoc/>
    public static bool operator ==(MultiOrder<T> left, MultiOrder<T> right) => EqualityComparer<MultiOrder<T>>.Default.Equals(left, right);

    /// <inheritdoc/>
    public static bool operator !=(MultiOrder<T> left, MultiOrder<T> right) => !(left == right);
}
