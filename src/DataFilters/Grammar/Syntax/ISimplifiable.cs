namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// Marker interface for elements that can be rewritten
    /// </summary>
    public interface ISimplifiable
    {
        /// <summary>
        /// Builds a <see cref="FilterExpression"/> which is equivalent to the current instance but which complexity should be lower.
        /// </summary>
        /// <returns>a rewritten version of the current <see cref="FilterExpression"/> which 
        /// <see cref="FilterExpression.Complexity"/> should be lower.</returns>
        FilterExpression Simplify();
    }
}