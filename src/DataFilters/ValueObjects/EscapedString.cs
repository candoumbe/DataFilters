using System;
using Ardalis.GuardClauses;

namespace DataFilters.ValueObjects;

public interface IString
{
    string Value { get; }


    string ToString() => Value;
}

/// <summary>
/// A value object that wraps a string that has been properly escaped.
/// </summary>
public class EscapedString : IEquatable<EscapedString>, IString

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
    public static EscapedString From(string value) => new (Guard.Against.Null(value));

    /// <summary>
    /// Checks if the current instance holds an empty <see cref="Value"/>.
    /// </summary>
    /// <returns><see langword="true"/> if <see cref="Value"/> is empty and <see langword="false"/> otherwise.</returns>
    public bool IsEmpty() => Value == string.Empty;

    /// <inheritdoc />
    public bool Equals(EscapedString other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Value == other.Value;
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        return obj.GetType() == GetType() && Equals((EscapedString)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode() =>  Value?.GetHashCode() ??  0  ;

    public static bool operator ==(EscapedString left, EscapedString right) => Equals(left, right);

    public static bool operator !=(EscapedString left, EscapedString right) => !Equals(left, right);

    /// <inheritdoc />
    public override string ToString() => Value;
}