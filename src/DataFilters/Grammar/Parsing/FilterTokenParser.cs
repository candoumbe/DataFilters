using DataFilters.Grammar.Syntax;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataFilters.Grammar.Parsing
{
    /// <summary>
    /// Parses a tree of <see cref="FilterToken"/> into <see cref="FilterExpression"/>
    /// </summary>
    public static class FilterTokenParser
    {
        /// <summary>
        /// Parser for <see cref="FilterToken.Alpha"/>
        /// </summary>
        public static TokenListParser<FilterToken, ConstantValueExpression> AlphaNumeric => from data in (
                                                                                            from numberBefore in Token.EqualTo(FilterToken.Numeric).Optional()
                                                                                            from value in Token.EqualTo(FilterToken.Alpha)
                                                                                                .Or(Token.EqualTo(FilterToken.Escaped)).Optional()
                                                                                            from numberAfter in Token.EqualTo(FilterToken.Numeric).Optional()
                                                                                            where numberBefore != null || value != null || numberAfter != null
                                                                                            select new { numberBefore, value, numberAfter }
                                                                                            ).AtLeastOnce()
                                                                                            select new ConstantValueExpression(
                                                                                                string.Concat(
                                                                                                    data.Select(d => string.Concat(
                                                                                                        d.numberBefore?.ToStringValue() ?? string.Empty,
                                                                                                        d.value?.ToStringValue() ?? string.Empty,
                                                                                                        d.numberAfter?.ToStringValue() ?? string.Empty)))
                                                                                            );

        /// <summary>
        /// Parser for '*' character
        /// </summary>
        private static TokenListParser<FilterToken, AsteriskExpression> Asterisk => from __ in Token.EqualTo(FilterToken.Asterisk)
                                                                                    select new AsteriskExpression();

        /// <summary>
        /// Parser for "starts with" expressions
        /// </summary>
        public static TokenListParser<FilterToken, StartsWithExpression> StartsWith => from data in (
                                                                                           from puncBefore in Punctuation.Many()
                                                                                           from alpha in AlphaNumeric.Many()
                                                                                           from puncAfter in Punctuation.Many()
                                                                                           where puncBefore.Length > 0 || alpha.Length > 0 || puncAfter.Length > 0
                                                                                           select new
                                                                                           {
                                                                                               value = string.Concat(
                                                                                                   string.Concat(puncBefore.Select(x => x.Value)),
                                                                                                   string.Concat(alpha.Select(item => item.Value)),
                                                                                                   string.Concat(puncAfter.Select(x => x.Value)))
                                                                                           }).AtLeastOnce()
                                                                                       from escaped in Token.EqualTo(FilterToken.Backslash).Many()
                                                                                       from _ in Asterisk
                                                                                       where escaped.Length == 0
                                                                                       select new StartsWithExpression(string.Concat(data.Select(x => x.value)));

        /// <summary>
        /// Parser for "starts with" expressions
        /// </summary>
        public static TokenListParser<FilterToken, EndsWithExpression> EndsWith => from _ in Asterisk
                                                                                   from data in (
                                                                                       from puncBefore in Punctuation.Many()
                                                                                       from alpha in AlphaNumeric.Many()
                                                                                       from puncAfter in Punctuation.Many()
                                                                                       where puncBefore.Length > 0 || alpha.Length > 0 || puncAfter.Length > 0
                                                                                       select new
                                                                                       {
                                                                                           value = string.Concat(
                                                                                               string.Concat(puncBefore.Select(x => x.Value)),
                                                                                               string.Concat(alpha.Select(item => item.Value)),
                                                                                               string.Concat(puncAfter.Select(x => x.Value)))
                                                                                       }
                                                                                       ).AtLeastOnce()
                                                                                   select new EndsWithExpression(string.Concat(data.Select(x => x.value)));

        /// <summary>
        /// Parser for "contains" expression
        /// </summary>
        public static TokenListParser<FilterToken, ContainsExpression> Contains => from _ in Asterisk
                                                                                   from data in (
                                                                                   from puncBefore in Punctuation.Many()
                                                                                   from alpha in AlphaNumeric.Many()
                                                                                   from puncAfter in Punctuation.Many()
                                                                                   where puncBefore.Length > 0 || alpha.Length > 0 || puncAfter.Length > 0
                                                                                   select new
                                                                                   {
                                                                                       value = string.Concat(
                                                                                           string.Concat(puncBefore.Select(x => x.Value)),
                                                                                           string.Concat(alpha.Select(item => item.Value)),
                                                                                           string.Concat(puncAfter.Select(x => x.Value)))
                                                                                   }).AtLeastOnce()
                                                                                   from escaped in Token.EqualTo(FilterToken.Backslash).Many()
                                                                                   from __ in Asterisk
                                                                                   select new ContainsExpression(string.Concat(data.Select(x => x.value)));

        public static TokenListParser<FilterToken, OrExpression> Or => from left in Expression
                                                                       from _ in Token.EqualTo(FilterToken.Or)
                                                                       from right in Expression
                                                                       select new OrExpression(left, right);

        /// <summary>
        /// Parser for logical AND expression
        /// </summary>
        public static TokenListParser<FilterToken, AndExpression> And => Parse.Chain(Token.EqualTo(FilterToken.And),
             (from left in Expression
              from _ in Token.EqualTo(FilterToken.And)
              from right in Expression
              select new AndExpression(left, right)).Try()
            .Or(from left in AlphaNumeric
                from _ in Asterisk
                from right in AlphaNumeric
                select new AndExpression(new StartsWithExpression(left.Value), new EndsWithExpression(right.Value))),
            (_, left, right) => new AndExpression(left, right))
            ;

        /// <summary>
        /// Parser for NOT expression
        /// </summary>
        public static TokenListParser<FilterToken, NotExpression> Not => from _ in Token.EqualTo(FilterToken.Not)
                                                                         from expression in Expression
                                                                         select new NotExpression(expression);

        private static TokenListParser<FilterToken, RegularExpression> Regex => from start in Token.EqualTo(FilterToken.OpenSquaredBracket)
                                                                                from regex in AlphaNumeric
                                                                                from end in Token.EqualTo(FilterToken.CloseSquaredBracket)
                                                                                select new RegularExpression(regex.Value);

        /// <summary>
        /// Parses Range expressions
        /// </summary>
        public static TokenListParser<FilterToken, RangeExpression> Range
        {
            get
            {
                return  // Case [ min TO max ] 
                        (
                            // from prop in Property
                            // from eq in Token.EqualTo(FilterToken.Equal)
                            // from _ in AnyExpression.Many()
                            from start in Token.EqualTo(FilterToken.OpenSquaredBracket)
                            from min in DateAndTime.Try()
                                                    .Cast<FilterToken, DateTimeExpression, FilterExpression>()
                                                    .Or(Number.Try().Cast<FilterToken, ConstantValueExpression, FilterExpression>())
                                                    .Or(AlphaNumeric.Cast<FilterToken, ConstantValueExpression, FilterExpression>())

                            from rangeSeparator in RangeSeparator

                            from max in DateAndTime.Try()
                                                    .Cast<FilterToken, DateTimeExpression, FilterExpression>()
                                                    .Or(Number.Try().Cast<FilterToken, ConstantValueExpression, FilterExpression>())
                                                    .Or(AlphaNumeric.Cast<FilterToken, ConstantValueExpression, FilterExpression>())

                            from end in Token.EqualTo(FilterToken.CloseSquaredBracket)

                            where min != default || max != default
                            select new RangeExpression(
                                min: new BoundaryExpression(min switch
                                {
                                    ConstantValueExpression constant => constant,
                                    DateTimeExpression dateTime => dateTime,
                                    _ => throw new ArgumentOutOfRangeException($"Unsupported '{min?.GetType()}' for min value")
                                }, included: true),
                                max: new BoundaryExpression(max switch
                                {
                                    ConstantValueExpression constant => constant,
                                    DateTimeExpression dateTime => dateTime,
                                    _ => throw new ArgumentOutOfRangeException($"Unsupported '{max?.GetType()}' for max value")
                                }, included: true)
                            )
                        ).Try()
                      // Case ] min TO max ] : lower bound excluded from the range
                      .Or(
                            // from prop in Property
                            // from eq in Token.EqualTo(FilterToken.Equal)
                            // from _ in AnyExpression.Many()
                            from start in Token.EqualTo(FilterToken.CloseSquaredBracket)
                            from min in DateAndTime.Try().Cast<FilterToken, DateTimeExpression, FilterExpression>()
                                                   .Or(Number.Try().Cast<FilterToken, ConstantValueExpression, FilterExpression>())
                                                   .Or(Asterisk.Try().Cast<FilterToken, AsteriskExpression, FilterExpression>())
                                                   .Or(AlphaNumeric.Try().Cast<FilterToken, ConstantValueExpression, FilterExpression>())

                            from rangeSeparator in RangeSeparator

                            from max in DateAndTime.Try().Cast<FilterToken, DateTimeExpression, FilterExpression>()
                                                   .Or(Number.Try().Cast<FilterToken, ConstantValueExpression, FilterExpression>())
                                                   .Or(AlphaNumeric.Cast<FilterToken, ConstantValueExpression, FilterExpression>())

                            from end in Token.EqualTo(FilterToken.CloseSquaredBracket)

                            where min != default || max != default
                            select new RangeExpression(
                                min: new BoundaryExpression(min switch
                                {
                                    AsteriskExpression asterisk => asterisk,
                                    ConstantValueExpression constant => constant,
                                    DateTimeExpression dateTime => dateTime,
                                    _ => throw new ArgumentOutOfRangeException($"Unsupported '{min?.GetType()}' for min value")
                                }, included: false),
                                max: new BoundaryExpression(max switch
                                {
                                    ConstantValueExpression constant => constant,
                                    DateTimeExpression dateTime => dateTime,
                                    _ => throw new ArgumentOutOfRangeException($"Unsupported '{max?.GetType()}' for max value")
                                }, included: true)
                           )
                        ).Try()
                      // Case syntax [ min TO max [ : upper bound excluded from the range
                      .Or(
                            // from prop in Property
                            // from eq in Token.EqualTo(FilterToken.Equal)
                            // from _ in AnyExpression.Many()
                            from start in Token.EqualTo(FilterToken.OpenSquaredBracket)
                            from min in DateAndTime.Try().Cast<FilterToken, DateTimeExpression, FilterExpression>()
                                                   .Or(Number.Try().Cast<FilterToken, ConstantValueExpression, FilterExpression>())
                                                   .Or(AlphaNumeric.Try().Cast<FilterToken, ConstantValueExpression, FilterExpression>())

                            from rangeSeparatpr in RangeSeparator

                            from max in DateAndTime.Try().Cast<FilterToken, DateTimeExpression, FilterExpression>()
                                                   .Or(Number.Try().Cast<FilterToken, ConstantValueExpression, FilterExpression>())
                                                   .Or(Asterisk.Try().Cast<FilterToken, AsteriskExpression, FilterExpression>())
                                                   .Or(AlphaNumeric.Cast<FilterToken, ConstantValueExpression, FilterExpression>())

                            from end in Token.EqualTo(FilterToken.OpenSquaredBracket)

                            where min != default || max != default
                            select new RangeExpression(
                                min: new BoundaryExpression(min switch
                                {
                                    ConstantValueExpression constant => constant,
                                    DateTimeExpression dateTime => dateTime,
                                    _ => throw new ArgumentOutOfRangeException($"Unsupported '{min?.GetType()}' for min value")
                                }, included: true),
                                max: new BoundaryExpression(max switch
                                {
                                    AsteriskExpression asterisk => asterisk,
                                    ConstantValueExpression constant => constant,
                                    DateTimeExpression dateTime => dateTime,
                                    _ => throw new ArgumentOutOfRangeException($"Unsupported '{max?.GetType()}' for max value")
                                }, included: false)
                           )
                        ).Try()
                      // Case  ] min TO max [ : lower and upper bounds excluded from the range
                      .Or(
                            // from prop in Property
                            // from eq in Token.EqualTo(FilterToken.Equal)
                            // from _ in AnyExpression.Many()
                            from start in Token.EqualTo(FilterToken.CloseSquaredBracket)
                            from min in DateAndTime.Try().Cast<FilterToken, DateTimeExpression, FilterExpression>()
                                                   .Or(Number.Try().Cast<FilterToken, ConstantValueExpression, FilterExpression>())
                                                   .Or(Asterisk.Try().Cast<FilterToken, AsteriskExpression, FilterExpression>())
                                                   .Or(AlphaNumeric.Try().Cast<FilterToken, ConstantValueExpression, FilterExpression>())

                            from _ in RangeSeparator

                            from max in DateAndTime.Try().Cast<FilterToken, DateTimeExpression, FilterExpression>()
                                                   .Or(Number.Try().Cast<FilterToken, ConstantValueExpression, FilterExpression>())
                                                   .Or(Asterisk.Cast<FilterToken, AsteriskExpression, FilterExpression>())
                                                   .Or(AlphaNumeric.Cast<FilterToken, ConstantValueExpression, FilterExpression>())

                            from end in Token.EqualTo(FilterToken.OpenSquaredBracket)

                            where min != default || max != default
                            select new RangeExpression(
                                min: new BoundaryExpression(min switch
                                {
                                    AsteriskExpression asterisk => asterisk,
                                    ConstantValueExpression constant => constant,
                                    DateTimeExpression dateTime => dateTime,
                                    _ => throw new ArgumentOutOfRangeException($"Unsupported '{min?.GetType()}' for min value")
                                }, included: false),
                                max: new BoundaryExpression(max switch
                                {
                                    AsteriskExpression asterisk => asterisk,
                                    ConstantValueExpression constant => constant,
                                    DateTimeExpression dateTime => dateTime,
                                    _ => throw new ArgumentOutOfRangeException($"Unsupported '{max?.GetType()}' for max value")
                                }, included: false)
                           )
                        );
            }
        }

        private static TokenListParser<FilterToken, Token<FilterToken>> RangeSeparator => Token.EqualToValueIgnoreCase(FilterToken.Alpha, "to")
                                                                                                  .Named("Range separator")
                                                                                                  .Between(Whitespace, Whitespace);

        private static TokenListParser<FilterToken, Token<FilterToken>[]> Whitespace => Token.EqualTo(FilterToken.Whitespace)
            .AtLeastOnce();

        /// <summary>
        /// Group expression
        /// </summary>
        public static TokenListParser<FilterToken, GroupExpression> Group => from lbracket in Token.EqualTo(FilterToken.OpenParenthese)
                                                                             from expression in AnyExpression
                                                                             from rbracket in Token.EqualTo(FilterToken.CloseParenthese)
                                                                             select new GroupExpression(expression);

        private static TokenListParser<FilterToken, FilterExpression> AnyExpression => And.Try().Cast<FilterToken, AndExpression, FilterExpression>()
                                                                                    .Or(Or.Try().Cast<FilterToken, OrExpression, FilterExpression>())
                                                                                    .Or(OneOf.Try().Cast<FilterToken, OneOfExpression, FilterExpression>())
                                                                                    .Or(Expression);

        /// <summary>
        /// Property name parser
        /// </summary>
        public static TokenListParser<FilterToken, PropertyNameExpression> Property => from prop in Token.EqualTo(FilterToken.Alpha)
                                                                                           //    .Select(a => a.ToStringValue())
                                                                                       from subProps in (
                                                                                           from _ in Token.EqualTo(FilterToken.OpenSquaredBracket)
                                                                                           from subProp in AlphaNumeric.Between(Token.EqualTo(FilterToken.DoubleQuote), Token.EqualTo(FilterToken.DoubleQuote))
                                                                                           from __ in Token.EqualTo(FilterToken.CloseSquaredBracket)
                                                                                           select @$"[""{subProp.Value}""]"
                                                                                       ).Many()
                                                                                           //    from _ in Token.EqualTo(FilterToken.Equal).Try()
                                                                                           //    from remaining in AnyExpression
                                                                                       select new PropertyNameExpression(string.Concat(prop.ToStringValue(), string.Concat(subProps)));

        public static TokenListParser<FilterToken, OneOfExpression> OneOf => (from before in EndsWith.Try().Cast<FilterToken, EndsWithExpression, FilterExpression>()
                                                                                .Or(StartsWith.Try().Cast<FilterToken, StartsWithExpression, FilterExpression>())
                                                                                .Or(AlphaNumeric.Cast<FilterToken, ConstantValueExpression, FilterExpression>())
                                                                                .OptionalOrDefault()
                                                                              from regex in Regex
                                                                              from after in EndsWith.Try().Cast<FilterToken, EndsWithExpression, FilterExpression>()
                                                                                .Or(StartsWith.Try().Cast<FilterToken, StartsWithExpression, FilterExpression>())
                                                                                .Or(AlphaNumeric.Cast<FilterToken, ConstantValueExpression, FilterExpression>())
                                                                                .OptionalOrDefault()
                                                                              select new { before, regex, after })
            .Select(item =>
            {
                List<FilterExpression> expressions = new();
                if (item.before is null && item.after is null)
                {
                    expressions.AddRange(item.regex.Value.Select(chr => new ConstantValueExpression(chr.ToString())).ToArray());
                }
                else if (item.after is null)
                {
                    switch (item.before)
                    {
                        case ConstantValueExpression constant: // Syntax like ma[Nn]
                            {
                                expressions.AddRange(item.regex.Value.Select(chr => new ConstantValueExpression($"{constant.Value}{chr}"))
                                    .ToArray());
                                break;
                            }
                        case EndsWithExpression endsWith: // Syntax like *ma[Nn]
                            {
                                expressions.AddRange(item.regex.Value.Select(chr => new EndsWithExpression($"{endsWith.Value}{chr}"))
                                    .ToArray());
                                break;
                            }
                        case StartsWithExpression startsWith: // Syntax like ma*[Nn]
                            {
                                expressions.AddRange(item.regex.Value.Select(chr => new AndExpression(new StartsWithExpression(startsWith.Value), new EndsWithExpression(chr.ToString())))
                                    .ToArray());
                                break;
                            }
                        default:
                            throw new ArgumentOutOfRangeException($"Unsupported {nameof(item.before)} expression type when {nameof(item.after)} expression is null");
                    }
                }
                else if (item.before is null)
                {
                    switch (item.after)
                    {
                        case ConstantValueExpression constant: // Syntaxt like [Mm]an
                            {
                                expressions.AddRange(item.regex.Value.Select(chr => new ConstantValueExpression($"{chr}{constant.Value}"))
                                    .ToArray());
                            }
                            break;
                        case EndsWithExpression endsWith: // Syntax like [Mm]*an
                            {
                                expressions.AddRange(item.regex.Value.Select(chr => new AndExpression(new StartsWithExpression(chr.ToString()), new EndsWithExpression(endsWith.Value)))
                                    .ToArray());
                            }
                            break;
                        case StartsWithExpression startsWith: // Syntax like [Mm]an*
                            {
                                expressions.AddRange(item.regex.Value.Select(chr => new StartsWithExpression($"{chr}{startsWith.Value}"))
                                    .ToArray());
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException($"Unsupported {nameof(item.after)} expression type when before expression is null");
                    }
                }
                else
                {
                    switch (item.before)
                    {
                        case ConstantValueExpression constantBefore:
                            {
                                switch (item.after)
                                {
                                    case ConstantValueExpression constantAfter: // Syntax like Bat[Mm]an
                                        expressions.AddRange(item.regex.Value.Select(chr => new ConstantValueExpression($"{constantBefore.Value}{chr}{constantAfter.Value}"))
                                            .ToArray());
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException($"Unsupported type {item.after.GetType()} for {nameof(item.after)} " +
                                            $"when {nameof(item.before)} and {item.after} are not null expression type when before expression is null");
                                }
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException($"Unsupported {nameof(item.before)} expression type when before expression is null");
                    }
                }

                return new OneOfExpression(expressions.ToArray());
            });

        public static TokenListParser<FilterToken, DateTimeExpression> DateAndTime => (from date in Date
                                                                                       from separator in Token.EqualToValue(FilterToken.Alpha, "T")
                                                                                                              .Or(Token.EqualTo(FilterToken.Whitespace))
                                                                                       from time in Time
                                                                                       from offset in Offset.OptionalOrDefault()
                                                                                       select new DateTimeExpression(date,
                                                                                                                     new(hours: time.Hours,
                                                                                                                         minutes: time.Minutes,
                                                                                                                         seconds: time.Seconds,
                                                                                                                         offset: offset))).Try()
                                                                                      // DATE AND TIME
                                                                                      .Or(
                                                                                          from date in Date
                                                                                          from separator in Token.EqualToValue(FilterToken.Alpha, "T")
                                                                                                                 .Or(Token.EqualTo(FilterToken.Whitespace))
                                                                                          from time in Time
                                                                                          select new DateTimeExpression(date, time)).Try()
                                                                                      // DATE
                                                                                      .Or(from date in Date
                                                                                          select new DateTimeExpression(date)
                                                                                        ).Try()
                                                                                    // TIME
                                                                                    .Or(from time in Time
                                                                                        select new DateTimeExpression(time: time)
            );

        public static TokenListParser<FilterToken, TimeOffset> Offset => (from _ in Token.EqualToValue(FilterToken.Alpha, "Z")
                                                                          select new TimeOffset(0, 0)).Try()
                                                                          .Or(
                                                                                from sign in Token.EqualTo(FilterToken.Dash)
                                                                                                  .Or(Token.EqualToValue(FilterToken.None, "+"))
                                                                                                  .Select(n => n.ToStringValue())
                                                                                from hour in Token.EqualTo(FilterToken.Numeric)
                                                                                                .Select(n => int.Parse(n.ToStringValue()))
                                                                                from _ in Colon
                                                                                from minutes in Token.EqualTo(FilterToken.Numeric)
                                                                                    .Select(n => int.Parse(n.ToStringValue()))
                                                                                select new TimeOffset(hours: sign switch
                                                                                {
                                                                                    "-" => -hour,
                                                                                    _ => hour
                                                                                }, minutes)
                                                                          );
        public static TokenListParser<FilterToken, DateExpression> Date => from year in Token.EqualTo(FilterToken.Numeric)
                                                                                .Apply(Numerics.Integer)
                                                                                .Select(n => int.Parse(n.ToStringValue()))
                                                                           from _ in Dash
                                                                           from month in Token.EqualTo(FilterToken.Numeric)
                                                                                .Apply(Numerics.Integer)
                                                                                .Select(n => int.Parse(n.ToStringValue()))
                                                                           from __ in Dash
                                                                           from day in Token.EqualTo(FilterToken.Numeric)
                                                                                .Apply(Numerics.Integer)
                                                                                .Select(n => int.Parse(n.ToStringValue()))
                                                                           select new DateExpression(year, month, day);

        private static TokenListParser<FilterToken, Token<FilterToken>> Colon => Token.EqualTo(FilterToken.Colon);
        private static TokenListParser<FilterToken, Token<FilterToken>> Dash => Token.EqualTo(FilterToken.Dash);

        public static TokenListParser<FilterToken, TimeExpression> Time => from hour in Token.EqualTo(FilterToken.Numeric)
                                                                                .Select(n => int.Parse(n.ToStringValue()))
                                                                           from _ in Colon
                                                                           from minutes in Token.EqualTo(FilterToken.Numeric)
                                                                               .Select(n => int.Parse(n.ToStringValue()))
                                                                           from __ in Colon
                                                                           from seconds in Token.EqualTo(FilterToken.Numeric)
                                                                               .Select(n => int.Parse(n.ToStringValue()))
                                                                           select new TimeExpression(hour, minutes, seconds);

        /// <summary>
        /// Parser for full criteria
        /// </summary>
        public static TokenListParser<FilterToken, FilterExpression> Expression => Parse.Ref(() => Group.Try().Cast<FilterToken, GroupExpression, FilterExpression>())
                                                                                        .Or(Parse.Ref(() => Range.Try().Cast<FilterToken, RangeExpression, FilterExpression>()))
                                                                                        .Or(Parse.Ref(() => StartsWith.Try().Cast<FilterToken, StartsWithExpression, FilterExpression>()))
                                                                                        .Or(Parse.Ref(() => Contains.Try().Cast<FilterToken, ContainsExpression, FilterExpression>()))
                                                                                        .Or(Parse.Ref(() => EndsWith.Try().Cast<FilterToken, EndsWithExpression, FilterExpression>()))
                                                                                        .Or(Parse.Ref(() => Not.Try().Cast<FilterToken, NotExpression, FilterExpression>()))
                                                                                        .Or(Parse.Ref(() => OneOf.Try().Cast<FilterToken, OneOfExpression, FilterExpression>()))
                                                                                        .Or(Parse.Ref(() => AlphaNumeric.Cast<FilterToken, ConstantValueExpression, FilterExpression>()))
            ;

        public static TokenListParser<FilterToken, (PropertyNameExpression, FilterExpression)> Criterion => from property in Property
                                                                                                            from _ in Token.EqualTo(FilterToken.Equal)
                                                                                                            from expression in AnyExpression
                                                                                                            select (new PropertyNameExpression(property.Name), expression);

        public static TokenListParser<FilterToken, ConstantValueExpression> Number => from n in Token.EqualTo(FilterToken.Numeric)
                                                                                                     .Apply(Numerics.Decimal)
                                                                                      select new ConstantValueExpression(n.ToStringValue());

        public static TokenListParser<FilterToken, (PropertyNameExpression, FilterExpression)[]> Criteria => from criteria in Criterion.ManyDelimitedBy(Token.EqualToValue(FilterToken.None, "&"))
                                                                                                             select criteria;

        private static TokenListParser<FilterToken, ConstantValueExpression> Punctuation =>
        (
            from c in Token.EqualTo(FilterToken.Dot)
            select new ConstantValueExpression(c.ToStringValue())
        ).Or(
                from c in Token.EqualTo(FilterToken.Colon)
                select new ConstantValueExpression(c.ToStringValue())
        ).Or(
            from c in Token.EqualTo(FilterToken.Dash)
            select new ConstantValueExpression(c.ToStringValue())
        ).Or(
            from c in Token.EqualTo(FilterToken.Underscore)
            select new ConstantValueExpression(c.ToStringValue())
        );
    }
}
