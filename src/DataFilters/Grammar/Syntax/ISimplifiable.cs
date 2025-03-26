namespace DataFilters.Grammar.Syntax;

/// <summary>
/// Defines a <see cref="Simplify"/> method which rewrites an instance to a "simpler" form.
/// </summary>
public interface ISimplifiable
{
    /// <summary>
    /// Builds a <see cref="FilterExpression"/> which is equivalent to the current instance but which
    /// <see cref="FilterExpression.Complexity"/> should be lower than the initial <see cref="FilterExpression"/>.
    /// </summary>
    /// <returns>a rewritten version of the current <see cref="FilterExpression"/> which
    /// <see cref="FilterExpression.Complexity"/> should be lower.</returns>
    FilterExpression Simplify();
}