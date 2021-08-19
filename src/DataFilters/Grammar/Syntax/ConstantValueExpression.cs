namespace DataFilters.Grammar.Syntax
{

    using System;
    using System.Globalization;

    using OneOf;

    /// <summary>
    /// An expression that holds a constant value
    /// </summary>
    public sealed class ConstantValueExpression : FilterExpression, IEquatable<ConstantValueExpression>, IBoundaryExpression
    {
        /// <summary>
        /// Gets the "raw" value hold by the current instance.
        /// </summary>
        public OneOf<string, DateTime, DateTimeOffset, short, int, long, Guid, decimal, double, float, bool, byte, char> Value { get; }

        /// <summary>
        /// Builds a new <see cref="ConstantValueExpression"/> that holds the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is <see cref="string.Empty"/> or <paramref name="value"/> is not currently supported
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        public ConstantValueExpression(OneOf<string, DateTime, DateTimeOffset, short, int, long, Guid, decimal, double, float,bool, byte, char> value)
        {

            if (value.Value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value.Value is string stringInput && string.IsNullOrEmpty(stringInput))
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            Value = value;
        }

        ///<inheritdoc/>
        public bool Equals(ConstantValueExpression other) => Equals(Value.Value, other?.Value.Value);

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as ConstantValueExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => Value.Value.GetHashCode();

        ///<inheritdoc/>
        public override string ToString() => new { Type = nameof(ConstantValueExpression), Value.Value, Complexity }.Jsonify();

        ///<inheritdoc/>
        public override double Complexity => 1;

        ///<inheritdoc/>
        public override string ParseableString => Value.Match(
            (string stringValue) => stringValue,
            (DateTime dateTimeValue) => dateTimeValue.ToString(CultureInfo.InvariantCulture),
            (DateTimeOffset dateTimeOffsetValue) => dateTimeOffsetValue.ToString(CultureInfo.InvariantCulture),
            (short shortValue) => shortValue.ToString(CultureInfo.InvariantCulture),
            (int intValue) => intValue.ToString(CultureInfo.InvariantCulture),
            (long longValue) => longValue.ToString(CultureInfo.InvariantCulture),
            (Guid guidValue) => guidValue.ToString("x"),
            (decimal decimalValue) => decimalValue.ToString(CultureInfo.InvariantCulture),
            (double doubleValue) => doubleValue.ToString(CultureInfo.InvariantCulture),
            (float floatValue) => floatValue.ToString(CultureInfo.InvariantCulture),
            (bool boolValue) => boolValue.ToString(),
            (byte byteValue) => byteValue.ToString(),
            (char chrValue) => chrValue.ToString()
        );
    }
}