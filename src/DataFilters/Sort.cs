#if STRING_SEGMENT
using Microsoft.Extensions.Primitives;
# endif
using System;
using static DataFilters.SortDirection;

namespace DataFilters
{
    /// <summary>
    /// Allows to define a <see cref="Sort"/> expression
    /// </summary>
    /// <typeparam name="T">Type onto which the sort applies</typeparam>
    public class Sort<T> : ISort<T>
    {
        public string Expression { get; }

        public SortDirection Direction { get; }

        public Sort(string expression, SortDirection direction = Ascending)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                throw new ArgumentOutOfRangeException(nameof(expression), "cannot be null or whitespace");
            }
            Expression = expression;
            Direction = direction;
        }

        public bool Equals(ISort<T> other)
            => other is Sort<T> sort && (Expression, Direction) == (sort.Expression, sort.Direction);

        public override bool Equals(object obj) => Equals(obj as Sort<T>);

#if ! (NETSTANDARD1_0 || NETSTANDARD1_3 || NETSTANDARD2_0)
        public override int GetHashCode() => HashCode.Combine(Expression, Direction);
#else
        public override int GetHashCode() => (Expression, Direction).GetHashCode();

        /// <inheritdoc/>
        public bool IsEquivalentTo(ISort<T> other) => Equals(other);
#endif

        public override string ToString() => this.Jsonify();
    }
}
