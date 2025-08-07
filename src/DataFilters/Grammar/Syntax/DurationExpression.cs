
using System;

namespace DataFilters.Grammar.Syntax;
/// <summary>
/// A <see cref="FilterExpression"/> implementation that contains values associated to a duration (see "https://en.wikipedia.org/wiki/ISO_8601#Durations")
/// </summary>
public sealed class DurationExpression : FilterExpression, IEquatable<DurationExpression>, IFormattable
{
    /// <summary>
    /// Years part of the expression
    /// </summary>
    public int Years { get; }

    /// <summary>
    /// "Months" part of the expression
    /// </summary>
    public int Months { get; }

    /// <summary>
    /// "Days" part of the expression
    /// </summary>
    public int Days { get; }

    /// <summary>
    /// "Hours" part of the expression
    /// </summary>
    public int Hours { get; }

    /// <summary>
    /// "Minutes" part of the expression
    /// </summary>
    public int Minutes { get; }

    /// <summary>
    /// Seconds part of the expression
    /// </summary>
    public int Seconds { get; }

    private readonly Lazy<string> _lazyEscapedParseableString;

    /// <summary>
    /// Weeks part of the expression
    /// </summary>
    public int Weeks { get; }

    /// <summary>
    /// Builds a new <see cref="DurationExpression"/> instance
    /// </summary>
    /// <param name="years"></param>
    /// <param name="months"></param>
    /// <param name="weeks"></param>
    /// <param name="days"></param>
    /// <param name="hours"></param>
    /// <param name="minutes"></param>
    /// <param name="seconds"></param>
    public DurationExpression(int years = 0, int months = 0, int weeks = 0, int days = 0, int hours = 0, int minutes = 0, int seconds = 0)
    {
        if (years < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(years), years, $"{nameof(years)} cannot be negative");
        }

        if (months < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(months), months, $"{nameof(months)} cannot be negative");
        }

        if (weeks < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(weeks), weeks, $"{nameof(weeks)} cannot be negative");
        }

        if (days < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(days), days, $"{nameof(days)} cannot be negative");
        }

        if (hours < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(hours), hours, $"{nameof(hours)} cannot be negative");
        }

        if (minutes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minutes), minutes, $"{nameof(minutes)} cannot be negative");
        }

        if (seconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(seconds), seconds, $"{nameof(seconds)} cannot be negative");
        }

        (Years, Months, Weeks, Days, Hours, Minutes, Seconds) = (years, months, weeks, days, hours, minutes, seconds);

        _lazyEscapedParseableString = new Lazy<string>(() => (Years, Months, Weeks, Days, Hours, Minutes, Seconds) switch
        {
            (0, 0, 0, 0, 0, 0, 0) => "PT0S",
            _ => $"P{(Years > 0 ? $"{Years}Y" : string.Empty)}{(Months > 0 ? $"{Months}M" : string.Empty)}{(Weeks > 0 ? $"{Weeks}W" : string.Empty)}{(Days > 0 ? $"{Days}D" : string.Empty)}T{(Hours > 0 ? $"{Hours}H" : string.Empty)}{(Minutes > 0 ? $"{Minutes}M" : string.Empty)}{(Seconds > 0 ? $"{Seconds}S" : string.Empty)}"
        });
    }

    ///<inheritdoc/>
    public override bool IsEquivalentTo(FilterExpression other)
    {
        bool equivalent = false;

        if (((other as ISimplifiable)?.Simplify() ?? other) is DurationExpression otherDuration)
        {
            equivalent = Equals(otherDuration);
            if (!equivalent)
            {
                DateTime otherDateTime = ConvertToDateTime(otherDuration);
                DateTime current = ConvertToDateTime(this);

                equivalent = (current - otherDateTime) == TimeSpan.Zero;
            }
        }

        return equivalent;

        static DateTime ConvertToDateTime(DurationExpression duration)
        {
            return DateTime.MinValue.AddYears(duration.Years)
                .AddMonths(duration.Months)
                .AddDays((duration.Weeks * 7) + duration.Days)
                .AddHours(duration.Hours)
                .AddMinutes(duration.Minutes)
                .AddSeconds(duration.Seconds);
        }
    }

    ///<inheritdoc/>
    public override bool Equals(object obj) => Equals(obj as DurationExpression);

    ///<inheritdoc/>
    public bool Equals(DurationExpression other) => (Years, Months, Weeks, Days, Hours, Minutes, Seconds) == (other?.Years, other?.Months, other?.Weeks, other?.Days, other?.Hours, other?.Minutes, other?.Seconds);

    ///<inheritdoc/>
    public override int GetHashCode() => (Years, Months, Weeks, Days, Hours, Minutes, Seconds).GetHashCode();

    ///<inheritdoc/>
    public override string EscapedParseableString => _lazyEscapedParseableString.Value;
}