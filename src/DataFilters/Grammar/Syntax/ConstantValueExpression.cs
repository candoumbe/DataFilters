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
        public OneOf<string, DateTime, DateTimeOffset, Guid, double, float, int, long, bool, char> Value { get; }

        private readonly Lazy<string> _lazyParseableString;

        /// <summary>
        /// Builds a new <see cref="ConstantValueExpression"/> that holds the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is <see cref="string.Empty"/> or <paramref name="value"/> is not currently supported
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        public ConstantValueExpression(OneOf<string, DateTime, DateTimeOffset, Guid, double, float, int, long, bool, char> value)
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

            _lazyParseableString = new(() => Value.Match(
                (string stringValue) => stringValue,
                (DateTime dateTimeValue) => dateTimeValue.ToString("s", CultureInfo.InvariantCulture),
                (DateTimeOffset dateTimeOffsetValue) => dateTimeOffsetValue.ToString("s", CultureInfo.InvariantCulture),
                (Guid guidValue) => guidValue.ToString("x"),
                (double doubleValue) => $"{doubleValue.ToString("G17", CultureInfo.InvariantCulture)}D",
                (float floatValue) => $"{floatValue.ToString("G9", CultureInfo.InvariantCulture)}F",
                (int intValue) => intValue.ToString(CultureInfo.InvariantCulture),
                (long longValue) => $"{longValue.ToString(CultureInfo.InvariantCulture)}L",
                (bool boolValue) => boolValue.ToString(),
                (char chrValue) => chrValue.ToString()
            ));
        }

        ///<inheritdoc/>
        public bool Equals(ConstantValueExpression other) => Equals(Value.Value, other?.Value.Value);

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as ConstantValueExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => Value.Value.GetHashCode();

        ///<inheritdoc/>
        public override string ToString() => new { Type = nameof(ConstantValueExpression), Value.Value, ValueType = Value.Value.GetType().Name, Complexity, ParseableString }
#if NETSTANDARD1_3
        .Jsonify()
#elif NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
        .Jsonify(new() { IgnoreNullValues = true })
#endif
            ;

        ///<inheritdoc/>
        public override double Complexity => 1;

        ///<inheritdoc/>
        public override string ParseableString => _lazyParseableString.Value;
    }
}