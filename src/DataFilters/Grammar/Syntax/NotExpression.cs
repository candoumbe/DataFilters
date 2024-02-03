namespace DataFilters.Grammar.Syntax
{
    using System;

    /// <summary>
    /// An expression that negate wrapped inside
    /// </summary>
    public sealed class NotExpression : FilterExpression, IEquatable<NotExpression>, ISimplifiable
    {
        /// <summary>
        /// Expression that the NOT logical is applied to
        /// </summary>
        public FilterExpression Expression { get; }

        private readonly Lazy<string> _lazyEscapedParseableString;

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
                FilterExpression expr => expr
            };

            _lazyEscapedParseableString = new Lazy<string>(() => $"!{Expression.EscapedParseableString}");
        }

        ///<inheritdoc/>
        public bool Equals(NotExpression other) => Expression.Equals(other?.Expression);

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as NotExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => Expression.GetHashCode();

        ///<inheritdoc/>
        public override string ToString() => "{NotExpression [" +
            $"Expression = {Expression.GetType().Name}, " +
            $"{nameof(Expression.EscapedParseableString)} = '{Expression.EscapedParseableString}', " +
            $"{nameof(Expression.OriginalString)} = '{Expression.OriginalString}']," +
            $"{nameof(EscapedParseableString)} = {EscapedParseableString}}}" +
            $"{nameof(Expression)} : {Expression} ]";

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