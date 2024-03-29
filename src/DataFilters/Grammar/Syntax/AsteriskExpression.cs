﻿namespace DataFilters.Grammar.Syntax
{
    using System;

    /// <summary>
    /// <see cref="AsteriskExpression"/> represent a <c>*</c> that can be used as a building block for more
    /// complex <see cref="FilterExpression"/>s.
    /// </summary>
    public sealed class AsteriskExpression : FilterExpression, IEquatable<AsteriskExpression>, IBoundaryExpression
    {
        /// <summary>
        /// The unique instance of the <see cref="AsteriskExpression"/>.
        /// </summary>
        /// <remarks>
        /// This field is <see langword="readonly"/>
        /// </remarks>
        public static AsteriskExpression Instance { get; } = new();

        private AsteriskExpression() { }

        ///<inheritdoc/>
        public bool Equals(AsteriskExpression other) => other is not null;

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as AsteriskExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => 1;

        ///<inheritdoc/>
        public override string EscapedParseableString => "*";

        /// <summary>
        /// Computes a <see cref="EndsWithExpression"/> by adding a <see cref="AsteriskExpression"/> to a <see cref="ConstantValueExpression"/>.
        /// </summary>
        /// <param name="_"></param>
        /// <param name="right"></param>
        /// <returns><see cref="EndsWithExpression"/></returns>
        public static EndsWithExpression operator +(AsteriskExpression _, ConstantValueExpression right) => new(right.Value);

#if !NET6_0_OR_GREATER
        /// <summary>
        /// Computes a <see cref="StartsWithExpression"/> by adding a <see cref="ConstantValueExpression"/> to a <see cref="AsteriskExpression"/>.
        /// </summary>
        /// <param name="left">Left operand</param>
        /// <param name="right">Right operand</param>
        /// <returns><see cref="StartsWithExpression"/></returns>
#else
        ///<inheritdoc />
#endif
#pragma warning disable IDE0060, RCS1163
        public static StartsWithExpression operator +(ConstantValueExpression left, AsteriskExpression right) => new(left.Value);
#pragma warning restore IDE0060, RCS1163
    }
}