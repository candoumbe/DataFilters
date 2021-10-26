namespace DataFilters.Grammar.Syntax
{
    using System;

    /// <summary>
    /// A <see cref="FilterExpression"/> that combine two <see cref="FilterExpression"/> expressions using the logical <c>AND</c> operator
    /// </summary>
    public sealed class AndExpression : FilterExpression, IEquatable<AndExpression>, ISimplifiable
    {
        private readonly Lazy<string> _lazyToString;

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
        /// Builds a new <see cref="AndExpression"/> that combines <paramref name="left"/> and <paramref name="right"/> using the logical
        /// <c>AND</c> operator
        /// </summary>
        /// <remarks>
        /// <paramref name="left"/> and/or <paramref name="right"/> will be wrapped inside a <see cref="GroupExpression"/> when either is a 
        /// <see cref="AndExpression"/> or a <see cref="OrExpression"/> instance.
        /// </remarks>
        /// <param name="left">Left member</param>
        /// <param name="right">Right member</param>
        /// <exception cref="ArgumentNullException">if either <paramref name="left"/> or <paramref name="right"/> is <c>null</c>.</exception>
        public AndExpression(FilterExpression left, FilterExpression right)
        {
            Left = left switch
            {
                null => throw new ArgumentNullException(nameof(left)),
                AndExpression or OrExpression => new GroupExpression(left),
                _ => left
            };

            Right = right switch
            {
                null => throw new ArgumentNullException(nameof(right)),
                AndExpression or OrExpression => new GroupExpression(right),
                _ => right
            };

            _lazyToString = new(() => $@"[""{nameof(Left)} ({Left.GetType().Name})"": {Left.EscapedParseableString}; ""{nameof(Right)} ({Right.GetType().Name})"": {Right.EscapedParseableString}]");
        }

        ///<inheritdoc/>
        public bool Equals(AndExpression other) => Left.Equals(other?.Left) && Right.Equals(other?.Right);

        ///<inheritdoc/>
        public override bool Equals(object obj) => obj switch
        {
            AndExpression and => Equals(and),
            FilterExpression expression => IsEquivalentTo(expression),
            _ => false
        };

        ///<inheritdoc/>
        public override int GetHashCode() => (Left, Right).GetHashCode();

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
            }

            return simplifiedExpression;
        }

        ///<inheritdoc/>
        public override bool IsEquivalentTo(FilterExpression other)
            => ReferenceEquals(this, other)
                || other switch
                {
                    AndExpression and => (Left.IsEquivalentTo(and.Left) && Right.IsEquivalentTo(and.Right)) || (Left.IsEquivalentTo(and.Right) && Right.IsEquivalentTo(and.Left)),
                    ConstantValueExpression constant => Simplify().IsEquivalentTo(constant),
                    ISimplifiable simplifiable => IsEquivalentTo(simplifiable.Simplify()),
                    _ => false
                };

        ///<inheritdoc/>
        public override string EscapedParseableString => $"{Left.EscapedParseableString},{Right.EscapedParseableString}";

        ///<inheritdoc/>
        public override string ToString() => _lazyToString.Value;
    }
}