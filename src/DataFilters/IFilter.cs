using System;

namespace DataFilters;

/// <summary>
/// Defines the basic shape of a filter.
/// </summary>
public interface IFilter : IEquatable<IFilter>
{
    /// <summary>
    /// Gets the JSON representation of the filter
    /// </summary>
    /// <returns>A json representation of the current instance.</returns>
    string ToJson();

    /// <summary>
    /// Computes a new <see cref="IFilter"/> instance which is the exact opposite of the current instance.
    /// </summary>
    /// <returns>The exact opposite of the current instance.</returns>
    IFilter Negate();

    ///<inheritdoc/>
    public void ToString() => ToJson();
}