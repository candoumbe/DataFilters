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
#if !STRING_SEGMENT
        public string Expression { get; }
#else
        public StringSegment Expression { get; }
#endif

        public SortDirection Direction { get; }

#if STRING_SEGMENT
        /// <summary>
        /// Builds a new 
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="direction"></param>
        public Sort(StringSegment expression, SortDirection direction = Ascending)
        {
            if (expression.Trim().Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(expression), "cannot be empty or whitespace only");
            }
            Expression = expression;
            Direction = direction;
        }
#endif
        public Sort(string expression, SortDirection direction = Ascending)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                throw new ArgumentOutOfRangeException(nameof(expression), "cannot be null or whitespace");
            }
            Expression = expression;
            Direction = direction;
        }

        public bool Equals(ISort<T> other) => other is Sort<T> sort
            && (Expression, Direction) == (sort.Expression, sort.Direction);

        public override bool Equals(object obj) => Equals(obj as Sort<T>);

        public override int GetHashCode() => (Expression, Direction).GetHashCode();

#if !STRING_SEGMENT
        public override string ToString() => this.Stringify();
#else
        public override string ToString() => new { Expression = Expression.Value, Direction = Direction.ToString() }.Stringify();
#endif
    }
}
