using DataFilters.Casing;

namespace DataFilters;

/// <summary>
/// Allow to customize the computation of an <see cref="IFilter"/> instance.
/// </summary>
#if NET6_0_OR_GREATER
public record FilterOptions
#else
public class FilterOptions
#endif
{
    private PropertyNameResolutionStrategy _propertyNameResolutionStrategy;

    /// <summary>
    /// Gets or sets the default <see cref="PropertyNameResolutionStrategy"/> to use when matching property names whilst
    /// computing <see cref="IFilter"/> instances.
    /// </summary>
    public PropertyNameResolutionStrategy DefaultPropertyNameResolutionStrategy
    {
        get => _propertyNameResolutionStrategy;
#if !NET6_0_OR_GREATER
        set
#else
        init
#endif
        => _propertyNameResolutionStrategy = value ?? PropertyNameResolutionStrategy.Default;
    }

    /// <summary>
    /// <see cref="FilterLogic"/> to apply when converting a query that could result in a <see cref="MultiFilter"/> instance.
    /// <para>
    /// This will only apply when the logic cannot be determined by the query from which the computation itself
    /// <example>
    /// <code>
    /// FilterOptions options = new() { Logic = FilterLogic.And };
    /// string query = "Nickname=bat&amp;Name=Grayson";
    /// IFilter  filter = query.ToFilter&lt;T&gt;(options);
    /// </code>
    /// </example>
    /// will result in a <see cref="IFilter"/> that is equivalent to
    /// <code>
    /// MultiFilter multifilter = new ()
    /// {
    ///     Logic = FilterLogic.And,
    ///     Filters = new IFilter[]
    ///     {
    ///         new Filter("Nickname", EqualTo, "bat"),
    ///         new Filter("Name", EqualTo, "Grayson"),
    ///     }
    /// }
    /// </code>
    /// whereas
    /// <example>
    /// <code>
    /// FilterOptions options = new() { Logic = FilterLogic.Or };
    /// string query = "Nickname=bat&amp;Name=Grayson";
    /// IFilter  filter = query.ToFilter&lt;T&gt;(options);
    /// </code>
    /// </example>
    /// will result in a <see cref="IFilter"/> that is equivalent to
    /// <code>
    /// MultiFilter multifilter = new ()
    /// {
    ///     Logic = FilterLogic.Or,
    ///     Filters = new IFilter[]
    ///     {
    ///         new Filter("Nickname", EqualTo, "bat"),
    ///         new Filter("Name", EqualTo, "Grayson"),
    ///     }
    /// }
    /// </code>
    /// </para>
    /// </summary>
    /// <remarks>
    /// <see cref="Logic"/> is <see cref="FilterLogic.And"/> by default.
    /// </remarks>
    public FilterLogic Logic
    {
        get;
#if !NET6_0_OR_GREATER
        set;
#else
        init;
#endif
    }

    /// <summary>
    /// Builds a new <see cref="FilterOptions"/> instance.
    /// </summary>
    public FilterOptions()
    {
        Logic = FilterLogic.And;
        DefaultPropertyNameResolutionStrategy = PropertyNameResolutionStrategy.Default;
    }
}
