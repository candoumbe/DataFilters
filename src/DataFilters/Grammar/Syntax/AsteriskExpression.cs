using System;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// Expression that defines equality
    /// </summary>
    public class AsteriskExpression : FilterExpression, IEquatable<AsteriskExpression>
    {
        public bool Equals(AsteriskExpression other) => !(other is null);

        public override bool Equals(object obj) => Equals(obj as AsteriskExpression);

        public override int GetHashCode() => 0;
    }
}