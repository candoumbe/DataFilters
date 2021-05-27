using System;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// An expression that group wrapped inside
    /// </summary>
#if NETSTANDARD1_3
    public sealed class GroupExpression : FilterExpression, IEquatable<GroupExpression>
#else
    public record GroupExpression : FilterExpression, IEquatable<GroupExpression>
#endif
    {
        /// <summary>
        /// Expression that the group is applied onto
        /// </summary>
        public FilterExpression Expression { get; }

        /// <summary>
        /// Builds a new <see cref="GroupExpression"/> that holds the specified <paramref name="expression"/>.
        /// </summary>
        /// <param name="expression"></param>
        /// <exception cref="ArgumentNullException"><paramref name="expression"/> is <c>null</c>.</exception>
        public GroupExpression(FilterExpression expression) => Expression = expression ?? throw new ArgumentNullException(nameof(expression));

#if NETSTANDARD1_3
        ///<inheritdoc/>
        public bool Equals(GroupExpression other) => Expression.Equals(other?.Expression);

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as GroupExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => Expression.GetHashCode();

        ///<inheritdoc/>
        public override string ToString() => $"{GetType().Name} : Expression ({Expression.GetType().Name }) -> {Expression}";
#endif
    }
}