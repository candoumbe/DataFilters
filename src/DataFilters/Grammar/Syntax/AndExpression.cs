using System;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// An expression that combine two <see cref="FilterExpression"/> expressions using the logical <c>AND</c> operator
    /// </summary>
    public class AndExpression : FilterExpression, IEquatable<AndExpression>
    {
        /// <summary>
        /// Left part of the expression
        /// </summary>
        public FilterExpression Left { get; }

        /// <summary>
        /// Right part of the expression
        /// </summary>
        public FilterExpression Right { get; }

        /// <summary>
        /// Builds a new <see cref="AndExpression"/> that combiens <paramref name="left"/> and <paramref name="right"/> using the logical
        /// <c>OR</c> operator
        /// </summary>
        /// <param name="left">Left member</param>
        /// <param name="right">Right member</param>
        /// <exception cref="ArgumentNullException">if either <paramref name="left"/> or <paramref name="right"/> is <c>null</c>.</exception>
        public AndExpression(FilterExpression left, FilterExpression right)
        {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
        }

        public bool Equals(AndExpression other) => Left.Equals(other?.Left) && Right.Equals(other?.Right);

        public override bool Equals(object obj) => Equals(obj as AndExpression);

#if NETSTANDARD1_3 || NETSTANDARD2_0
        public override int GetHashCode() => (Left, Right).GetHashCode();
#else
        public override int GetHashCode() => HashCode.Combine(Left, Right);
#endif

        public override string ToString() => $"{nameof(AndExpression)} : {{{nameof(Left)} ({Left.GetType().Name}) -> {Left}, {{{nameof(Right)} ({Right.GetType().Name}) -> {Right}}}";
    }
}