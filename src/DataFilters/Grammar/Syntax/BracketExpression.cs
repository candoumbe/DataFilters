using System;
using System.Collections.Generic;
using System.Linq;

using Utilities;

namespace DataFilters.Grammar.Syntax;


/// <summary>
/// A <see cref="FilterExpression"/> that holds a regex pattern as its <see cref="Values"/>.
/// </summary>
/// <remarks>
/// As a regular expression can have many form, the constructor gives a way to define if it has :
/// <list type="bullet">
///  <item>a range : <c>[a-f]</c> would be built like <c>new RegularExpression(new RegularRangeValue('a', 'f'))</c></item>
///  <item>a set of values : <c>[aMn]</c> would be built by using <see cref="ConstantBracketValue"/> as constructor parameters</item>
///  <item>a combination of both : <c>[a-fMn]</c> by using<c>{ ('a', 'f', true), ('M', 'M', false), ('n', 'n', false),  }</c> to <see cref="Values"/></item>
/// </list>
/// </remarks>
public sealed class BracketExpression : FilterExpression, IEquatable<BracketExpression>
{
    private static readonly ArrayEqualityComparer<BracketValue> s_equalityComparer = new();

    /// <summary>
    /// Builds a new <see cref="BracketExpression"/> instance.
    /// </summary>
    /// <param name="values"></param>
    public BracketExpression(params BracketValue[] values)
    {
        if (values is null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        Values = values.Where(value => value is not null)
            .ToArray();
    }

    /// <summary>
    /// Values of the original regex
    /// </summary>
    public IEnumerable<BracketValue> Values { get; }

    ///<inheritdoc/>
    public bool Equals(BracketExpression other) => other is not null
                                                   && s_equalityComparer.Equals(Values.ToArray(), other.Values.ToArray());

    ///<inheritdoc/>
    public override bool Equals(object obj) => obj switch
    {
        BracketExpression bracket => Equals(bracket),
        FilterExpression expression => IsEquivalentTo(expression),
        _ => false
    };

    ///<inheritdoc/>
    public override int GetHashCode() => s_equalityComparer.GetHashCode([.. Values]);

    ///<inheritdoc/>
    public override string ToString() => $"{nameof(BracketExpression)} : [{string.Join(",", Values)}]";

    ///<inheritdoc/>
    public override double Complexity => Values.Select(x => x.Complexity)
        .Aggregate((initial, current) => initial * current);

        ///<inheritdoc/>
        public override bool IsEquivalentTo(FilterExpression other)
        {
            bool equivalent = false;
            if (other is not null)
            {
                if (ReferenceEquals(this, other))
                {
                    equivalent = true;
                }
                else if (other is BracketExpression bracketExpression)
                {
                    equivalent = Equals(bracketExpression);
                }
                else if (other is OneOfExpression oneOf)
                {
                    equivalent = oneOf.Values.Exactly(oneOf.Values.OfType<StringValueExpression>().Count())
                        && oneOf.Values.All(x => x is StringValueExpression)
                             && Values.All(value => value switch
                             {
                                 // TODO replace call to .ToStringValue with instanciation of new StringSegmentLinkedList([chr])
                            ConstantBracketValue constant => constant.Value.All(chr => oneOf.Values.Any(expr => expr.As<StringValueExpression>().Value.ToStringValue().Equals(chr.ToString()))),
                                 RangeBracketValue range => Enumerable.Range(range.Start, range.End - range.Start + 1)
                                     .Select(ascii => (char)ascii)
                                     .All(chr => oneOf.Values.Any(expr => expr.As<StringValueExpression>().Value.ToStringValue().Equals(chr.ToString()))),
                                 _ => throw new NotSupportedException("Unsupported value")
                             });
            }
        }

        return equivalent;
    }

    ///<inheritdoc/>
    public override string EscapedParseableString => $"[{string.Join(",", Values)}]";
}