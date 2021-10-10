namespace DataFilters.Grammar.Syntax
{
    using System;

    /// <summary>
    /// An expression that holds a constant value
    /// </summary>
    public abstract class ConstantValueExpression : FilterExpression, IEquatable<ConstantValueExpression>
    {
        /// <summary>
        /// Gets the "raw" value hold by the current instance.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Builds a new <see cref="ConstantValueExpression"/> that holds the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is <see cref="string.Empty"/> or <paramref name="value"/> is not currently supported
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        protected ConstantValueExpression(string value)
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

        ///<inheritdoc/>
        public virtual bool Equals(ConstantValueExpression other) => Equals(Value, other?.Value);

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as ConstantValueExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => Value.GetHashCode();

        ///<inheritdoc/>
        public override double Complexity => 1;

        ///<inheritdoc/>
        public static bool operator ==(ConstantValueExpression left, ConstantValueExpression right)
            => left?.Equals(right) ?? false;

        ///<inheritdoc/>
        public static bool operator !=(ConstantValueExpression left, ConstantValueExpression right)
            => !(left == right);
    }
}