namespace DataFilters.Casing;

/// <summary>
/// <see cref="PropertyNameResolutionStrategy"/> implementation that leaves the name extracted from querystring unchanged.
/// </summary>
public class DefaultPropertyNameResolutionStrategy : PropertyNameResolutionStrategy
{
    /// <inheritdoc/>
    public override string Handle(string name) => name;
}