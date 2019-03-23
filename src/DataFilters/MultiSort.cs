using System;
using System.Collections.Generic;
using System.Linq;

namespace DataFilters
{
    public class MultiSort<T> : ISort<T>, IEquatable<MultiSort<T>>
    {
        public IEnumerable<Sort<T>> Sorts => _sorts;

        private readonly IList<Sort<T>> _sorts;

        public MultiSort() => _sorts = new List<Sort<T>>();

        public MultiSort<T> Add(Sort<T> sort)
        {
            _sorts.Add(sort);

            return this;
        }

        public bool Equals(ISort<T> other) => Equals(other as MultiSort<T>);

        public bool Equals(MultiSort<T> other) => !(other is null)
            && Sorts.SequenceEqual(other?.Sorts);

        public override bool Equals(object obj) => Equals(obj as MultiSort<T>);

        public override int GetHashCode() => Sorts.GetHashCode();

        public override string ToString() => $"{nameof(Sorts)}:[{string.Join(",", Sorts.Select(x => x.ToString()))}]";
    }
}
