namespace DataFilters.Grammar.Syntax
{
    using System;

    /// <summary>
    /// An expression that negate whatever <see cref="FilterExpression"/> wrapped inside.
    /// </summary>
    public sealed class NotExpression : FilterExpression, IEquatable<NotExpression>, ISimplifiable
    {
        /// <summary>
        /// Expression that the NOT logical is applied to
        /// </summary>
        public FilterExpression Expression { get; }

        /// <summary>
        /// Builds a new <see cref="NotExpression"/> that holds the specified <paramref name="expression"/>.
        /// </summary>
        /// <param name="expression">The expression onto which the <c>not</c> operator will be applied</param>
        /// <exception cref="ArgumentNullException"><paramref name="expression"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <paramref name="expression"/> will be automatically wrapped inside a <see cref="GroupExpression"/> if it's a <see cref="BinaryFilterExpression"/>
        /// in order to keep the semantic
        /// .
        /// </remarks>
        public NotExpression(FilterExpression expression)
        {
            (EscapedParseableString, Expression) = expression switch
            {
                null => throw new ArgumentNullException(nameof(expression)),
                BinaryFilterExpression expr => ($"!({expr.EscapedParseableString})", new GroupExpression(expr)),
                _ => ($"!{expression.EscapedParseableString}", expression)
            };
        }

        ///<inheritdoc/>
        public bool Equals(NotExpression other) => Expression.Equals(other?.Expression);

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as NotExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => Expression.GetHashCode();

        /// <inheritdoc />
        public override string ToString(string format, IFormatProvider formatProvider)
        {
            FormattableString formattable = format switch
            {
                "d" or "D" => $"@{nameof(NotExpression)}({Expression:d})",
                "f" or "F" => $"@{nameof(NotExpression)}[{Expression:f}]",
                null or "" => $"{EscapedParseableString}",
                _ => throw new ArgumentOutOfRangeException(nameof(format), $"Unsupported '{format}' format")
            };

            return formattable.ToString(formatProvider);
        }

        ///<inheritdoc/>
        public FilterExpression Simplify()
            => Expression switch
            {
                NotExpression innerNotExpression => innerNotExpression.Expression,
                ISimplifiable simplifiable => !simplifiable.Simplify(),
                _ => this
            };

        ///<inheritdoc/>
        public override string EscapedParseableString { get; }

        ///<inheritdoc/>
        public override double Complexity => Expression.Complexity;

        ///<inheritdoc/>
        public static bool operator ==(NotExpression left, NotExpression right) => left switch
        {
            null => right is null,
            _ => left.Equals(right)
        };

        ///<inheritdoc/>
        public static bool operator !=(NotExpression left, NotExpression right) => !(left == right);
    }
}