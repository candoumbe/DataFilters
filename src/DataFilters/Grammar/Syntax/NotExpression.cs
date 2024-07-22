namespace DataFilters.Grammar.Syntax
{
    using System;

    /// <summary>
    /// An expression that negate wrapped inside
    /// </summary>
    public sealed class NotExpression : FilterExpression, IEquatable<NotExpression>, ISimplifiable, IFormattable
    {
        /// <summary>
        /// Expression that the NOT logical is applied to
        /// </summary>
        public FilterExpression Expression { get; }

        private readonly Lazy<string> _lazyEscapedParseableString;
        private readonly Lazy<string> _lazyOriginalString;

        /// <summary>
        /// Builds a new <see cref="NotExpression"/> that holds the specified <paramref name="expression"/>.
        /// </summary>
        /// <param name="expression"></param>
        /// <exception cref="ArgumentNullException"><paramref name="expression"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <paramref name="expression"/> will be automatically wrapped inside a <see cref="GroupExpression"/> when it's a <see cref="BinaryFilterExpression"/>.
        /// </remarks>
        public NotExpression(FilterExpression expression)
        {
            Expression = expression switch
            {
                null => throw new ArgumentNullException(nameof(expression)),
                BinaryFilterExpression expr => new GroupExpression(expr),
                _ => expression
            };

            _lazyEscapedParseableString = new Lazy<string>(() => $"!{Expression.EscapedParseableString}");
            _lazyOriginalString = new Lazy<string>(() => $"!{Expression.OriginalString}");
        }

        ///<inheritdoc/>
        public bool Equals(NotExpression other) => Expression.Equals(other?.Expression);

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as NotExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => Expression.GetHashCode();

        ///<inheritdoc/>
        public override string ToString() => $"{{{nameof(NotExpression)} [" +
            $"Expression = {Expression.GetType().Name}, " +
            $"{nameof(Expression)}.{nameof(Expression.EscapedParseableString)} = '{Expression.EscapedParseableString}',{Environment.NewLine} " +
            $"{nameof(Expression)}.{nameof(Expression.OriginalString)} = '{Expression.OriginalString}'],{Environment.NewLine}" +
            $"{nameof(EscapedParseableString)} = {EscapedParseableString}}},{Environment.NewLine}" +
            $"{nameof(OriginalString)} : {OriginalString} ]";

        /// <inheritdoc />
        public override string ToString(string format, IFormatProvider formatProvider)
        {
            FormattableString formattable = format switch
            {
                "d" or "D" => $"@{nameof(NotExpression)}({Expression:d})",
                null or "" => $"{ToString()}",
                _ => throw new ArgumentOutOfRangeException(nameof(format), $"Unsupported '{format}' format")
            };

            return formattable.ToString(formatProvider);
        }

        ///<inheritdoc/>
        public FilterExpression Simplify()
            => Expression switch
            {
                NotExpression innerNotExpression => innerNotExpression.Expression,
                ISimplifiable simplifiable => new NotExpression(simplifiable.Simplify()),
                _ => this
            };

        ///<inheritdoc/>
        public override string EscapedParseableString => _lazyEscapedParseableString.Value;

        ///<inheritdoc/>
        public override string OriginalString => _lazyOriginalString.Value;

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