﻿namespace DataFilters.Grammar.Syntax
{
    using System;

    /// <summary>
    /// Combines two <see cref="FilterExpression"/>s using the logical <c>OR</c> operator
    /// </summary>
    public sealed class OrExpression : BinaryFilterExpression, IEquatable<OrExpression>
    {
        private readonly Lazy<string> _lazyToString;
        private readonly Lazy<string> _lazyEscapedParseableString;

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
        /// <exception cref="ArgumentNullException">if either <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.</exception>
        public OrExpression(FilterExpression left, FilterExpression right) : base(left, right)
        {
            _lazyToString = new(() => $@"[""{nameof(Left)} ({Left.GetType().Name})"": {Left.EscapedParseableString}; ""{nameof(Right)} ({Right.GetType().Name})"": {Right.EscapedParseableString}]");
            _lazyEscapedParseableString = new(() => $"{Left.EscapedParseableString}|{Right.EscapedParseableString}");
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
        public override string ToString() => _lazyToString.Value;

        ///<inheritdoc/>
        public override string EscapedParseableString => _lazyEscapedParseableString.Value;

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
        public override double Complexity => Left.Complexity + Right.Complexity;
    }
}