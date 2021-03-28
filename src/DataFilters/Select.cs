#if STRING_SEGMENT
using Microsoft.Extensions.Primitives;
# endif
using EnsureThat;

using System;

namespace DataFilters
{
    /// <summary>
    /// Allows to define a <see cref="Select"/> expression
    /// </summary>
    public class Select
    {
        /// <summary>
        /// The string representation of <typeparamref name="T"/> properties to be selected
        /// </summary>
        public string Expression { get; }

        /// <summary>
        /// Builds a new <see cref="Select{T}"/> instance based on the provided <paramref name="expression"/>.
        /// </summary>
        /// <param name="expression"></param>
        public Select(string expression)
        {
            Ensure.That(expression, nameof(expression)).IsNotNullOrWhiteSpace();
            Ensure.That(expression, nameof(expression)).Matches($"^{Constants.ValidFieldNamePattern}$");
            Expression = expression;
        }

        /// <inheritdoc/>
        public bool Equals(Select other) => other is not null && Expression == other.Expression;

        /// <inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as Select);

        /// <inheritdoc/>
        public override int GetHashCode() => Expression.GetHashCode();

        /// <inheritdoc/>
        public bool IsEquivalentTo(Select other) => Equals(other);

        /// <inheritdoc/>
        public override string ToString() => this.Jsonify();
    }
}
