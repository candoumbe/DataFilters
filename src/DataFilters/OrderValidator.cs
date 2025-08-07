using System;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;

namespace DataFilters;
/// <summary>
/// Validates sort expression
/// </summary>
public partial class OrderValidator : AbstractValidator<string>
{
    private const string FieldPattern = Filter.ValidFieldNamePattern;
    /// <summary>
    /// Order expression pattern.
    /// </summary>
    public static readonly string Pattern = @$"^\s*(-|\+)?(({FieldPattern})\w*)+(\s*,\s*((-|\+)?(({FieldPattern})\w*)+)\s*)*$";
    private readonly Regex _orderRegex = new(Pattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
    private const char Separator = ',';

    /// <summary>
    /// Builds a new <see cref="OrderValidator"/> instance.
    /// </summary>
    public OrderValidator() => RuleFor(x => x)
        .Matches(Pattern)
        .WithMessage(search =>
        {
            string[] incorrectExpressions = search.Split([Separator])
                // TODO use ZLinq
                    .Where(x => !_orderRegex.IsMatch(x))
                    .Select(x => $"""
                                  "{x}"
                                  """)
                    .ToArray();

            return $"""
                    Sort expression{(incorrectExpressions.Length == 1 ? string.Empty : "s")} {string.Join(", ", incorrectExpressions)}
                    do{(incorrectExpressions.Length == 1 ? "es" : string.Empty)} not match "{Pattern}.
                    """;
        });
}