﻿namespace DataFilters
{
#if STRING_SEGMENT
    using Microsoft.Extensions.Primitives;
#endif
    using System;

    using static DataFilters.OrderDirection;

    /// <summary>
    /// An expression to order <typeparamref name="T"/> elements.
    /// </summary>
    /// <typeparam name="T">Type of elements onto which the sort expression will applies</typeparam>
    public class Order<T> : IOrder<T>
    {
        /// <summary>
        /// The original expression used to create the current <see cref="Order{T}"/>.
        /// </summary>
        public string Expression { get; }

        /// <summary>
        /// The "direction" of the sort./>
        /// </summary>
        public OrderDirection Direction { get; }

        /// <summary>
        /// Creates a new <see cref="Order{T}"/> using the specified <paramref name="expression"/> and <paramref name="direction"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <param name="direction">The direction (optional).</param>
        /// <exception cref="ArgumentOutOfRangeException"> <paramref name="expression"/> is <c>null</c>, empty or contains only whitespaces. </exception>
        public Order(string expression, OrderDirection direction = Ascending)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                throw new ArgumentOutOfRangeException(nameof(expression), "cannot be null or whitespace");
            }
            Expression = expression;
            Direction = direction;
        }

        /// <inheritdoc/>
        public bool Equals(IOrder<T> other)
            => other is Order<T> sort && (Expression, Direction) == (sort.Expression, sort.Direction);

        /// <inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as Order<T>);

        /// <inheritdoc/>
#if !(NETSTANDARD1_0 || NETSTANDARD1_3 || NETSTANDARD2_0)
        public override int GetHashCode() => HashCode.Combine(Expression, Direction);
#else
        public override int GetHashCode() => (Expression, Direction).GetHashCode();

        /// <inheritdoc/>
        public bool IsEquivalentTo(IOrder<T> other) => Equals(other);
#endif

        /// <inheritdoc/>
        public override string ToString() => this.Jsonify();
    }
}
