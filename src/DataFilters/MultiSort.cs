using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace DataFilters
{
    public class MultiSort<T> : ISort<T>, IEquatable<MultiSort<T>>
    {
        public IEnumerable<Sort<T>> Sorts => _sorts;

        private readonly Sort<T>[] _sorts;

        private static readonly ArrayEqualityComparer<Sort<T>> equalityComparer = new ArrayEqualityComparer<Sort<T>>();

        /// <summary>
        /// Builds a new <see cref="MultiSort{T}"/> instance
        /// </summary>
        public MultiSort(params Sort<T>[] sorts) => _sorts = sorts?.Where(s => s != null)
                                                                   .ToArray();

        public bool Equals(ISort<T> other) => Equals(other as MultiSort<T>);

        public bool Equals(MultiSort<T> other) => !(other is null)
            && equalityComparer.Equals(_sorts, other._sorts);

        /// <inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as MultiSort<T>);

        public bool IsEquivalentTo(ISort<T> other)
        {
            return other is MultiSort<T> otherMultisort && Sorts.SequenceEqual(otherMultisort.Sorts);
        }

        /// <inheritdoc/>
        public override int GetHashCode() => equalityComparer.GetHashCode(_sorts);
        
        public override string ToString() => $"{nameof(Sorts)}:[{string.Join(",", Sorts.Select(x => x.ToString()))}]";

        public static bool operator ==(MultiSort<T> left, MultiSort<T> right)
        {
            return EqualityComparer<MultiSort<T>>.Default.Equals(left, right);
        }

        public static bool operator !=(MultiSort<T> left, MultiSort<T> right)
        {
            return !(left == right);
        }
    }
}
