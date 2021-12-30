namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// Base class for filter expression
    /// </summary>
    /// <para>
    /// <see cref="FilterExpression"/>s can take many forms : from the simplest <see cref="ConstantValueExpression"/> to more complex <see cref="GroupExpression"/>s.
    /// </para>
    public abstract class FilterExpression : IHaveComplexity, IParseableString
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
        public virtual bool IsEquivalentTo(FilterExpression other) => Equals(other);

        ///<inheritdoc/>
        public virtual double Complexity => 1;

        ///<inheritdoc/>
        public override string ToString() => $"{GetType().Name}: '{OriginalString}'";

        ///<inheritdoc/>
        public abstract string EscapedParseableString { get; }

        ///<inheritdoc/>
        public virtual string OriginalString => EscapedParseableString;
    }
}