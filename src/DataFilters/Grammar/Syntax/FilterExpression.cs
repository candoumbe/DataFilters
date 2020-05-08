namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// Base class for filter expression
    /// </summary>
    public abstract class FilterExpression
    {
        /// <summary>
        /// Tests if <paramref name="other"/> is equivalent to the current expression.
        /// 
        /// <para>
        /// The default implementation defers to <see cref="object.Equals(object)"/> implementation.
        /// The meaning of the equivalency of two <see cref="FilterExpression"/>s is left to the implementor. 
        /// </para>
        /// 
        /// </summary>
        /// <param name="other"><see cref="FilterExpression"/> against which the current instance will test is equivalency.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="other"/> is "equivalent" to the current instance.
        /// </returns>
        public virtual bool IsEquivalentTo(FilterExpression other) => Equals(other);
    }
}
