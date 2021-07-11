namespace DataFilters.Grammar.Syntax
{
    using System;

    /// <summary>
    /// An expression that holds a constant value
    /// </summary>
    public sealed class ConstantValueExpression : FilterExpression, IEquatable<ConstantValueExpression>, IBoundaryExpression
    {
        /// <summary>
        /// Gets the "raw" value hold by the current instance.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Builds a new <see cref="ConstantValueExpression"/> that holds the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is <see cref="string.Empty"/> or <paramref name="value"/> is not currently supported
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        private ConstantValueExpression(object value)
        {
            Value = value switch
            {
                short shortValue => shortValue,
                string stringValue when stringValue.Length == 0 => throw new ArgumentOutOfRangeException(nameof(value), value, $"{nameof(value)} cannot be empty"),
                string stringValue => stringValue,
                DateTime dateTimeValue => dateTimeValue,
                DateTimeOffset dateTimeOffsetValue => dateTimeOffsetValue,
                Guid guidValue => guidValue,
                decimal decimalValue => decimalValue,
                double doubleValue => doubleValue,
                long longValue => longValue,
                bool boolValue => boolValue,
                byte byteValue => byteValue,
                int intValue => intValue,
                char chrValue => chrValue,
                null => throw new ArgumentNullException(nameof(value)),
                _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unsupported type '{value.GetType()}' for {nameof(ConstantValueExpression)}")
            };
        }

        /// <summary>
        /// Builds a new <see cref="ConstantValueExpression"/> that holds the specified <see cref="string"/> <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is <see cref="string.Empty"/> or <paramref name="value"/> is not currently supported
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        public ConstantValueExpression(string value) : this((object)value) { }

        /// <summary>
        /// Builds a new <see cref="ConstantValueExpression"/> that holds the specified <see cref="decimal"/> <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        public ConstantValueExpression(decimal value) : this((object)value) { }

        /// <summary>
        /// Builds a new <see cref="ConstantValueExpression"/> that holds the specified <see cref="float"/> <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        public ConstantValueExpression(double value) : this((object)value) { }


        /// <summary>
        /// Builds a new <see cref="ConstantValueExpression"/> that holds the specified <see cref="DateTime"/> <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is <see cref="string.Empty"/> or <paramref name="value"/> is not currently supported
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        public ConstantValueExpression(DateTime value) : this((object)value) { }

        /// <summary>
        /// Builds a new <see cref="ConstantValueExpression"/> that holds the specified <see cref="DateTimeOffset"/> <paramref name="value"/>.
        /// </summary>
        public ConstantValueExpression(DateTimeOffset value) : this((object)value) { }

        /// <summary>
        /// Builds a new <see cref="ConstantValueExpression"/> that holds the specified <see cref="long"/> <paramref name="value"/>.
        /// </summary>
        public ConstantValueExpression(long value) : this((object)value) { }

        /// <summary>
        /// Builds a new <see cref="ConstantValueExpression"/> that holds the specified <see cref="int"/> <paramref name="value"/>.
        /// </summary>
        public ConstantValueExpression(int value) : this((object)value) { }

        /// <summary>
        /// Builds a new <see cref="ConstantValueExpression"/> that holds the specified <see cref="byte"/> <paramref name="value"/>.
        /// </summary>
        public ConstantValueExpression(byte value) : this((object)value) { }

        /// <summary>
        /// Builds a new <see cref="ConstantValueExpression"/> that holds the specified <see cref="Guid"/> <paramref name="value"/>.
        /// </summary>
        public ConstantValueExpression(Guid value) : this((object)value) { }

        /// <summary>
        /// Builds a new <see cref="ConstantValueExpression"/> that holds the specified <see cref="Guid"/> <paramref name="value"/>.
        /// </summary>
        public ConstantValueExpression(bool value) : this((object)value) { }

        ///<inheritdoc/>
        public bool Equals(ConstantValueExpression other) => Equals(Value, other?.Value);

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as ConstantValueExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => Value.GetHashCode();

        ///<inheritdoc/>
        public override string ToString() => this.Jsonify();

        ///<inheritdoc/>
        public override double Complexity => 1;
    }
}