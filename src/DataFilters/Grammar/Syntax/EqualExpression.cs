using System;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// Expression that defines equality
    /// </summary>
    public class EqualExpression : FilterExpression, IEquatable<EqualExpression>
    {
        public bool Equals(EqualExpression other) => !(other is null);

        public override bool Equals(object obj) => Equals(obj as EqualExpression);

        public override int GetHashCode() => nameof(EqualExpression).GetHashCode();
    }
}