using System;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// An expression that holds a constant value
    /// </summary>
    public class ConstantValueExpression : FilterExpression, IEquatable<ConstantValueExpression>, IBoundaryExpression
    {
        public string Value { get; }

        /// <summary>
        /// Builds a new <see cref="ConstantValueExpression"/> that holds the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is <see cref="string.Empty"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        public ConstantValueExpression(string value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(value)} cannot be empty");
            }

            Value = value;
        }

        public bool Equals(ConstantValueExpression other) => Value == other?.Value;

        public override bool Equals(object obj) => Equals(obj as ConstantValueExpression);

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => $"{GetType().Name} : {nameof(Value)} -> {Value}";
    }
}