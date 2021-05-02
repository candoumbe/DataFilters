using System;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// An expression that holds a constant value
    /// </summary>
    public sealed class ConstantValueExpression : FilterExpression, IEquatable<ConstantValueExpression>, IBoundaryExpression
    {
        /// <summary>
        /// "Raw" value of the constant
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Builds a new <see cref="ConstantValueExpression"/> that holds the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is <see cref="string.Empty"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        public ConstantValueExpression(object value)
        {
            Value = value switch
            {
                null => throw new ArgumentNullException(nameof(value)),
                short shortValue => shortValue,
                string stringValue when stringValue.Length == 0 => throw new ArgumentOutOfRangeException(nameof(value), value, $"{nameof(value)} cannot be empty"),
                string stringValue => stringValue,
                DateTime dateTimeValue => dateTimeValue,
                DateTimeOffset dateTimeOffsetValue => dateTimeOffsetValue,
                Guid guidValue => guidValue,
                long longValue => longValue,
                bool boolValue => boolValue,
                byte byteValue => byteValue,
                int intValue => intValue,
                _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unsupported type '{value.GetType()}' for {nameof(ConstantValueExpression)}")
            };
        }

        ///<inheritdoc/>
        public bool Equals(ConstantValueExpression other) => Equals(Value, other?.Value);

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as ConstantValueExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => Value.GetHashCode();

        ///<inheritdoc/>
        public override string ToString() => this.Jsonify();
    }
}