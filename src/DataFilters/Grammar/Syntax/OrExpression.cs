namespace DataFilters.Grammar.Syntax
{
    using System;

    /// <summary>
    /// An expression that combine two <see cref="FilterExpression"/> expressions using the logical <c>OR</c> operator
    /// </summary>
    public sealed class OrExpression : FilterExpression, IEquatable<OrExpression>, ISimplifiable
    {
        /// <summary>
        /// Left member of the expression
        /// </summary>
        public FilterExpression Left { get; }

        /// <summary>
        /// Right member of the expression
        /// </summary>
        public FilterExpression Right { get; }

        /// <summary>
        /// Builds a new <see cref="OrExpression"/> that combines <paramref name="left"/> and <paramref name="right"/> using the logical
        /// <c>OR</c> operator
        /// </summary>
        /// <remarks>
        /// The constructor will wrap <paramref name="left"/> (respectively  <paramref name="right"/>) inside a <see cref="GroupExpression"/> when <paramref name="left"/> is either
        /// a <see cref="AndExpression"/> or a <see cref="OrExpression"/>.
        /// </remarks>
        /// <param name="left">Left member of the expression</param>
        /// <param name="right">Right member of the expression</param>
        /// <exception cref="ArgumentNullException">if either <paramref name="left"/> or <paramref name="right"/> is <c>null</c>.</exception>
        public OrExpression(FilterExpression left, FilterExpression right)
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
        }

        ///<inheritdoc/>
        public bool Equals(OrExpression other) => Left.Equals(other?.Left) && Right.Equals(other?.Right);

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as OrExpression);

        ///<inheritdoc/>
#if NETSTANDARD1_3 || NETSTANDARD2_0
        public override int GetHashCode() => (Left, Right).GetHashCode();
#else
        public override int GetHashCode() => HashCode.Combine(Left, Right);
#endif

        ///<inheritdoc/>
        public override string ToString() => new { Type = nameof(OrExpression), Left, Right, Complexity }.Jsonify();

        ///<inheritdoc/>
        public override string EscapedParseableString => $"{Left.EscapedParseableString}|{Right.EscapedParseableString}";

        /// <inheritdoc/>
        public override bool IsEquivalentTo(FilterExpression other)
        {
            return other switch
            {
                OrExpression or => (or.Right.IsEquivalentTo(Right) && or.Left.IsEquivalentTo(Left))
                                                || (or.Left.IsEquivalentTo(Right) && or.Right.IsEquivalentTo(Left)),
                OneOfExpression oneOf => IsEquivalentTo(oneOf.Simplify()),
                _ => Left.IsEquivalentTo(Right) && Left.IsEquivalentTo(other),
            };
        }

        ///<inheritdoc/>
        public FilterExpression Simplify()
        {
            FilterExpression simplifiedLeft = (Left as ISimplifiable)?.Simplify() ?? Left;
            FilterExpression simplifiedRigth = (Right as ISimplifiable)?.Simplify() ?? Right;

            return simplifiedLeft.IsEquivalentTo(simplifiedRigth) ? simplifiedLeft : this;
        }

        ///<inheritdoc/>
        public override double Complexity => Left.Complexity + Right.Complexity;
    }
}