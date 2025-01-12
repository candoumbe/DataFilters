namespace DataFilters.Grammar.Syntax
{
    using System;

    /// <summary>
    /// Allows to treat several expressions as a single unit.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A group expression can be used whenever there is a need to apply a logical operator to several expressions at once.
    /// </para>
    /// <para>
    /// The <see cref="Complexity"/> value of a <see cref="GroupExpression"/> is equivalent to the complexity of its inner <see cref="Expression"/>
    /// plus a marginal overhead.
    /// </para>
    /// </remarks>
    public sealed class GroupExpression : FilterExpression, IEquatable<GroupExpression>, ISimplifiable
    {
        private readonly Lazy<string> _lazyEscapedString;

        /// <summary>
        /// Builds a new <see cref="GroupExpression"/> that holds the specified <paramref name="expression"/>.
        /// </summary>
        /// <param name="expression">the expression to group</param>
        /// <exception cref="ArgumentNullException"><paramref name="expression"/> is <see langword="null"/>.</exception>
        public GroupExpression(FilterExpression expression)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            _lazyEscapedString = new Lazy<string>(() => $"({Expression.EscapedParseableString})");
        }

        /// <summary>
        /// <see cref="FilterExpression"/> that the current instance is applied onto.
        /// </summary>
        public FilterExpression Expression { get; }

        ///<inheritdoc/>
        public bool Equals(GroupExpression other) => Expression.Equals(other?.Expression);

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as GroupExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => Expression.GetHashCode();

        /// <inheritdoc />
        public override string ToString(string format, IFormatProvider formatProvider)
        {
            FormattableString formattable = format switch
            {
                "d" or "D" => $"@{nameof(GroupExpression)}({Expression:d})",
                "f" or "F" => $"@{nameof(GroupExpression)}({Expression:d})",
                null or "" => $"{EscapedParseableString}",
                _ => throw new ArgumentOutOfRangeException(nameof(format), $"Unsupported '{format}' format")
            };

            return formattable.ToString(formatProvider);
        }

        ///<inheritdoc/>
        public override string EscapedParseableString => _lazyEscapedString.Value;

        ///<inheritdoc/>
        public override double Complexity => 0.1 + Expression.Complexity;

        ///<inheritdoc/>
        public override bool IsEquivalentTo(FilterExpression other) => other switch
        {
            GroupExpression {Expression: var innerExpression } => Expression.IsEquivalentTo(innerExpression),
            _ => Expression.IsEquivalentTo(other)
        };

        ///<inheritdoc/>
        public FilterExpression Simplify()
            => Expression switch
            {
                //ConstantValueExpression constant => constant,
                ISimplifiable simplify => simplify.Simplify(),
                _ => Expression
            };
    }
}