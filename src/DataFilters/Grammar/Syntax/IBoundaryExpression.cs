namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// Marker interface that identifies types that can be used as <see cref="IntervalExpression"/>'s boundaries
    /// </summary>
    public interface IBoundaryExpression : IHaveComplexity, IParseableString;
}
