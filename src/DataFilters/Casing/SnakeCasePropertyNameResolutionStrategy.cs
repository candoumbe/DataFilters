namespace DataFilters.Casing
{
    using System;

    /// <summary>
    /// <see cref="PropertyNameResolutionStrategy"/> that transform input to its snake_case equivalent.
    /// </summary>
    public class SnakeCasePropertyNameResolutionStrategy : PropertyNameResolutionStrategy
    {
        ///<inheritdoc/>
        public override string Handle(string name) => name.ToSnakeCase();
    }
}