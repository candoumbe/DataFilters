using System;
using System.Collections.Generic;
using System.Linq;

using Utilities;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// A <see cref="FilterExpression"/> that holds a regex pattern as its <see cref="Value"/>.
    /// </summary>
    /// <remarks>
    /// As a regular expression can have many form, the constructor gives a way to define if it has :
    /// <list type="bullet">
    ///  <item>a range : <c>[a-f]</c> would be built like <c>new RegularExpression(new RegularRangeValue('a', 'f'))</c></item>
    ///  <item>a set of values : <c>[aMn]</c> would be built by using <see cref="ConstantBracketValue"/> as constructor parameters</item>
    ///  <item>a combination of both : <c>[a-fMn]</c> by using<c>{ ('a', 'f', true), ('M', 'M', false), ('n', 'n', false),  }</c> to <see cref="Value"/></item>
    /// </list>
    /// </remarks>
    public sealed class BracketExpression : FilterExpression, IEquatable<BracketExpression>, IHaveComplexity
    {
        /// <summary>
        /// Builds a new <see cref="BracketExpression"/> instance.
        /// </summary>
        /// <param name="value"></param>
        public BracketExpression(BracketValue value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Values of the original regex
        /// </summary>
        public BracketValue Value { get; }

        ///<inheritdoc/>
        public bool Equals(BracketExpression other) => Value.Equals(other?.Value);

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as BracketExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => Value.GetHashCode();

        ///<inheritdoc/>
        public override string ToString() => $"{nameof(BracketExpression)} : [{string.Join(",", Value)}]";

        ///<inheritdoc/>
        public override double Complexity => Value switch {
            ConstantBracketValue constant => 1.5 * constant.Value.Length,
            RangeBracketValue  range  => 1.5 * (range.End - range.Start + 1),
            _ => throw new NotSupportedException("Unsupported value")
        };

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
                    equivalent = oneOf.Values.Exactly(oneOf.Values.OfType<ConstantValueExpression>().Count())
                        && oneOf.Values.All(x => x is ConstantValueExpression constant && constant.Value is string)
                        && Value switch
                        {
                            ConstantBracketValue constant => constant.Value.All(chr => oneOf.Values.Any(expr => expr.As<ConstantValueExpression>().Value.Equals(chr.ToString()))),
                            RangeBracketValue range => Enumerable.Range(range.Start, range.End - range.Start + 1)
                                                                 .Select(ascii => (char)ascii)
                                                                 .All(chr => oneOf.Values.Any(expr => expr.As<ConstantValueExpression>().Value.Equals(chr.ToString()))),
                            _ => throw new NotSupportedException("Unsupported value")
                        };
                }
            }

            return equivalent;
        }
    }
}