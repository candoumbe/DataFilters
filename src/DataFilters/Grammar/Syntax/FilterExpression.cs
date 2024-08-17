using System;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// Base class for filter expression
    /// </summary>
    /// <para>
    /// <see cref="FilterExpression"/>s can take many forms : from the simplest <see cref="ConstantValueExpression"/> to more complex <see cref="GroupExpression"/>s.
    /// </para>
    public abstract class FilterExpression : IHaveComplexity, IParseableString, IFormattable
    {
        /// <summary>
        /// Tests if <paramref name="other"/> is equivalent to the current instance.
        /// </summary>
        /// <param name="other"><see cref="FilterExpression"/> against which the current instance will test is equivalency.</param>
        /// <remarks>
        /// The default implementation defers to <see cref="object.Equals(object)"/> implementation.
        /// The meaning of the equivalency of two <see cref="FilterExpression"/>s is left to the implementor.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if <paramref name="other"/> is "equivalent" to the current instance.
        /// </returns>
        public virtual bool IsEquivalentTo(FilterExpression other) => Equals(other) || Equals(other.As<ISimplifiable>()?.Simplify());

        ///<inheritdoc/>
        public virtual double Complexity => 1;

        ///<inheritdoc/>
        public override string ToString() => $"{GetType().Name}: '{OriginalString}'";

        /// <inheritdoc />
        public virtual string ToString(string format, IFormatProvider formatProvider)
        {
            FormattableString formattable = format switch
            {
                "d" or "D" => $"@{GetType().Name}",
                null or "" => $"{ToString()}",
                _ => throw new ArgumentOutOfRangeException(nameof(format), $"Unsupported '{format}' format")
            };

            return formattable.ToString(formatProvider);
        }

        ///<inheritdoc/>
        public abstract string EscapedParseableString { get; }

        ///<inheritdoc/>
        public virtual string OriginalString => EscapedParseableString;

        /// <summary>
        /// Returns a <see cref="FilterExpression"/> that is the result of applying the NOT logical operator to the specified <paramref name="expression"/>.
        /// </summary>
        public static NotExpression operator !(FilterExpression expression) => new(expression);

        /// <summary>
        /// This utility method will simplify the given <paramref name="expression"/>
        /// </summary>
        /// <param name="expression">The expression to "simplify"</param>
        /// <returns>The simplified version of <paramref name="expression"/> if it implements <see cref="ISimplifiable"/> or <paramref name="expression"/> if it cannot be simplified </returns>
        /// <remarks>
        /// The method will try to recursively simplify <paramref name="expression"/>
        /// </remarks>
        protected static FilterExpression SimplifyLocal(FilterExpression expression)
        {
            FilterExpression current = expression;
            FilterExpression previous;

            do
            {
                previous = current;
                current = current.As<ISimplifiable>()?.Simplify() ?? current;
            } while (current.Complexity < previous.Complexity && current is ISimplifiable);

            return current;
        }
    }
}