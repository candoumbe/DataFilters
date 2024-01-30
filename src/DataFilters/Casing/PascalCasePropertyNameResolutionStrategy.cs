namespace DataFilters.Casing;

using System;

/// <summary>
/// <see cref="PropertyNameResolutionStrategy"/> that transform input to it <strong>PascalCase</strong> equivalent.
/// </summary>
public class PascalCasePropertyNameResolutionStrategy : PropertyNameResolutionStrategy
{
    ///<inheritdoc/>
    public override string Handle(string name) => name.ToPascalCase();
}