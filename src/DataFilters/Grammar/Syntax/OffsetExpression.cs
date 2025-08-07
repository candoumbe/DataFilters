using System;

namespace DataFilters.Grammar.Syntax;
/// <summary>
/// <see cref="OffsetExpression"/> records the offset between a time and the UTC time
/// </summary>
public sealed class OffsetExpression : FilterExpression, IEquatable<OffsetExpression>
{
    /// <summary>
    /// Gets the number of hours of offset with the UTC time
    /// </summary>
    public int Hours { get; }

    /// <summary>
    /// Get the number of minutes of offset with the UTC time
    /// </summary>
    public int Minutes { get; }

    /// <summary>
    /// Sign associated with offset
    /// </summary>
    public NumericSign Sign { get; }

    /// <summary>
    /// The Zero time offset
    /// </summary>
    public static OffsetExpression Zero => new();

    private readonly Lazy<string> _lazyParseableString;

    /// <summary>
    /// Builds a new <see cref="OffsetExpression"/> instance
    /// </summary>
    /// <param name="sign"></param>
    /// <param name="hours"></param>
    /// <param name="minutes">The sign of </param>
    public OffsetExpression(NumericSign sign = NumericSign.Plus, uint hours = 0, uint minutes = 0)
    {
        if (hours > 23)
        {
            throw new ArgumentOutOfRangeException(nameof(hours),
                                                  $"{nameof(hours)} must be between 0 and 23 inclusive");
        }

        if (minutes > 59)
        {
            throw new ArgumentOutOfRangeException(nameof(minutes),
                                                  $"{nameof(minutes)} must be between 0 and 59 inclusive");
        }

        Hours = (int)hours;
        Minutes = (int)minutes;
        Sign = sign;

        _lazyParseableString = new(() => (Hours, Minutes, Sign) switch
        {
            (0, 0, _)                 => "Z",
            (_, _, NumericSign.Minus) => $"-{Hours:D2}:{Minutes:D2}",
            _                         => $"+{Hours:D2}:{Minutes:D2}",
        });
    }

    ///<inheritdoc/>
    public override string EscapedParseableString => _lazyParseableString.Value;

    ///<inheritdoc/>
    public override bool Equals(object obj) => Equals(obj as OffsetExpression);

    ///<inheritdoc/>
    public bool Equals(OffsetExpression other) => (Hours, Minutes, Sign) switch
    {
        (0, 0, _) => other is { Hours: 0, Minutes: 0 },
        _         => (Hours, Minutes, Sign) == (other?.Hours, other?.Minutes, other?.Sign)
    };

    ///<inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Hours, Minutes, Sign);

    ///<inheritdoc/>
    public void Deconstruct(out NumericSign sign, out int hours, out int minutes, out string escapedParseableString,
                            out string originalString)
    {
        sign = Sign;
        hours = Hours;
        minutes = Minutes;
        escapedParseableString = EscapedParseableString;
        originalString = OriginalString;
    }

    /// <inheritdoc />
    public static bool operator ==(OffsetExpression left, OffsetExpression right) => left switch
    {
        null => right is null,
        _    => left.Equals(right)
    };

    /// <inheritdoc />
    public static bool operator !=(OffsetExpression left, OffsetExpression right) => !(left == right);
}