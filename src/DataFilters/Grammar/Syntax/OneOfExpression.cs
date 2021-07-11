namespace DataFilters.Grammar.Syntax
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Utilities;

    /// <summary>
    /// a <see cref="FilterExpression"/> that contains multiple <see cref="FilterExpression"/>s as <see cref="Values"/>.
    /// </summary>
    public sealed class OneOfExpression : FilterExpression, IEquatable<OneOfExpression>, ISimplifiable
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
        /// <exception cref="ArgumentNullException"><paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="values"/> is empty. </exception>
        public OneOfExpression(params FilterExpression[] values)
        {
            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (values.Length == 0)
            {
                throw new InvalidOperationException($"{nameof(OneOfExpression)} cannot be empty");
            }

            _values = values.Where(x => x != null)
                            .ToArray();
        }

        /// <inheritdoc/>
        public override bool IsEquivalentTo(FilterExpression other)
        {
            bool equivalent;
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
                    equivalent = _values[i].IsEquivalentTo(other);
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

        /// <summary>
        /// Reduces the complexity of the current expression.
        /// </summary>
        public FilterExpression Simplify()
        {
            ISet<FilterExpression> curatedExpressions = new HashSet<FilterExpression>();

            foreach (FilterExpression expr in _values.Distinct())
            {
                FilterExpression simplifiedExpression = (expr as ISimplifiable)?.Simplify() ?? expr;
                if (!curatedExpressions.Contains(simplifiedExpression) && !curatedExpressions.Any(existing => existing.IsEquivalentTo(simplifiedExpression)))
                {
                    curatedExpressions.Add(simplifiedExpression);
                }
            }

            return curatedExpressions.Count switch
            {
                1 => curatedExpressions.Single(),
                2 => new OrExpression(curatedExpressions.First(), curatedExpressions.Last()),
                _ => new OneOfExpression(curatedExpressions.ToArray()),
            };
        }
    }
}
