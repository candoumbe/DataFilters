using System;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// A <see cref="FilterExpression"/> that holds a string value
    /// </summary>
#if NETSTANDARD1_3
    public sealed class ContainsExpression : FilterExpression, IEquatable<ContainsExpression>
#else
    public record ContainsExpression : FilterExpression, IEquatable<ContainsExpression>
#endif
    {
        /// <summary>
        /// The value that was between two <see cref="AsteriskExpression"/>
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Builds a new <see cref="ContainsExpression"/> that holds the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException">if <paramref name="value"/> is <c>null</c></exception>
        /// <exception cref="ArgumentOutOfRangeException">if <paramref name="value"/> is <c>empty</c></exception>
        public ContainsExpression(string value)
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

        ///<inheritdoc/>
        public override double Complexity => 1.5;

#if NETSTANDARD1_3
        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as ContainsExpression);

        ///<inheritdoc/>
        public bool Equals(ContainsExpression other) => Value == other?.Value;

        ///<inheritdoc/>
        public override int GetHashCode() => Value.GetHashCode();

        ///<inheritdoc/>
        public override string ToString() => this.Jsonify();

#endif
    }
}