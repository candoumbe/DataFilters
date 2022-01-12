#if NETSTANDARD2_0_OR_GREATER || NET
using DataFilters.Casing;

using System;
using System.Runtime.Serialization;
using Ardalis.GuardClauses;

namespace DataFilters;

/// <summary>
/// <see cref="FilterServiceOptions"/> allows to customize the behavior of <see cref="IFilterService"/>
/// </summary>
public class FilterServiceOptions
{
    public const int DefaultCacheSize = 1_000;

    /// <summary>
    /// Defines how the IDataFilterService implementation will handle property names.
    /// </summary>
    /// <remarks>
    ///     The default value is <see cref="PropertyNameResolutionStrategy.Default"/>
    /// </remarks>
    public PropertyNameResolutionStrategy PropertyNameResolutionStrategy
    {
        get => _strategy;
#if NET5_0_OR_GREATER
        init => _strategy = value ?? PropertyNameResolutionStrategy.Default;
#else
        set => _strategy = value ?? PropertyNameResolutionStrategy.Default;
#endif
    }

    private PropertyNameResolutionStrategy _strategy;

    /// <summary>
    /// Defines the number of elements to keep in the local cache
    /// </summary>
#if NET5_0_OR_GREATER
    public int MaxCacheSize { get; init; }
#else
    public int MaxCacheSize { get; set; }
#endif

    /// <summary>
    /// Builds a new <see cref="FilterServiceOptions"/> instance with <see cref="DefaultCacheSize"/> as size
    /// and <see cref="PropertyNameResolutionStrategy.Default"/>.
    /// </summary>
    public FilterServiceOptions() : this(DefaultCacheSize, PropertyNameResolutionStrategy.Default)
    {}

    /// <summary>
    /// Builds a new <see cref="FilterServiceOptions"/> instance specifies <paramref name="maxCacheSize"/>
    /// and used
    /// </summary>
    /// <param name="maxCacheSize">defines how many items the cache can contain at most</param>
    /// <param name="strategy">defines how the <see cref="FilterService"/> will behave when comparing </param>
    public FilterServiceOptions(int maxCacheSize, PropertyNameResolutionStrategy strategy)
    {
        MaxCacheSize = Guard.Against.OutOfRange(maxCacheSize, nameof(maxCacheSize), rangeFrom: 1, rangeTo: int.MaxValue);
        PropertyNameResolutionStrategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    /// <remarks>
    /// This method should be called right after setting all properties.
    /// <para>
    /// <example>
    /// The last line will throw a <see cref="FilterServiceOptionsInvalidValueException"/> because <see cref="MaxCacheSize"/> must be a positive integer
    /// <code>
    /// FilterServiceOptions options = new ()
    /// {
    ///     MaxCacheSize = -3,
    ///     Strategy = PropertyNameResolutionStrategy.CamelCase
    /// };
    ///
    /// options.Validate();
    /// </code>
    /// </example>
    /// </para>
    /// </remarks>
    /// <exception cref="FilterServiceOptionsInvalidValueException">when <see cref="MaxCacheSize"/>'s value is negative or zero</exception>
    public void Validate()
    {
        if (MaxCacheSize < 1)
        {
            throw new FilterServiceOptionsInvalidValueException($"{MaxCacheSize} is not a valid value for {nameof(MaxCacheSize)}");
        }
    }
}
#endif