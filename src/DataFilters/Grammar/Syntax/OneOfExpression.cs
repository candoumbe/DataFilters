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
        private static readonly ArrayEqualityComparer<FilterExpression> EqualityComparer = new();

        /// <summary>
        /// Collection of <see cref="FilterExpression"/> that the current instance holds.
        /// </summary>
        public IReadOnlyCollection<FilterExpression> Values => _values.ToList().AsReadOnly();

        private readonly FilterExpression[] _values;

        private readonly Lazy<string> _lazyParseableString;

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

            _values = [.. values.Where(x => x is not null)];

            _lazyParseableString = new(() => $"{{{string.Join(",", Values.Select(v => v.EscapedParseableString))}}}");
        }

        /// <inheritdoc/>
        public override bool IsEquivalentTo(FilterExpression other)
        {
            bool equivalent;
            if (ReferenceEquals(this, other))
            {
                equivalent = true;
            }
            else
            {
                equivalent = other is OneOfExpression oneOfExpression
                    ? EqualityComparer.Equals([.. oneOfExpression._values], [.. _values])
                                    || !(_values.Except(oneOfExpression._values).Any() || oneOfExpression._values.Except(_values).Any())
                    : other switch
                    {
                        AsteriskExpression asterisk => Simplify().IsEquivalentTo(asterisk),
                        ConstantValueExpression constant => Simplify().IsEquivalentTo(constant),
                        DateExpression date => Simplify().IsEquivalentTo(date),
                        DateTimeExpression dateTime => Simplify().IsEquivalentTo(dateTime),
                        ISimplifiable simplifiable => Simplify().IsEquivalentTo(simplifiable.Simplify()),
                        _ => false
                    };
            }

            return equivalent;
        }

        ///<inheritdoc/>
        public bool Equals(OneOfExpression other) => other is not null && EqualityComparer.Equals(_values, other._values);

        /// <inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as OneOfExpression);

        /// <inheritdoc/>
        public override int GetHashCode() => EqualityComparer.GetHashCode(_values);

        ///<inheritdoc/>
        public static bool operator ==(OneOfExpression left, OneOfExpression right) => left?.Equals(right) ?? false;

        ///<inheritdoc/>
        public static bool operator !=(OneOfExpression left, OneOfExpression right) => !(left == right);

        ///<inheritdoc/>
        public override double Complexity => Values.Sum(expression => expression.Complexity);

        /// <inheritdoc/>
        public FilterExpression Simplify()
        {
            HashSet<FilterExpression> curatedExpressions = [];

            foreach (IGrouping<bool, FilterExpression> expr in _values.ToLookup(x => x is OneOfExpression))
            {
                if (expr.Key)
                {
                    OneOfExpression oneOfExpression = new(expr.Select(item => ((OneOfExpression)item).Values)
                                                              .SelectMany(x => x)
                                                              .ToArray());

                    curatedExpressions.Add(oneOfExpression.Simplify());
                }
                else
                {
                    foreach (FilterExpression expression in expr)
                    {
                        FilterExpression simplifiedExpression = (expression as ISimplifiable)?.Simplify() ?? expression;
                        if (!curatedExpressions.Contains(simplifiedExpression) && !curatedExpressions.Any(existing => existing.IsEquivalentTo(simplifiedExpression)))
                        {
                            curatedExpressions.Add(simplifiedExpression);
                        }
                    }
                }
            }

            FilterExpression simplifiedResult;

            switch (curatedExpressions.Count)
            {
                case 1:
                    simplifiedResult = curatedExpressions.Single();
                    break;
                case 2:
                    FilterExpression first = curatedExpressions.First();
                    FilterExpression other = curatedExpressions.Last();
                    simplifiedResult = first is OneOfExpression oneOfFirst && other is OneOfExpression oneOfSecond
                        ? new OneOfExpression([.. oneOfFirst.Values, .. oneOfSecond.Values])
                        : new OrExpression(first, other);
                    break;
                default:
                    simplifiedResult = new OneOfExpression([.. curatedExpressions]);
                    break;
            }

            return simplifiedResult;
        }

        ///<inheritdoc/>
        public override string EscapedParseableString => _lazyParseableString.Value;
    }
}
