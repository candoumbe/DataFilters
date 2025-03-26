namespace DataFilters.Casing;

/// <summary>
/// <para>
/// Base class to derive from to alter the way <see cref="System.StringExtensions.ToFilter{T}(string)"/>
/// and <see cref="System.StringExtensions.ToSort{T}(string)"/> performs property names lookup.
/// </para>
/// </summary>
public abstract class PropertyNameResolutionStrategy
{
    /// <summary>
    /// A casing strategy that leave the fieldname casing unchanged
    /// </summary>
    public static readonly PropertyNameResolutionStrategy Default = new DefaultPropertyNameResolutionStrategy();

    /// <summary>
    /// A resolution strategy that change each name case to conform to camelCasing.
    /// </summary>
    public static readonly PropertyNameResolutionStrategy CamelCase = new CamelCasePropertyNameResolutionStrategy();

    /// <summary>
    /// A resolution strategy that change each name case to conform to pascal casing.
    /// </summary>
    public static readonly PropertyNameResolutionStrategy PascalCase = new PascalCasePropertyNameResolutionStrategy();

    /// <summary>
    /// A casing strategy that change each fieldName case to conform to snake casing.
    /// </summary>
    public static readonly PropertyNameResolutionStrategy SnakeCase = new SnakeCasePropertyNameResolutionStrategy();

    /// <summary>
    /// Performs the desired transformation.
    /// The result of this method will be used instead of name <paramref name="name"/>
    /// </summary>
    /// <param name="name">The name of the key in the query string</param>
    /// <returns>The name to use when looking up for a corresponding property</returns>
    public abstract string Handle(string name);
}