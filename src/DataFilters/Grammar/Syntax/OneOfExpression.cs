
using System;
using System.Collections.Generic;
using System.Linq;

using Utilities;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// a <see cref="FilterExpression"/> that contains multiple <see cref="FilterExpression"/>s as <see cref="Values"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="OneOfExpression"/> indicates that the filter expression should
    /// </remarks>
    public sealed class OneOfExpression : FilterExpression, IEquatable<OneOfExpression>
    {
        private static readonly ArrayEqualityComparer<FilterExpression> equalityComparer = new();

        /// <summary>
        /// Collection of <see cref="FilterExpression"/> that the current instance holds.
        /// </summary>
        public IEnumerable<FilterExpression> Values => _values;

        private readonly FilterExpression[] _values;

        /// <summary>
        /// Builds a new <see cref="OneOfExpression"/> instance.
        /// </summary>
        /// <param name="values"></param>
        /// <exception cref="ArgumentNullException"><paramref name="values"/> is null.</exception>
        public OneOfExpression(params FilterExpression[] values)
        {
            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }
            _values = values.Where(x => x != null)
                            .ToArray();
        }

        /// <inheritdoc/>
        public override bool IsEquivalentTo(FilterExpression other)
        {
            bool equivalent = false;

            if (other is OneOfExpression oneOfExpression)
            {
                if (equalityComparer.Equals(oneOfExpression._values, _values))
                {
                    equivalent = true;
                }
                else
                {
                    equivalent = !(_values.Except(oneOfExpression._values).Any() || oneOfExpression._values.Except(_values).Any());
                }
            }
            else
            {
                int i = 0;
                do
                {
                    equivalent = _values[i].Equals(other);
                    i++;
                }
                while (i < _values.Length && equivalent);
            }

            return equivalent;
        }

        ///<inheritdoc/>
        public bool Equals(OneOfExpression other) => other is not null && equalityComparer.Equals(Values.ToArray(), other.Values.ToArray());

        /// <inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as OneOfExpression);

        /// <inheritdoc/>
        public override int GetHashCode() => equalityComparer.GetHashCode(_values);

        ///<inheritdoc/>
        public static bool operator ==(OneOfExpression left, OneOfExpression right) => left?.Equals(right) ?? false;

        ///<inheritdoc/>
        public static bool operator !=(OneOfExpression left, OneOfExpression right) => !(left == right);

        ///<inheritdoc/>
        public override double Complexity => Values.Sum(expression => expression.Complexity);

        ///<inheritdoc/>
        public override string ToString() => $"{{{nameof(Complexity)}:{Complexity}, {nameof(Values)}: [{string.Join(",", Values)}]}}";
    }
}
