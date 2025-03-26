namespace DataFilters.Grammar.Syntax;

using System;
using System.Collections.Generic;
using System.Linq;

using Utilities;

/// <summary>
/// a <see cref="FilterExpression"/> that contains multiple <see cref="FilterExpression"/>s as <see cref="Values"/>.
/// </summary>
public sealed class OneOfExpression : FilterExpression, IEquatable<OneOfExpression>, ISimplifiable, IEnumerable<FilterExpression>
{
    private static readonly ArrayEqualityComparer<FilterExpression> EqualityComparer = new();

    /// <summary>
    /// Collection of <see cref="FilterExpression"/> that the current instance holds.
    /// </summary>
    public IReadOnlyList<FilterExpression> Values => [ .. _values];

    private readonly FilterExpression[] _values;

    private readonly Lazy<string> _lazyEscapedParseableString;
    private readonly Lazy<string> _lazyOriginalString;

    /// <summary>
    /// Builds a new <see cref="OneOfExpression"/> instance.
    /// </summary>
    /// <param name="values"></param>
    /// <exception cref="ArgumentNullException"><paramref name="values"/> is <see langword="null"/>.</exception>
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

        _lazyEscapedParseableString = new Lazy<string>(() => $"{{{string.Join("|", Values.Select(v => v.EscapedParseableString))}}}");
        _lazyOriginalString = new Lazy<string>(() => $"{{{string.Join("|", Values.Select(v => v.OriginalString))}}}");
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
                    AsteriskExpression asterisk => Values.All(x => x is AsteriskExpression),
                    ConstantValueExpression constant => Values.All(value => value.IsEquivalentTo(constant)),
                    DateExpression date => Values.All(value => value.IsEquivalentTo(date)),
                    DateTimeExpression dateTime => Values.All(value => value.Equals(dateTime) || value.IsEquivalentTo(dateTime)),
                    OrExpression or => Values.All(value => value.Equals(or.Left) || value.Equals(or.Right) || value.IsEquivalentTo(or.Left) || value.IsEquivalentTo(or.Right)),
                    ISimplifiable simplifiable => Values.All(value => value.IsEquivalentTo(simplifiable.Simplify())),
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
            
        foreach (FilterExpression expression in Values)
        {
            FilterExpression simplified = expression.As<ISimplifiable>()?.Simplify() ?? expression;

            curatedExpressions.RemoveWhere(val => val.Equals(simplified) || (val.IsEquivalentTo(simplified) 
                                                                             && val.Complexity > simplified.Complexity)); 
            curatedExpressions.Add(simplified);

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
                simplifiedResult = new OrExpression(first, other);
                break;
            default:
                simplifiedResult = new OneOfExpression([.. curatedExpressions]);
                break;
        }

        return simplifiedResult;
    }

    ///<inheritdoc/>
    public override string EscapedParseableString => _lazyEscapedParseableString.Value;

    /// <inheritdoc />
    public override string ToString() => _lazyOriginalString.Value;

    /// <inheritdoc />
    IEnumerator<FilterExpression> IEnumerable<FilterExpression>.GetEnumerator()
    {
        foreach (FilterExpression expression in _values)
        {
            yield return expression;
        }
    }

    /// <inheritdoc />
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _values.GetEnumerator();

    /// <inheritdoc />
    public override string ToString(string format, IFormatProvider formatProvider)
    {
        FormattableString formattable = format switch
        {
            "D" or "d" => $"{{{string.Join(", ", Values.Select(expression => $"{expression:d}"))}}}",
            _ => $"{_lazyOriginalString.Value}"
        };

        return formattable.ToString(formatProvider);
    }
}