using System;
using Ardalis.GuardClauses;

namespace DataFilters.ValueObjects;

/// <summary>
/// A value object that wraps a string that has been properly escaped.
/// </summary>
public record EscapedString

{
    /// <summary>
    /// The underlying escaped string value
    /// </summary>
    public string Value { get; }

    private EscapedString(string value) => Value = value;

    /// <summary>
    /// Creates a new <see cref="EscapedString"/> that holds the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>a new <see cref="EscapedString"/>.</returns>
    /// <exception cref="ArgumentNullException">when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static EscapedString From(string value)
        => new (Guard.Against.Null(value));

    /// <inheritdoc />
    public override string ToString() => Value;
}