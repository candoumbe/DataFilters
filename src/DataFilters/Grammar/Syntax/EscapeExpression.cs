using System;
using System.Collections.Generic;
using System.Text;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// An expression that 
    /// </summary>
    public class EscapeExpression : FilterExpression, IEquatable<EscapeExpression>
    {
        public char Value { get; }

        /// <summary>
        /// Builds a new <see cref="EscapeExpression"/>
        /// </summary>
        /// <param name="escapedCharacter"></param>
        public EscapeExpression(char escapedCharacter)
        {
            if (escapedCharacter == default)
            {
                throw new ArgumentOutOfRangeException(nameof(escapedCharacter), escapedCharacter, "cannot be default(char)");
            }

            Value = escapedCharacter;
        }

        public bool Equals(EscapeExpression other) => Equals(other?.Value, Value);

        public override bool Equals(object obj) => Equals(obj as EscapeExpression);

        public override int GetHashCode() => Value.GetHashCode();

        public static bool operator ==(EscapeExpression left, EscapeExpression right) => Equals(left, right);

        public static bool operator !=(EscapeExpression left, EscapeExpression right) => !Equals(left, right);
    }
}
