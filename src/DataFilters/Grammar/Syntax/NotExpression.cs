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
        public NotExpression(FilterExpression expression) => Expression = expression ?? throw new ArgumentNullException(nameof(expression));

        ///<inheritdoc/>
        public bool Equals(NotExpression other) => Expression.Equals(other?.Expression);

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as NotExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => Expression.GetHashCode();

        ///<inheritdoc/>
        public override string ToString() => $"{GetType().Name} : Expression ({Expression.GetType().Name}) -> {Expression}";

        ///<inheritdoc/>
        public override double Complexity => Expression.Complexity;
    }
}