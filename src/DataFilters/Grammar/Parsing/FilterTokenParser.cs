namespace DataFilters.Grammar.Parsing
{
    using DataFilters.Grammar.Syntax;

    using Superpower;
    using Superpower.Model;
    using Superpower.Parsers;

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Parses a tree of <see cref="FilterToken"/> into <see cref="FilterExpression"/>
    /// </summary>
    public static class FilterTokenParser
    {
        /// <summary>
        /// Parser for many <see cref="FilterToken.Letter"/>
        /// </summary>
        public static TokenListParser<FilterToken, ConstantValueExpression> AlphaNumeric => (from data in (
                                                                                            from numberBefore in Token.EqualTo(FilterToken.Digit).Optional()
                                                                                            from value in Alpha.Optional()
                                                                                            from numberAfter in Token.EqualTo(FilterToken.Digit).Optional()
                                                                                            where numberBefore is not null || value is not null || numberAfter is not null
                                                                                            select new { numberBefore, value, numberAfter }
                                                                                            ).AtLeastOnce()
                                                                                            select new ConstantValueExpression(
                                                                                                string.Concat(
                                                                                                    data.Select(d => string.Concat(
                                                                                                        d.numberBefore?.ToStringValue() ?? string.Empty,
                                                                                                        d.value?.ToStringValue() ?? string.Empty,
                                                                                                        d.numberAfter?.ToStringValue() ?? string.Empty)))
                                                                                            )
                                                                                            ).Try();

        private static TokenListParser<FilterToken, Token<FilterToken>> Alpha => Token.EqualTo(FilterToken.Letter).Try()
                                                                                      .Or(Token.EqualTo(FilterToken.Escaped)).Try()
                                                                                      .Or(Token.EqualTo(FilterToken.Underscore)).Try()
                                                                                      .Or(Token.EqualTo(FilterToken.None));

        private static TokenListParser<FilterToken, Token<FilterToken>> Digit => Token.EqualTo(FilterToken.Digit);

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
                                                                                                   string.Concat(puncBefore.Select(x => x.Value.Value)),
                                                                                                   string.Concat(alpha.Select(item => item.Value.Value)),
                                                                                                   string.Concat(puncAfter.Select(x => x.Value.Value)))
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
                                                                                               string.Concat(puncBefore.Select(x => x.Value.Value)),
                                                                                               string.Concat(alpha.Select(item => item.Value.Value)),
                                                                                               string.Concat(puncAfter.Select(x => x.Value.Value)))
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
                                                                                           string.Concat(puncBefore.Select(x => x.Value.Value)),
                                                                                           string.Concat(alpha.Select(item => item.Value.Value)),
                                                                                           string.Concat(puncAfter.Select(x => x.Value.Value)))
                                                                                   }).AtLeastOnce()
                                                                                   from escaped in Token.EqualTo(FilterToken.Backslash).Many()
                                                                                   from __ in Asterisk
                                                                                   select new ContainsExpression(string.Concat(data.Select(x => x.value)));

        /// <summary>
        /// Parser for logical OR expression.
        /// </summary>
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
                select new AndExpression(new StartsWithExpression(left.Value.Value.ToString()), new EndsWithExpression(right.Value.Value.ToString()))),
            (_, left, right) => new AndExpression(left, right))
            ;

        /// <summary>
        /// Parser for NOT expression
        /// </summary>
        public static TokenListParser<FilterToken, NotExpression> Not => from _ in Token.EqualTo(FilterToken.Not)
                                                                         from expression in Expression
                                                                         select new NotExpression(expression);

        private static TokenListParser<FilterToken, BracketExpression> Bracket => (
                                                                                    from _ in Token.EqualTo(FilterToken.OpenSquaredBracket)
                                                                                    from rangeValues in BuildRangeBracketValuesParser(Alpha).Or(BuildRangeBracketValuesParser(Digit)).AtLeastOnce()
                                                                                    from __ in Token.EqualTo(FilterToken.CloseSquaredBracket)
                                                                                    select new BracketExpression(rangeValues)
                                                                                ).Try()
                                                                                .Or
                                                                                (
                                                                                    from _ in Token.EqualTo(FilterToken.OpenSquaredBracket)
                                                                                    from alpha in Alpha.Try().Or(Digit).AtLeastOnce()
                                                                                    from __ in Token.EqualTo(FilterToken.CloseSquaredBracket)
                                                                                    select new BracketExpression(new ConstantBracketValue(alpha.Select(chr => chr.ToStringValue())
                                                                                                                                                     .Aggregate((total, current) => $"{total}{current}"))
                                                                                                                )
                                                                                );
        /// <summary>
        /// Builds a parser that can extract bracket expression values
        /// </summary>
        /// <param name="token">The parser that can parse values inside a <c>[</c> qnd <c>]</c></param>
        /// <returns></returns>
        private static TokenListParser<FilterToken, BracketValue> BuildRangeBracketValuesParser(TokenListParser<FilterToken, Token<FilterToken>> token)
            => (from rangeStart in token
                from _ in Token.EqualTo(FilterToken.Dash)
                from rangeEnd in token
                select (BracketValue)new RangeBracketValue(rangeStart.ToStringValue()[0], rangeEnd.ToStringValue()[0]))
            .Try()
            .Or(from values in token.AtLeastOnce()
                select (BracketValue)new ConstantBracketValue(string.Concat(values.Select(value => value.ToStringValue()))))


            ;
        /// <summary>
        /// Parses Range expressions
        /// </summary>
        public static TokenListParser<FilterToken, IntervalExpression> Interval
        {
            get
            {
                return  // Case [ min TO max ] 
                        (
                            from _ in Token.EqualTo(FilterToken.OpenSquaredBracket)
                            from min in DateAndTime.Try()
                                                    .Cast<FilterToken, DateTimeExpression, FilterExpression>()
                                                    .Or(Number.Try().Cast<FilterToken, ConstantValueExpression, FilterExpression>())
                                                    .Or(AlphaNumeric.Cast<FilterToken, ConstantValueExpression, FilterExpression>())

                            from __ in RangeSeparator

                            from max in DateAndTime.Try()
                                                    .Cast<FilterToken, DateTimeExpression, FilterExpression>()
                                                    .Or(Number.Try().Cast<FilterToken, ConstantValueExpression, FilterExpression>())
                                                    .Or(AlphaNumeric.Cast<FilterToken, ConstantValueExpression, FilterExpression>())

                            from ___ in Token.EqualTo(FilterToken.CloseSquaredBracket)

                            where min != default || max != default
                            select new IntervalExpression(
                                min: new BoundaryExpression(min switch
                                {
                                    ConstantValueExpression constant => constant,
                                    DateTimeExpression dateTime => dateTime,
                                    _ => throw new NotSupportedException($"Unsupported '{min?.GetType()}' for min value")
                                }, included: true),
                                max: new BoundaryExpression(max switch
                                {
                                    ConstantValueExpression constant => constant,
                                    DateTimeExpression dateTime => dateTime,
                                    _ => throw new NotSupportedException($"Unsupported '{max?.GetType()}' for max value")
                                }, included: true)
                            )
                        ).Try()
                      // Case ] min TO max ] : lower bound excluded from the interval
                      .Or(
                            from _ in Token.EqualTo(FilterToken.CloseSquaredBracket)
                            from min in DateAndTime.Try().Cast<FilterToken, DateTimeExpression, FilterExpression>()
                                                   .Or(Number.Try().Cast<FilterToken, ConstantValueExpression, FilterExpression>())
                                                   .Or(Asterisk.Try().Cast<FilterToken, AsteriskExpression, FilterExpression>())
                                                   .Or(AlphaNumeric.Try().Cast<FilterToken, ConstantValueExpression, FilterExpression>())

                            from __ in RangeSeparator

                            from max in DateAndTime.Try().Cast<FilterToken, DateTimeExpression, FilterExpression>()
                                                   .Or(Number.Try().Cast<FilterToken, ConstantValueExpression, FilterExpression>())
                                                   .Or(AlphaNumeric.Cast<FilterToken, ConstantValueExpression, FilterExpression>())

                            from ___ in Token.EqualTo(FilterToken.CloseSquaredBracket)

                            where min != default || max != default
                            select new IntervalExpression(
                                min: new BoundaryExpression(min switch
                                {
                                    AsteriskExpression asterisk => asterisk,
                                    ConstantValueExpression constant => constant,
                                    DateTimeExpression dateTime => dateTime,
                                    _ => throw new NotSupportedException($"Unsupported '{min?.GetType()}' for min value")
                                }, included: false),
                                max: new BoundaryExpression(max switch
                                {
                                    ConstantValueExpression constant => constant,
                                    DateTimeExpression dateTime => dateTime,
                                    _ => throw new NotSupportedException($"Unsupported '{max?.GetType()}' for max value")
                                }, included: true)
                           )
                        ).Try()
                      // Case syntax [ min TO max [ : upper bound excluded from the interval
                      .Or(
                            from _ in Token.EqualTo(FilterToken.OpenSquaredBracket)
                            from min in DateAndTime.Try().Cast<FilterToken, DateTimeExpression, FilterExpression>()
                                                   .Or(Number.Try().Cast<FilterToken, ConstantValueExpression, FilterExpression>())
                                                   .Or(AlphaNumeric.Try().Cast<FilterToken, ConstantValueExpression, FilterExpression>())
                            from __ in RangeSeparator
                            from max in DateAndTime.Try().Cast<FilterToken, DateTimeExpression, FilterExpression>()
                                                   .Or(Number.Try().Cast<FilterToken, ConstantValueExpression, FilterExpression>())
                                                   .Or(Asterisk.Try().Cast<FilterToken, AsteriskExpression, FilterExpression>())
                                                   .Or(AlphaNumeric.Cast<FilterToken, ConstantValueExpression, FilterExpression>())

                            from ___ in Token.EqualTo(FilterToken.OpenSquaredBracket)

                            where min != default || max != default
                            select new IntervalExpression(
                                min: new BoundaryExpression(min switch
                                {
                                    ConstantValueExpression constant => constant,
                                    DateTimeExpression dateTime => dateTime,
                                    _ => throw new NotSupportedException($"Unsupported '{min?.GetType()}' for min value")
                                }, included: true),
                                max: new BoundaryExpression(max switch
                                {
                                    AsteriskExpression asterisk => asterisk,
                                    ConstantValueExpression constant => constant,
                                    DateTimeExpression dateTime => dateTime,
                                    _ => throw new NotSupportedException($"Unsupported '{max?.GetType()}' for max value")
                                }, included: false)
                           )
                        ).Try()
                      // Case  ] min TO max [ : lower and upper bounds excluded from the interval
                      .Or(
                            from _ in Token.EqualTo(FilterToken.CloseSquaredBracket)
                            from min in DateAndTime.Try().Cast<FilterToken, DateTimeExpression, FilterExpression>()
                                                   .Or(Number.Try().Cast<FilterToken, ConstantValueExpression, FilterExpression>())
                                                   .Or(Asterisk.Try().Cast<FilterToken, AsteriskExpression, FilterExpression>())
                                                   .Or(AlphaNumeric.Try().Cast<FilterToken, ConstantValueExpression, FilterExpression>())

                            from __ in RangeSeparator
                            from max in DateAndTime.Try().Cast<FilterToken, DateTimeExpression, FilterExpression>()
                                                   .Or(Number.Try().Cast<FilterToken, ConstantValueExpression, FilterExpression>())
                                                   .Or(Asterisk.Try().Cast<FilterToken, AsteriskExpression, FilterExpression>())
                                                   .Or(AlphaNumeric.Cast<FilterToken, ConstantValueExpression, FilterExpression>())
                            from ___ in Token.EqualTo(FilterToken.OpenSquaredBracket)

                            where min != default || max != default
                            select new IntervalExpression(
                                min: new BoundaryExpression(min switch
                                {
                                    AsteriskExpression asterisk => asterisk,
                                    ConstantValueExpression constant => constant,
                                    DateTimeExpression dateTime => dateTime,
                                    _ => throw new NotSupportedException($"Unsupported '{min?.GetType()}' for min value")
                                }, included: false),
                                max: new BoundaryExpression(max switch
                                {
                                    AsteriskExpression asterisk => asterisk,
                                    ConstantValueExpression constant => constant,
                                    DateTimeExpression dateTime => dateTime,
                                    _ => throw new NotSupportedException($"Unsupported '{max?.GetType()}' for max value")
                                }, included: false)
                           )
                        );
            }
        }

        private static TokenListParser<FilterToken, Token<FilterToken>> RangeSeparator => from _ in Whitespace.AtLeastOnce()
                                                                                          from rangeSeparator in Token.EqualToValue(FilterToken.Letter, "T")
                                                                                                .Then(_ => Token.EqualToValue(FilterToken.Letter, "O"))
                                                                                          from __ in Whitespace.AtLeastOnce()
                                                                                          select rangeSeparator;

        private static TokenListParser<FilterToken, Token<FilterToken>[]> Whitespace => Token.EqualTo(FilterToken.Whitespace)
            .AtLeastOnce();

        /// <summary>
        /// Group expression
        /// </summary>
        public static TokenListParser<FilterToken, GroupExpression> Group => from _ in Token.EqualTo(FilterToken.OpenParenthese)
                                                                             from expression in AnyExpression
                                                                             from __ in Token.EqualTo(FilterToken.CloseParenthese)
                                                                             select new GroupExpression(expression);

        private static TokenListParser<FilterToken, FilterExpression> AnyExpression => And.Try().Cast<FilterToken, AndExpression, FilterExpression>()
                                                                                    .Or(Or.Try().Cast<FilterToken, OrExpression, FilterExpression>())
                                                                                    .Or(OneOf.Try().Cast<FilterToken, OneOfExpression, FilterExpression>())
                                                                                    .Or(Expression);

        /// <summary>
        /// Property name parser
        /// </summary>
        public static TokenListParser<FilterToken, PropertyName> Property => from prop in AlphaNumeric
                                                                             from subProps in (
                                                                                 from _ in Token.EqualTo(FilterToken.OpenSquaredBracket)
                                                                                 from subProp in AlphaNumeric.Between(Token.EqualTo(FilterToken.DoubleQuote), Token.EqualTo(FilterToken.DoubleQuote))
                                                                                 from __ in Token.EqualTo(FilterToken.CloseSquaredBracket)
                                                                                 select @$"[""{subProp.Value.Value}""]"
                                                                             ).Many()
                                                                             select new PropertyName(string.Concat(prop.Value.Value.ToString(), string.Concat(subProps)));

        private static IEnumerable<char> ConvertRegexToCharArray(IEnumerable<BracketValue> values)
            => values.Select(value =>
                    value switch
                    {
                        RangeBracketValue rangeValue => Enumerable.Range(rangeValue.Start, rangeValue.End - rangeValue.Start + 1)
                                                                    .Select(ascii => (char)ascii),
                        ConstantBracketValue constantValue => constantValue.Value.ToCharArray(),
                        _ => throw new NotSupportedException("Unexpected regex value")
                    })
                .SelectMany(x => x);

        /// <summary>
        /// Parser for expressions that contains one or more regex parts.
        /// </summary>
        public static TokenListParser<FilterToken, OneOfExpression> OneOf => (
                                                                                from head in Bracket
                                                                                from body in Asterisk.Try().Cast<FilterToken, AsteriskExpression, FilterExpression>()
                                                                                                        .Or(AlphaNumeric.Cast<FilterToken, ConstantValueExpression, FilterExpression>())
                                                                                                        .OptionalOrDefault()
                                                                                from tail in Bracket

                                                                                select (head: (FilterExpression)head, body, tail: (FilterExpression)tail)
                                                                             ).Try()
                                                                             .Or(
                                                                                 from head in EndsWith.Try().Cast<FilterToken, EndsWithExpression, FilterExpression>()
                                                                                                         .Or(StartsWith.Try().Cast<FilterToken, StartsWithExpression, FilterExpression>())
                                                                                                         .Or(AlphaNumeric.Cast<FilterToken, ConstantValueExpression, FilterExpression>())
                                                                                                         .Or(Asterisk.Cast<FilterToken, AsteriskExpression, FilterExpression>())
                                                                                                         .OptionalOrDefault()
                                                                                 from body in Bracket
                                                                                 from tail in EndsWith.Try().Cast<FilterToken, EndsWithExpression, FilterExpression>()
                                                                                                      .Or(StartsWith.Try().Cast<FilterToken, StartsWithExpression, FilterExpression>())
                                                                                                      .Or(AlphaNumeric.Try().Cast<FilterToken, ConstantValueExpression, FilterExpression>())
                                                                                                      .Or(Asterisk.Try().Cast<FilterToken, AsteriskExpression, FilterExpression>())
                                                                                                      .OptionalOrDefault()

                                                                                 select (head, body: (FilterExpression)body, tail)
                                                                               )
            .Select<FilterToken, (FilterExpression, FilterExpression, FilterExpression), OneOfExpression>(item =>
            {
                return item switch
                {
                    // *<regex>
                    (AsteriskExpression _, BracketExpression bracket, null) => new(ConvertRegexToCharArray(bracket.Values).Select(chr => new EndsWithExpression(chr.ToString()))
                                                                                                                     .ToArray()),

                    // *<regex><constant>
                    (AsteriskExpression _, BracketExpression bracket, ConstantValueExpression tail) => new(ConvertRegexToCharArray(bracket.Values).Select(chr => new EndsWithExpression($"{chr}{tail.Value.Value}"))
                                                                                                                                             .ToArray()),

                    // <regex>*
                    (null, BracketExpression bracket, AsteriskExpression _) => new(ConvertRegexToCharArray(bracket.Values).Select(chr => new StartsWithExpression(chr.ToString()))
                                                                                                                     .ToArray()),

                    // <regex><startwith>*
                    (null, BracketExpression bracket, StartsWithExpression body) => new(ConvertRegexToCharArray(bracket.Values).Select(chr => new StartsWithExpression($"{chr}{body.Value}"))
                                                                     .ToArray()),
                    // <constant><regex><constant>
                    (ConstantValueExpression bracket, BracketExpression regex, ConstantValueExpression tail) => new(ConvertRegexToCharArray(regex.Values).Select(chr => new ConstantValueExpression($"{bracket.Value.Value}{chr}{tail.Value.Value}"))
                                                                     .ToArray()),
                    // <constant><regex>
                    (ConstantValueExpression head, BracketExpression bracket, null) => new(ConvertRegexToCharArray(bracket.Values).Select(chr => new ConstantValueExpression($"{head.Value.Value}{chr}")).ToArray()),

                    // <regex><constant>
                    (null, BracketExpression bracket, ConstantValueExpression tail) => new(ConvertRegexToCharArray(bracket.Values).Select(chr => new ConstantValueExpression($"{chr}{tail.Value.Value}")).ToArray()),

                    // <regex>
                    (null, BracketExpression bracket, null) => new(ConvertRegexToCharArray(bracket.Values).Select(chr => new ConstantValueExpression(chr.ToString())).ToArray()),

                    // <regex><constant><regex>
                    (BracketExpression head, ConstantValueExpression body, BracketExpression tail) => new(ConvertRegexToCharArray(head.Values).CrossJoin(ConvertRegexToCharArray(tail.Values))
                                                                            .Select(tuple => new { start = tuple.Item1, end = tuple.Item2 })
                                                                            .Select(tuple => new ConstantValueExpression($"{tuple.start}{body.Value.Value}{tuple.end}"))
                                                                            .ToArray()),

                    // <endswith><regex>
                    (EndsWithExpression head, BracketExpression body, null) => new(ConvertRegexToCharArray(body.Values).Select(chr => new EndsWithExpression($"{head.Value}{chr}"))
                                                                                                                      .ToArray()),
                    // <startswith><regex>
                    (StartsWithExpression head, BracketExpression body, null) => new(ConvertRegexToCharArray(body.Values).Select(chr => (FilterExpression)new AndExpression(head,
                                                                                                                                                                           new EndsWithExpression(chr.ToString())))
                                                                                                                  .ToArray()),
                    // <regex><endswith>
                    (null, BracketExpression body, EndsWithExpression tail) => new(ConvertRegexToCharArray(body.Values).Select(chr => (FilterExpression)new AndExpression(new StartsWithExpression(chr.ToString()), tail))
                                                                                                                  .ToArray()),
                    _ => throw new NotSupportedException($"Unsupported {nameof(OneOf)} expression :  {item}")
                };
            });

        /// <summary>
        /// Parser for Date and Time
        /// </summary>
        public static TokenListParser<FilterToken, DateTimeExpression> DateAndTime => (from date in Date
                                                                                       from separator in Token.EqualToValue(FilterToken.Letter, "T")
                                                                                                              .Or(Token.EqualTo(FilterToken.Whitespace))
                                                                                       from time in Time
                                                                                       from offset in Offset.OptionalOrDefault()
                                                                                       select new DateTimeExpression(date,
                                                                                                                     new(hours: time.Hours,
                                                                                                                         minutes: time.Minutes,
                                                                                                                         seconds: time.Seconds,
                                                                                                                         milliseconds: time.Milliseconds,
                                                                                                                         offset: offset))).Try()
                                                                                      // DATE AND TIME
                                                                                      .Or(
                                                                                          from date in Date
                                                                                          from separator in Token.EqualToValue(FilterToken.Letter, "T")
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

        private static TokenListParser<FilterToken, TimeOffset> Offset => (from _ in Token.EqualToValue(FilterToken.Letter, "Z")
                                                                           select TimeOffset.Zero).Try()
                                                                          .Or(
                                                                                from sign in MinusSign.Or(PlusSign)
                                                                                                  .Select(n => n.ToStringValue())
                                                                                from hour in Token.EqualTo(FilterToken.Digit)
                                                                                                .Select(n => int.Parse(n.ToStringValue()))
                                                                                from _ in Colon
                                                                                from minutes in Token.EqualTo(FilterToken.Digit)
                                                                                    .Select(n => int.Parse(n.ToStringValue()))
                                                                                select new TimeOffset(hours: sign switch
                                                                                {
                                                                                    "-" => -hour,
                                                                                    _ => hour
                                                                                }, minutes));

        /// <summary>
        /// Parser for "date" expressions.
        /// </summary>
        public static TokenListParser<FilterToken, DateExpression> Date => from year in Token.EqualTo(FilterToken.Digit)
                                                                                .Apply(Numerics.Integer)
                                                                                .Select(n => int.Parse(n.ToStringValue()))
                                                                           from _ in Dash
                                                                           from month in Token.EqualTo(FilterToken.Digit)
                                                                                .Apply(Numerics.Integer)
                                                                                .Select(n => int.Parse(n.ToStringValue()))
                                                                           from __ in Dash
                                                                           from day in Token.EqualTo(FilterToken.Digit)
                                                                                .Apply(Numerics.Integer)
                                                                                .Select(n => int.Parse(n.ToStringValue()))
                                                                           select new DateExpression(year, month, day);

        private static TokenListParser<FilterToken, Token<FilterToken>> Colon => Token.EqualTo(FilterToken.Colon);

        private static TokenListParser<FilterToken, Token<FilterToken>> Dash => Token.EqualTo(FilterToken.Dash);

        private static TokenListParser<FilterToken, Token<FilterToken>> MinusSign => Dash;

        private static TokenListParser<FilterToken, Token<FilterToken>> PlusSign => Token.EqualToValue(FilterToken.None, "+");

        private static TokenListParser<FilterToken, string> MinusOrPlusSign => MinusSign.Try().Or(PlusSign.Try()).Optional()
                                                                                              .Select(token => token?.ToStringValue() switch
                                                                                              {
                                                                                                  "-" => "-",
                                                                                                  _ => string.Empty
                                                                                              });


        /// <summary>
        /// Parser for time expression.
        /// </summary>
        public static TokenListParser<FilterToken, TimeExpression> Time => (from hour in Token.EqualTo(FilterToken.Digit)
                                                                                             .Apply(Numerics.Integer)
                                                                                .Select(n => int.Parse(n.ToStringValue()))
                                                                            from _ in Colon
                                                                            from minutes in Token.EqualTo(FilterToken.Digit)
                                                                                .Select(n => int.Parse(n.ToStringValue()))
                                                                            from __ in Colon
                                                                            from seconds in Token.EqualTo(FilterToken.Digit)
                                                                                .Select(n => int.Parse(n.ToStringValue()))
                                                                            from ___ in Token.EqualTo(FilterToken.Dot)
                                                                            from milliseconds in Token.EqualTo(FilterToken.Digit)
                                                                                .Select(n => int.Parse(n.ToStringValue()))
                                                                            select new TimeExpression(hour, minutes, seconds, milliseconds))
                                                                           .Try()
                                                                           .Or(from hour in Token.EqualTo(FilterToken.Digit)
                                                                                             .Apply(Numerics.Integer)
                                                                                .Select(n => int.Parse(n.ToStringValue()))
                                                                               from _ in Colon
                                                                               from minutes in Token.EqualTo(FilterToken.Digit)
                                                                                   .Select(n => int.Parse(n.ToStringValue()))
                                                                               from __ in Colon
                                                                               from seconds in Token.EqualTo(FilterToken.Digit)
                                                                                   .Select(n => int.Parse(n.ToStringValue()))
                                                                               select new TimeExpression(hour, minutes, seconds));

        /// <summary>
        /// Parser for full criteria
        /// </summary>
        public static TokenListParser<FilterToken, FilterExpression> Expression => Parse.Ref(() => Group.Try().Cast<FilterToken, GroupExpression, FilterExpression>())
                                                                                        .Or(Parse.Ref(() => Interval.Try().Cast<FilterToken, IntervalExpression, FilterExpression>()))
                                                                                        .Or(Parse.Ref(() => StartsWith.Try().Cast<FilterToken, StartsWithExpression, FilterExpression>()))
                                                                                        .Or(Parse.Ref(() => Contains.Try().Cast<FilterToken, ContainsExpression, FilterExpression>()))
                                                                                        .Or(Parse.Ref(() => EndsWith.Try().Cast<FilterToken, EndsWithExpression, FilterExpression>()))
                                                                                        .Or(Parse.Ref(() => Not.Try().Cast<FilterToken, NotExpression, FilterExpression>()))
                                                                                        .Or(Parse.Ref(() => OneOf.Try().Cast<FilterToken, OneOfExpression, FilterExpression>()))
                                                                                        .Or(Parse.Ref(() => AlphaNumeric.Cast<FilterToken, ConstantValueExpression, FilterExpression>()))
            ;

        /// <summary>
        /// Parser for <c>property=value</c> pair.
        /// </summary>
        public static TokenListParser<FilterToken, (PropertyName, FilterExpression)> Criterion => from property in Property
                                                                                                  from _ in Token.EqualTo(FilterToken.Equal)
                                                                                                  from expression in AnyExpression
                                                                                                  select (new PropertyName(property.Name), expression);

        /// <summary>
        /// Parser for numeric expressions
        /// </summary>
        public static TokenListParser<FilterToken, ConstantValueExpression> Number => FloatOrDouble.Try().Or(IntegerOrLong);

        private static TokenListParser<FilterToken, ConstantValueExpression> FloatOrDouble
            => (from sign in MinusOrPlusSign.Try()
                from digitBeforeDot in Token.EqualTo(FilterToken.Digit).Apply(Numerics.NaturalUInt64)
                from _ in Token.EqualTo(FilterToken.Dot)
                from digitAfterDot in Token.EqualTo(FilterToken.Digit)
                from __ in Token.EqualToValueIgnoreCase(FilterToken.Letter, "d")
                select new ConstantValueExpression(double.Parse($"{sign}{digitBeforeDot}.{digitAfterDot.ToStringValue()}", CultureInfo.InvariantCulture)))
                .Try()
                .Or(
                     from sign in MinusOrPlusSign.Try()
                     from digitBeforeDot in Token.EqualTo(FilterToken.Digit).Apply(Numerics.NaturalUInt64)
                     from _ in Token.EqualTo(FilterToken.Dot)
                     from digitAfterDot in Token.EqualTo(FilterToken.Digit)
                     from __ in Token.EqualToValueIgnoreCase(FilterToken.Letter, "f")
                     select new ConstantValueExpression(float.Parse($"{sign}{digitBeforeDot}.{digitAfterDot.ToStringValue()}", CultureInfo.InvariantCulture)))
                .Try()
                .Or(
                    from sign in MinusOrPlusSign.Try()
                    from digitBeforeDot in Token.EqualTo(FilterToken.Digit).Apply(Numerics.NaturalUInt64)
                    from _ in Token.EqualTo(FilterToken.Dot)
                    from digitAfterDot in Token.EqualTo(FilterToken.Digit)
                    select new ConstantValueExpression(double.Parse($"{sign}{digitBeforeDot}.{digitAfterDot.ToStringValue()}", CultureInfo.InvariantCulture))
                )
                .Try()
                .Or(
                    from sign in MinusOrPlusSign.Try()
                    from value in Token.EqualTo(FilterToken.Digit).Apply(Numerics.NaturalUInt64)
                    from __ in Token.EqualToValueIgnoreCase(FilterToken.Letter, "d")
                    select new ConstantValueExpression(double.Parse($"{sign}{value}", CultureInfo.InvariantCulture))
                )


            ;

        private static TokenListParser<FilterToken, ConstantValueExpression> IntegerOrLong
            => (from sign in MinusOrPlusSign.Try()
                from number in Token.EqualTo(FilterToken.Digit).Apply(Numerics.IntegerInt64)
                from hint in Token.EqualToValueIgnoreCase(FilterToken.Letter, "L")
                select new ConstantValueExpression(long.Parse($"{sign}{number}", CultureInfo.InvariantCulture)))
                .Try()
                .Or(
                    from sign in MinusOrPlusSign.Try()
                    from number in Token.EqualTo(FilterToken.Digit).Apply(Numerics.IntegerInt32)
                    select new ConstantValueExpression(int.Parse($"{sign}{number}", CultureInfo.InvariantCulture))
                )
            ;


        /// <summary>
        /// Parser for many <see cref="Criterion"/> separated by <c>&amp;</c>.
        /// </summary>
        public static TokenListParser<FilterToken, (PropertyName, FilterExpression)[]> Criteria => from criteria in Criterion.ManyDelimitedBy(Token.EqualToValue(FilterToken.None, "&"))
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

        /// <summary>
        /// Parser for duration
        /// </summary>
        public static TokenListParser<FilterToken, DurationExpression> Duration => from p in Token.EqualToValue(FilterToken.Letter, "P")

                                                                                   from years in DurationPart("Y").OptionalOrDefault().Try()
                                                                                   from months in DurationPart("M").OptionalOrDefault().Try()
                                                                                   from weeks in DurationPart("W").OptionalOrDefault().Try()
                                                                                   from days in DurationPart("D").OptionalOrDefault().Try()

                                                                                   from timeSeparator in Token.EqualToValue(FilterToken.Letter, "T")

                                                                                   from hours in DurationPart("H").OptionalOrDefault().Try()
                                                                                   from minutes in DurationPart("M").OptionalOrDefault().Try()
                                                                                   from seconds in DurationPart("S").OptionalOrDefault().Try()

                                                                                   where years is not null
                                                                                         || months is not null
                                                                                         || weeks is not null
                                                                                         || days is not null
                                                                                         || hours is not null
                                                                                         || minutes is not null
                                                                                         || seconds is not null

                                                                                   select new DurationExpression(years: years is not null ? int.Parse(years.Value.Value.ToString()) : 0,
                                                                                                                 months: months is not null ? int.Parse(months.Value.Value.ToString()) : 0,
                                                                                                                 weeks: weeks is not null ? int.Parse(weeks.Value.Value.ToString()) : 0,
                                                                                                                 days: days is not null ? int.Parse(days.Value.Value.ToString()) : 0,
                                                                                                                 hours: hours is not null ? int.Parse(hours.Value.Value.ToString()) : 0,
                                                                                                                 minutes: minutes is not null ? int.Parse(minutes.Value.Value.ToString()) : 0,
                                                                                                                 seconds: seconds is not null ? int.Parse(seconds.Value.Value.ToString()) : 0);

        private static TokenListParser<FilterToken, ConstantValueExpression> DurationPart(string designator) => from n in Number
                                                                                                                from _ in Token.EqualToValue(FilterToken.Letter, designator).Try()
                                                                                                                select n;
    }
}
