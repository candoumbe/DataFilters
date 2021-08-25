namespace DataFilters.Grammar.Syntax
{
    using System;

    /// <summary>
    /// An expression that negate wrapped inside
    /// </summary>
    public sealed class NotExpression : FilterExpression, IEquatable<NotExpression>
    {
        /// <summary>
        /// Expression that the NOT logical is applied to
        /// </summary>
        public FilterExpression Expression { get; }

        /// <summary>
        /// Builds a new <see cref="NotExpression"/> that holds the specified <paramref name="expression"/>.
        /// </summary>
        /// <param name="expression"></param>
        /// <exception cref="ArgumentNullException"><paramref name="expression"/> is <c>null</c>.</exception>
        public NotExpression(FilterExpression expression) => Expression = expression switch
        {
            null => throw new ArgumentNullException(nameof(expression)),
            AndExpression and => new GroupExpression(and),
            OrExpression or => new GroupExpression(or),
            FilterExpression expr => expr
        };

        ///<inheritdoc/>
        public bool Equals(NotExpression other) => Expression.Equals(other?.Expression);

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as NotExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => Expression.GetHashCode();

        ///<inheritdoc/>
        public override string ToString() => $"{{NotExpression [" +
            $"Expression = {Expression.GetType().Name}, " +
            $"{nameof(Expression.EscapedParseableString)} = '{Expression.EscapedParseableString}', " +
            $"{nameof(Expression.OriginalString)} = '{Expression.OriginalString}']," +
            $"{nameof(EscapedParseableString)} = {EscapedParseableString}}}";

        ///<inheritdoc/>
        public override string EscapedParseableString => $"!{Expression.EscapedParseableString}";

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