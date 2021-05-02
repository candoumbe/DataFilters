using System;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// An expression that group wrapped inside
    /// </summary>
    public sealed class GroupExpression : FilterExpression, IEquatable<GroupExpression>
    {
        /// <summary>
        /// Expression that the group is applied onto
        /// </summary>
        public FilterExpression Expression { get; }

        /// <summary>
        /// Builds a new <see cref="GroupExpression"/> that holds the specified <paramref name="innerExpression"/>.
        /// </summary>
        /// <param name="innerExpression"></param>
        /// <exception cref="ArgumentNullException"><paramref name="innerExpression"/> is <c>null</c>.</exception>
        public GroupExpression(FilterExpression innerExpression) => Expression = innerExpression ?? throw new ArgumentNullException(nameof(innerExpression));

        ///<inheritdoc/>
        public bool Equals(GroupExpression other) => Expression.Equals(other?.Expression);

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as GroupExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => Expression.GetHashCode();

        ///<inheritdoc/>
        public override string ToString() => $"{GetType().Name} : Expression ({Expression.GetType().Name }) -> {Expression}";
    }
}