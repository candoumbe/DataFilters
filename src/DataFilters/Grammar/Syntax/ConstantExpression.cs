using System;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// An expression that holds a constant value
    /// </summary>
    public class ConstantExpression : FilterExpression, IEquatable<ConstantExpression>
    {
        public string Value { get; }

        /// <summary>
        /// Builds a new <see cref="ConstantExpression"/> that holds the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is <see cref="string.Empty"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        public ConstantExpression(string value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            Value = value;
        }

        public bool Equals(ConstantExpression other) => Value == other?.Value;

        public override bool Equals(object obj) => Equals(obj as ConstantExpression);

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => $"{GetType().Name} : {nameof(Value)} -> {Value}";
    }
}