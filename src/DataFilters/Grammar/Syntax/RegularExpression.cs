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
    ///  <item>a set of values : <c>[aMn]</c> would be built by using <see cref="RegexConstantValue"/> as constructor parameters</item>
    ///  <item>a combination of both : <c>[a-fMn]</c> by using<c>{ ('a', 'f', true), ('M', 'M', false), ('n', 'n', false),  }</c> to <see cref="Values"/></item>
    /// </list>
    /// </remarks>
    public sealed class RegularExpression : FilterExpression, IEquatable<RegularExpression>, IHaveComplexity
    {
        private static readonly ArrayEqualityComparer<RegexValue> _equalityComparer = new();

        /// <summary>
        /// Builds a new <see cref="RegularExpression"/> instance
        /// </summary>
        public RegularExpression(params RegexValue[] values)
        {
            Values = values.Where(x => !Equals(x, default))
                           .ToArray();
        }

        /// <summary>
        /// Values of the original regex
        /// </summary>
        public IReadOnlyList<RegexValue> Values { get; }

        ///<inheritdoc/>
        public bool Equals(RegularExpression other) => other is not null && _equalityComparer.Equals(Values.ToArray(), other.Values.ToArray());

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as RegularExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => Values.GetHashCode();

        ///<inheritdoc/>
        public override string ToString() => $"{nameof(RegularExpression)} : [{string.Join(",", Values)}]";

        ///<inheritdoc/>
        public override double Complexity => Values.Sum(value => value switch
        {
            RegexConstantValue constantValue => 1.5,
            RegexRangeValue regexRange => Enumerable.Range((int)regexRange.Start, regexRange.End - regexRange.Start + 1)
                                                    .Sum() * 1.5,
            _ => throw new NotSupportedException("Unsupported regex expression type")
        });
    }
}