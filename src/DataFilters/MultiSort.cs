using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace DataFilters
{
    /// <summary>
    /// Allow to sort by multiple <see cref="Sort{T}"/> expression
    /// </summary>
    /// <typeparam name="T">Type onto which the sort will be applied</typeparam>
    public class MultiSort<T> : ISort<T>, IEquatable<MultiSort<T>>
    {
        /// <summary>
        /// <see cref="Sort{T}"/> items that the current instance carries.
        /// </summary>
        public IEnumerable<Sort<T>> Sorts => _sorts;

        private readonly Sort<T>[] _sorts;

        private static readonly ArrayEqualityComparer<Sort<T>> equalityComparer = new();

        /// <summary>
        /// Builds a new <see cref="MultiSort{T}"/> instance.
        /// </summary>
        /// <param name="sorts"></param>
        public MultiSort(params Sort<T>[] sorts) => _sorts = sorts.Where(s => s is not null)
                                                                  .ToArray();
        /// <inheritdoc/>
        public bool Equals(ISort<T> other) => Equals(other as MultiSort<T>);

        /// <inheritdoc/>
        public bool Equals(MultiSort<T> other) => !(other is null)
                                                  && equalityComparer.Equals(_sorts, other._sorts);

        /// <inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as MultiSort<T>);

        /// <inheritdoc/>
        public bool IsEquivalentTo(ISort<T> other) => other is MultiSort<T> otherMultisort
                                                      && Sorts.SequenceEqual(otherMultisort.Sorts);

        /// <inheritdoc/>
        public override int GetHashCode() => equalityComparer.GetHashCode(_sorts);

        /// <inheritdoc/>
        public override string ToString() => $"{nameof(Sorts)}:[{string.Join(",", Sorts.Select(x => x.ToString()))}]";

        /// <inheritdoc/>
        public static bool operator ==(MultiSort<T> left, MultiSort<T> right) => EqualityComparer<MultiSort<T>>.Default.Equals(left, right);

        /// <inheritdoc/>
        public static bool operator !=(MultiSort<T> left, MultiSort<T> right) => !(left == right);
    }
}
