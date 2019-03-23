using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.StringSplitOptions;

namespace DataFilters
{
    /// <summary>
    /// Validates sort expression
    /// </summary>
    public class SortValidator : AbstractValidator<string>
    {
        public const string Pattern = @"^\s*(-|\+)?(([A-Za-z])\w*)+(\s*,\s*((-|\+)?(([A-Za-z])\w*)+)\s*)*$";
        private readonly Regex _sortRegex = new Regex(Pattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
        private const string _separator = ",";

        public SortValidator() => RuleFor(x => x)
                .Matches(Pattern)
                .WithMessage(search =>
                {
                    string[] incorrectExpresions = search.Split(new[] { _separator }, RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .Where(x => !_sortRegex.IsMatch(x))
                        .Select(x => $@"""{x}""")
                        .ToArray();

                    return $"Sort expression{(incorrectExpresions.Length == 1 ? string.Empty : "s")} {string.Join(", ", incorrectExpresions)} " +
                    $@"do{(incorrectExpresions.Length == 1 ? "es" : string.Empty)} not match ""{Pattern}"".";
                });
    }
}
