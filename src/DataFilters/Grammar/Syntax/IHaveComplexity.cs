namespace DataFilters.Grammar.Syntax;

/// <summary>
/// Marks an expression that have a complexity value.
/// </summary>
public interface IHaveComplexity
{
    /// <summary>
    /// Gets the "complexity" of the current instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The complexity is a hint, given two <see cref="FilterExpression"/>s, of which one is the cheapest to compute.
    /// For example, <c>*[Mm]an</c> and <c>*Man|*man</c> are semantically equivalent but the latter is less "complex" than the first because it a combination of 3 primary
    /// <see cref="FilterExpression"/>s (<c>starts with</c>, <c>Bracket</c> and <c>constant</c>).
    /// </para>
    /// </remarks>
#if NET5_0_OR_GREATER
    public virtual double Complexity => 0;
#else
    double Complexity { get; }

#endif
}