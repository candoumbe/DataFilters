using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace DataFilters.Grammar.Syntax
{
    public class OneOfExpression : FilterExpression, IEquatable<OneOfExpression>
    {

        private static readonly ArrayEqualityComparer<FilterExpression> equalityComparer = new ArrayEqualityComparer<FilterExpression>();

        public IEnumerable<FilterExpression> Values => _values;

        private readonly FilterExpression[] _values;

        public OneOfExpression(params FilterExpression[] values)
        {
            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }
            _values = values.Where(x => x != null)
                            .ToArray();
        }

        public bool Equals(OneOfExpression other) => other != null && equalityComparer.Equals(_values, other._values);

        /// <inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as OneOfExpression);

        /// <inheritdoc/>
        public override int GetHashCode() => equalityComparer.GetHashCode(_values);

        public static bool operator ==(OneOfExpression left, OneOfExpression right) => EqualityComparer<OneOfExpression>.Default.Equals(left, right);

        public static bool operator !=(OneOfExpression left, OneOfExpression right) => !(left == right);

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

            return equivalent;
        }
    }
}
