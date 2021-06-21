using System;
using System.Collections.Generic;

namespace DataFilters.Grammar.Syntax
{

    /// <summary>
    /// Stores a constant value used in a regex expression
    /// </summary>
#if NET5_0_OR_GREATER
    public record RegexConstantValue(char Value) : RegexValue;
#else
    public sealed class RegexConstantValue : RegexValue, IEquatable<RegexConstantValue>
    {
        /// <summary>
        /// Builds a new <see cref="RegexConstantValue"/> instance.
        /// </summary>
        /// <param name="value"></param>
        public RegexConstantValue(char value) => Value = value;

        /// <summary>
        /// Constant value of the regex
        /// </summary>
        public char Value { get; }

        ///<inheritdoc/>
        public bool Equals(RegexConstantValue other) => Value.Equals(other?.Value);

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as RegexConstantValue);

        ///<inheritdoc />
        public static bool operator ==(RegexConstantValue left, RegexConstantValue right) => EqualityComparer<RegexConstantValue>.Default.Equals(left, right);

        ///<inheritdoc />
        public static bool operator !=(RegexConstantValue left, RegexConstantValue right) => !(left == right);

        ///<inheritdoc />
        public override int GetHashCode() => Value.GetHashCode();

        ///<inheritdoc />
        public override string ToString() => $"'{Value}'";
    }
#endif
}