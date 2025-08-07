
using System;

namespace DataFilters.Casing;
/// <summary>
/// <see cref="PropertyNameResolutionStrategy"/> that transform input to it <strong>camelCase</strong> equivalent.
/// </summary>
public class CamelCasePropertyNameResolutionStrategy : PropertyNameResolutionStrategy
{
    ///<inheritdoc/>
    public override string Handle(string name) => name.ToCamelCase();
}