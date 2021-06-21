using System;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// A <see cref="FilterExpression"/> that combine two <see cref="FilterExpression"/> expressions using the logical <c>AND</c> operator
    /// </summary>
    public sealed class AndExpression : FilterExpression, IEquatable<AndExpression>, IHaveComplexity
    {
        /// <summary>
        /// Left part of the expression
        /// </summary>
        public FilterExpression Left { get; }

        /// <summary>
        /// Right part of the expression
        /// </summary>
        public FilterExpression Right { get; }

        /// <inheritdoc/>
        public override double Complexity => Right.Complexity * Left.Complexity;

        /// <summary>
        /// Builds a new <see cref="AndExpression"/> that combiens <paramref name="left"/> and <paramref name="right"/> using the logical
        /// <c>AND</c> operator
        /// </summary>
        /// <param name="left">Left member</param>
        /// <param name="right">Right member</param>
        /// <exception cref="ArgumentNullException">if either <paramref name="left"/> or <paramref name="right"/> is <c>null</c>.</exception>
        public AndExpression(FilterExpression left, FilterExpression right)
        {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
        }

        ///<inheritdoc/>
        public bool Equals(AndExpression other) => Left.Equals(other?.Left) && Right.Equals(other?.Right);

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as AndExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => (Left, Right).GetHashCode();

        ///<inheritdoc/>
        public override string ToString() => this.Jsonify();
    }
}