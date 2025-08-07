
using System;

namespace DataFilters.Grammar.Syntax;
/// <summary>
/// a <see cref="PropertyName"/> holds the name of a property a filter is build against.
/// </summary>
public record PropertyName
{
    /// <summary>
    /// Name of the property a filter is applied to
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Builds a new <see cref="PropertyName"/> with the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="name"></param>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="name"/> is <c>string.Empty</c> or contains only whitespaces.</exception>
    public PropertyName(string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentOutOfRangeException(nameof(name));
        }

        Name = name;
    }
}