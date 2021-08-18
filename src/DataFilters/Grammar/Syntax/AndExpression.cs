namespace DataFilters.Grammar.Syntax
{
    using System;
    using System.Linq;

    /// <summary>
    /// A <see cref="FilterExpression"/> that combine two <see cref="FilterExpression"/> expressions using the logical <c>AND</c> operator
    /// </summary>
    public sealed class AndExpression : FilterExpression, IEquatable<AndExpression>, ISimplifiable
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
        public override string ToString() => new { Type = nameof(AndExpression), Left, Right, Complexity }.Jsonify();

        ///<inheritdoc/>
        public FilterExpression Simplify()
        {
            FilterExpression simplifiedExpression = this;
            FilterExpression simplifiedLeft = (Left as ISimplifiable)?.Simplify() ?? Left;
            FilterExpression simplifiedRight = (Right as ISimplifiable)?.Simplify() ?? Right;

            if (simplifiedLeft.IsEquivalentTo(simplifiedRight))
            {
                simplifiedExpression = simplifiedLeft.Complexity < simplifiedRight.Complexity
                    ? simplifiedLeft
                    : simplifiedRight;
                                                            ;
            }

            return simplifiedExpression;
        }

        ///<inheritdoc/>
        public override bool IsEquivalentTo(FilterExpression other)
        {
            bool equivalent;
            if (ReferenceEquals(this, other))
            {
                equivalent = true;
            }
            else
            {
                equivalent = other switch
                {
                    AndExpression and => (Left.IsEquivalentTo(and.Left) && Right.IsEquivalentTo(and.Right)) || (Left.IsEquivalentTo(and.Right) && Right.IsEquivalentTo(and.Left)),
                    ConstantValueExpression constant => Simplify().IsEquivalentTo(constant),
                    ISimplifiable simplifiable => IsEquivalentTo(simplifiable.Simplify()),
                    _ => false
                };
            }

            return equivalent;
        }

        ///<inheritdoc/>
        public override string ParseableString => $"{Left.ParseableString},{Right.ParseableString}";
    }
}