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
        public static TokenListParser<FilterToken, ConstantValueExpression> AlphaNumeric => from data in (
                                                                                               from symbolBefore in Token.EqualTo(FilterToken.Escaped).Try()
                                                                                                                         .Or(Token.EqualTo(FilterToken.None).Try())
                                                                                                                         .Or(Token.EqualTo(FilterToken.Dot).Try()).Many()
                                                                                               from digitsBefore in Digit.Many()
                                                                                               from alpha in Alpha.Many()
                                                                                               from digitsAfter in Digit.Many()
                                                                                               from symbolAfter in Token.EqualTo(FilterToken.Escaped).Try()
                                                                                                                        .Or(Token.EqualTo(FilterToken.None).Try())
                                                                                                                        .Or(Token.EqualTo(FilterToken.Dot).Try())
                                                                                                                        .Many()
                                                                                               where symbolBefore.AtLeastOnce()
                                                                                                     || digitsBefore.AtLeastOnce()
                                                                                                     || alpha.AtLeastOnce()
                                                                                                     || digitsAfter.AtLeastOnce()
                                                                                                     || symbolAfter.AtLeastOnce()

                                                                                               let value = string.Concat(string.Concat(symbolBefore.Select(x => x.ToStringValue())),
                                                                                                                         string.Concat(digitsBefore.Select(x => x.ToStringValue())),
                                                                                                                         string.Concat(alpha.Select(item => item.ToStringValue())),
                                                                                                                         string.Concat(digitsAfter.Select(x => x.ToStringValue())),
                                                                                                                         string.Concat(symbolAfter.Select(x => x.ToStringValue())))
                                                                                               select value).AtLeastOnce()

                                                                                            let alphaNumericValue = string.Concat(data)
                                                                                            let textSpan = new TextSpan(alphaNumericValue)
                                                                                            select Numerics.Decimal.IsMatch(textSpan) || Numerics.Integer.IsMatch(textSpan)
                                                                                                  ? new NumericValueExpression(alphaNumericValue)
                                                                                                  : (ConstantValueExpression)new StringValueExpression(alphaNumericValue);

        private static TokenListParser<FilterToken, Token<FilterToken>> Alpha => Token.EqualTo(FilterToken.Letter).Try()
                                                                                      .Or(Token.EqualTo(FilterToken.Escaped)).Try()
                                                                                      .Or(Token.EqualTo(FilterToken.Underscore)).Try()
                                                                                      .Or(Token.EqualTo(FilterToken.None));

        private static TokenListParser<FilterToken, Token<FilterToken>> Digit => Token.EqualTo(FilterToken.Digit);

        private static TokenListParser<FilterToken, StringValueExpression> Bool => Token.EqualToValueIgnoreCase(FilterToken.Letter, "t")
                                                                                         .IgnoreThen(Token.EqualToValueIgnoreCase(FilterToken.Letter, "r"))
                                                                                         .IgnoreThen(Token.EqualToValueIgnoreCase(FilterToken.Letter, "u"))
                                                                                         .IgnoreThen(Token.EqualToValueIgnoreCase(FilterToken.Letter, "e"))
                                                                                         .Select(_ => new StringValueExpression(bool.TrueString)).Try()
                                                                   .Or(
                                                                        Token.EqualToValueIgnoreCase(FilterToken.Letter, "f")
                                                                             .IgnoreThen(Token.EqualToValueIgnoreCase(FilterToken.Letter, "a"))
                                                                             .IgnoreThen(Token.EqualToValueIgnoreCase(FilterToken.Letter, "l"))
                                                                             .IgnoreThen(Token.EqualToValueIgnoreCase(FilterToken.Letter, "s"))
                                                                             .IgnoreThen(Token.EqualToValueIgnoreCase(FilterToken.Letter, "e"))
                                                                             .Select(_ => new StringValueExpression(bool.FalseString)));

        /// <summary>
        /// Parser for '*' character
        /// </summary>
        private static TokenListParser<FilterToken, AsteriskExpression> Asterisk => from __ in Token.EqualTo(FilterToken.Asterisk)
                                                                                    select new AsteriskExpression();

        /// <summary>
        /// Parser for "starts with" expressions
        /// </summary>
        public static TokenListParser<FilterToken, StartsWithExpression> StartsWith => (from data in AlphaNumeric.AtLeastOnce()
                                                                                        from _ in Asterisk
                                                                                        select new StartsWithExpression(string.Concat(data.Select(x => x.Value)))).Try()
                                                                                        .Or(
                                                                                            from text in Text
                                                                                            from _ in Asterisk
                                                                                            select new StartsWithExpression(text)
                                                                                        );

        /// <summary>
        /// Parser for "starts with" expressions
        /// </summary>
        public static TokenListParser<FilterToken, EndsWithExpression> EndsWith => Asterisk.IgnoreThen(AlphaNumeric.AtLeastOnce())
                                                                                           .Select(data => new EndsWithExpression(string.Concat(data.Select(item => item.Value)))).Try()
                                                                                   .Or(Asterisk.IgnoreThen(Text)
                                                                                       .Select(text => new EndsWithExpression(text)));

        /// <summary>
        /// Parser for "contains" expression
        /// </summary>
        public static TokenListParser<FilterToken, ContainsExpression> Contains => (from _ in Asterisk
                                                                                   from data in (
                                                                                       from symbolBefore in Token.EqualTo(FilterToken.Escaped).Try().Or(Whitespace).Many()
                                                                                       from puncBefore in Punctuation.Many()
                                                                                       from alpha in AlphaNumeric.Many()
                                                                                       from puncAfter in Punctuation.Many()
                                                                                       from symbolAfter in Token.EqualTo(FilterToken.Escaped).Try().Or(Whitespace).Many()
                                                                                       where symbolBefore.AtLeastOnce()
                                                                                             || puncBefore.AtLeastOnce()
                                                                                             || alpha.AtLeastOnce()
                                                                                             || puncAfter.AtLeastOnce()
                                                                                             || symbolAfter.AtLeastOnce()
                                                                                       select new
                                                                                       {
                                                                                           value = string.Concat(string.Concat(symbolBefore.Select(x => x.ToStringValue())),
                                                                                                                 string.Concat(puncBefore.Select(x => x.Value)),
                                                                                                                 string.Concat(alpha.Select(item => item.Value)),
                                                                                                                 string.Concat(puncAfter.Select(x => x.Value)),
                                                                                                                 string.Concat(symbolAfter.Select(x => x.ToStringValue())))
                                                                                       }).AtLeastOnce()
                                                                                   from escaped in Token.EqualTo(FilterToken.Backslash).Many()
                                                                                   from __ in Asterisk
                                                                                   select new ContainsExpression(string.Concat(data.Select(x => x.value)))).Try()
                                                                                   .Or(Text.Between(Asterisk, Asterisk)
                                                                                            .Select(text => new ContainsExpression(text)));

        /// <summary>
        /// Parser for logical OR expression.
        /// </summary>
        public static TokenListParser<FilterToken, OrExpression> Or => from left in UnaryExpression
                                                                       from _ in Token.EqualTo(FilterToken.Or)
                                                                       from right in UnaryExpression
                                                                       select new OrExpression(left, right);

        /// <summary>
        /// Parser for logical AND expression
        /// </summary>
        public static TokenListParser<FilterToken, AndExpression> And => (from left in UnaryExpression
                                                                          from _ in Token.EqualTo(FilterToken.And)
                                                                          from right in UnaryExpression
                                                                          select new AndExpression(left, right)).Try()
                                                                        .Or(from left in AlphaNumeric
                                                                            from _ in Asterisk
                                                                            from right in AlphaNumeric
                                                                            select new AndExpression(new StartsWithExpression(left.Value),
                                                                                                    new EndsWithExpression(right.Value)));

        /// <summary>
        /// Parser for NOT expression
        /// </summary>
        public static TokenListParser<FilterToken, NotExpression> Not => from _ in Token.EqualTo(FilterToken.Bang)
                                                                         from expression in Parse.Ref(() => UnaryExpression)
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
                            from min in Constant
                            from __ in RangeSeparator
                            from max in Constant
                            from ___ in Token.EqualTo(FilterToken.CloseSquaredBracket)

                            where min != default || max != default
                            select new IntervalExpression(
                                min: new BoundaryExpression(min switch
                                {
                                    NumericValueExpression constant => constant,
                                    DateTimeExpression dateTime => dateTime,
                                    _ => throw new NotSupportedException($"Unsupported '{min?.GetType()}' for min value")
                                }, included: true),
                                max: new BoundaryExpression(max switch
                                {
                                    NumericValueExpression constant => constant,
                                    DateTimeExpression dateTime => dateTime,
                                    _ => throw new NotSupportedException($"Unsupported '{max?.GetType()}' for max value")
                                }, included: true)
                            )
                        ).Try()
                      // Case ] min TO max ] : lower bound excluded from the interval
                      .Or(
                            from _ in Token.EqualTo(FilterToken.CloseSquaredBracket)
                            from min in Asterisk.Try().Cast<FilterToken, AsteriskExpression, FilterExpression>()
                                                .Or(Constant)
                            from __ in RangeSeparator
                            from max in Constant
                            from ___ in Token.EqualTo(FilterToken.CloseSquaredBracket)

                            where min != default || max != default
                            select new IntervalExpression(
                                min: new BoundaryExpression(min switch
                                {
                                    AsteriskExpression asterisk => asterisk,
                                    NumericValueExpression constant => constant,
                                    DateTimeExpression dateTime => dateTime,
                                    _ => throw new NotSupportedException($"Unsupported '{min?.GetType()}' for min value")
                                }, included: false),
                                max: new BoundaryExpression(max switch
                                {
                                    NumericValueExpression constant => constant,
                                    DateTimeExpression dateTime => dateTime,
                                    _ => throw new NotSupportedException($"Unsupported '{max?.GetType()}' for max value")
                                }, included: true)
                           )
                        ).Try()
                      // Case syntax [ min TO max [ : upper bound excluded from the interval
                      .Or(
                            from _ in Token.EqualTo(FilterToken.OpenSquaredBracket)
                            from min in Constant
                            from __ in RangeSeparator
                            from max in Asterisk.Try().Cast<FilterToken, AsteriskExpression, FilterExpression>()
                                                .Or(Constant)

                            from ___ in Token.EqualTo(FilterToken.OpenSquaredBracket)

                            where min != default || max != default
                            select new IntervalExpression(
                                min: new BoundaryExpression(min switch
                                {
                                    NumericValueExpression constant => constant,
                                    DateTimeExpression dateTime => dateTime,
                                    _ => throw new NotSupportedException($"Unsupported '{min?.GetType()}' for min value")
                                }, included: true),
                                max: new BoundaryExpression(max switch
                                {
                                    AsteriskExpression asterisk => asterisk,
                                    NumericValueExpression constant => constant,
                                    DateTimeExpression dateTime => dateTime,
                                    _ => throw new NotSupportedException($"Unsupported '{max?.GetType()}' for max value")
                                }, included: false)
                           )
                        ).Try()
                      // Case  ] min TO max [ : lower and upper bounds excluded from the interval
                      .Or(
                            from _ in Token.EqualTo(FilterToken.CloseSquaredBracket)
                            from min in Asterisk.Try().Cast<FilterToken, AsteriskExpression, FilterExpression>()
                                                .Or(Constant)

                            from __ in RangeSeparator
                            from max in Asterisk.Try().Cast<FilterToken, AsteriskExpression, FilterExpression>()
                                                .Or(Constant)
                            from ___ in Token.EqualTo(FilterToken.OpenSquaredBracket)

                            where min != default || max != default
                            select new IntervalExpression(
                                min: new BoundaryExpression(min switch
                                {
                                    AsteriskExpression asterisk => asterisk,
                                    NumericValueExpression constant => constant,
                                    DateTimeExpression dateTime => dateTime,
                                    _ => throw new NotSupportedException($"Unsupported '{min?.GetType()}' for min value")
                                }, included: false),
                                max: new BoundaryExpression(max switch
                                {
                                    AsteriskExpression asterisk => asterisk,
                                    NumericValueExpression constant => constant,
                                    DateTimeExpression dateTime => dateTime,
                                    _ => throw new NotSupportedException($"Unsupported '{max?.GetType()}' for max value")
                                }, included: false)
                           )
                        );
            }
        }

        private static TokenListParser<FilterToken, FilterExpression> Constant => GlobalUniqueIdentifier.Try().Cast<FilterToken, GuidValueExpression, FilterExpression>()
                                                                                                          .Or(DateAndTime.Try().Cast<FilterToken, DateTimeExpression, FilterExpression>())
                                                                                                          .Or(Time.Try().Cast<FilterToken, TimeExpression, FilterExpression>())
                                                                                                          .Or(Date.Try().Cast<FilterToken, DateExpression, FilterExpression>())
                                                                                                          .Or(Duration.Try().Cast<FilterToken, DurationExpression, FilterExpression>())
                                                                                                          .Or(Number.Try().Cast<FilterToken, NumericValueExpression, FilterExpression>())
                                                                                                          .Or(Bool.Try().Cast<FilterToken, StringValueExpression, FilterExpression>())
                                                                                                          .Or(Text.Cast<FilterToken, TextExpression, FilterExpression>())
                                                                                                          .Or(AlphaNumeric.Cast<FilterToken, ConstantValueExpression, FilterExpression>())
            ;

        private static TokenListParser<FilterToken, Token<FilterToken>> RangeSeparator => from _ in Whitespace.AtLeastOnce()
                                                                                          from rangeSeparator in Token.EqualToValue(FilterToken.Letter, "T")
                                                                                                .Then(_ => Token.EqualToValue(FilterToken.Letter, "O"))
                                                                                          from __ in Whitespace.AtLeastOnce()
                                                                                          select rangeSeparator;

        private static TokenListParser<FilterToken, Token<FilterToken>> Whitespace => Token.EqualTo(FilterToken.Whitespace);

        /// <summary>
        /// Group expression
        /// </summary>
        public static TokenListParser<FilterToken, GroupExpression> Group => from _ in Token.EqualTo(FilterToken.OpenParenthese)
                                                                             from expression in BinaryOrUnaryExpression
                                                                             from __ in Token.EqualTo(FilterToken.CloseParenthese)
                                                                             select new GroupExpression(expression);

        private static TokenListParser<FilterToken, FilterExpression> BinaryOrUnaryExpression => Parse.Ref(() => Not.Try().Cast<FilterToken, NotExpression, FilterExpression>())
                                                                                                      .Or(Parse.Ref(() => And.Try().Cast<FilterToken, AndExpression, FilterExpression>()))
                                                                                                      .Or(Parse.Ref(() => Or.Try().Cast<FilterToken, OrExpression, FilterExpression>()))
                                                                                                      .Or(Parse.Ref(() => OneOf.Try().Cast<FilterToken, OneOfExpression, FilterExpression>()))
                                                                                                      .Or(Parse.Ref(() => UnaryExpression));

        /// <summary>
        /// Property name parser
        /// </summary>
        public static TokenListParser<FilterToken, PropertyName> Property => from prop in AlphaNumeric
                                                                             from subProps in (
                                                                                 from _ in Token.EqualTo(FilterToken.OpenSquaredBracket)
                                                                                 from subProp in AlphaNumeric.Between(Token.EqualTo(FilterToken.DoubleQuote), Token.EqualTo(FilterToken.DoubleQuote))
                                                                                 from __ in Token.EqualTo(FilterToken.CloseSquaredBracket)
                                                                                 select @$"[""{subProp.Value}""]"
                                                                             ).Many()
                                                                             select new PropertyName(string.Concat(prop.Value, string.Concat(subProps)));

        /// <summary>
        /// Parser for any text between double quotes <c>"</c>
        /// </summary>
        public static TokenListParser<FilterToken, TextExpression> Text => from _ in Token.EqualTo(FilterToken.DoubleQuote)
#if NETSTANDARD1_3
                                                                           from text in (Token.EqualTo(FilterToken.Letter)
                                                                                             .Or(Token.EqualTo(FilterToken.Digit))
                                                                                             .Or(Token.EqualTo(FilterToken.Escaped))
                                                                                             .Or(Token.EqualTo(FilterToken.None))).AtLeastOnce()
#else
                                                                           from text in Token.Matching<FilterToken>(token => token != FilterToken.DoubleQuote, "Any character or symbol except double quote character")
                                                                                                                                                                .AtLeastOnce()
#endif
                                                                           from __ in Token.EqualTo(FilterToken.DoubleQuote)
                                                                           select new TextExpression(TokensToString(text));

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
            .Select(item =>
            {
                return item switch
                {
                    // *<regex>
                    (AsteriskExpression _, BracketExpression bracket, null) => new OneOfExpression(ConvertRegexToCharArray(bracket.Values).Select(chr => new EndsWithExpression(chr.ToString()))
                                                                                                                     .ToArray()),

                    // *<regex><constant>
                    (AsteriskExpression _, BracketExpression bracket, ConstantValueExpression tail) => new OneOfExpression(ConvertRegexToCharArray(bracket.Values).Select(chr => new EndsWithExpression($"{chr}{tail.Value}"))
                                                                                                                                             .ToArray()),

                    // <regex>*
                    (null, BracketExpression bracket, AsteriskExpression _) => new OneOfExpression(ConvertRegexToCharArray(bracket.Values).Select(chr => new StartsWithExpression(chr.ToString()))
                                                                                                                     .ToArray()),

                    // <endswith><regex>
                    (EndsWithExpression head, BracketExpression body, null) => new OneOfExpression(ConvertRegexToCharArray(body.Values).Select(chr => new EndsWithExpression($"{head.Value}{chr}"))
                                                                                                                      .ToArray()),
                    // <startswith><regex>
                    (StartsWithExpression head, BracketExpression body, null) => new OneOfExpression(ConvertRegexToCharArray(body.Values).Select(chr => (FilterExpression)new AndExpression(head,
                                                                                                                                                                           new EndsWithExpression(chr.ToString())))
                                                                                                                  .ToArray()),
                    // <regex><startwith>*
                    (null, BracketExpression bracket, StartsWithExpression body) => new OneOfExpression(ConvertRegexToCharArray(bracket.Values).Select(chr => new StartsWithExpression($"{chr}{body.Value}")).ToArray()),
                    // <constant><regex><constant>
                    (ConstantValueExpression bracket, BracketExpression regex, ConstantValueExpression tail) => new OneOfExpression(ConvertRegexToCharArray(regex.Values).Select(chr => new StringValueExpression($"{bracket.Value}{chr}{tail.Value}")).ToArray()),
                    // <constant><regex>
                    (ConstantValueExpression head, BracketExpression bracket, null) => new OneOfExpression(ConvertRegexToCharArray(bracket.Values).Select(chr => new StringValueExpression($"{head.Value}{chr}")).ToArray()),

                    // <regex><constant>
                    (null, BracketExpression bracket, ConstantValueExpression tail) => new OneOfExpression(ConvertRegexToCharArray(bracket.Values).Select(chr => new StringValueExpression($"{chr}{tail.Value}")).ToArray()),

                    // <regex>
                    (null, BracketExpression bracket, null) => new OneOfExpression(ConvertRegexToCharArray(bracket.Values).Select(chr => new StringValueExpression(chr.ToString())).ToArray()),

                    // <regex><constant><regex>
                    (BracketExpression head, ConstantValueExpression body, BracketExpression tail) => new(ConvertRegexToCharArray(head.Values).CrossJoin(ConvertRegexToCharArray(tail.Values))
                                                                            .Select(tuple => new { start = tuple.Item1, end = tuple.Item2 })
                                                                            .Select(tuple => new StringValueExpression($"{tuple.start}{body.Value}{tuple.end}"))
                                                                            .ToArray()),

                    // <regex><endswith>
                    (null, BracketExpression body, EndsWithExpression tail) => new OneOfExpression(ConvertRegexToCharArray(body.Values).Select(chr => (FilterExpression)new AndExpression(new StartsWithExpression(chr.ToString()), tail)).ToArray()),
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
                                                                                                                         milliseconds: time.Milliseconds),
                                                                                                                     offset)).Try()
                                                                                      // DATE
                                                                                      .Or(from date in Date
                                                                                          select new DateTimeExpression(date)
                                                                                        ).Try()
                                                                                    // TIME
                                                                                    .Or(from time in Time
                                                                                        select new DateTimeExpression(time)
            );

        private static TokenListParser<FilterToken, OffsetExpression> Offset => (from _ in Token.EqualToValue(FilterToken.Letter, "Z")
                                                                                 select OffsetExpression.Zero).Try()
                                                                          .Or(
                                                                                from sign in MinusOrPlusSign.OptionalOrDefault(NumericSign.Plus)
                                                                                from hourDigits in IntDigits(2)
                                                                                from _ in Colon
                                                                                from minuteDigits in IntDigits(2)
                                                                                select new OffsetExpression(sign,
                                                                                                            uint.Parse(TokensToString(hourDigits), CultureInfo.InvariantCulture),
                                                                                                            uint.Parse(TokensToString(minuteDigits), CultureInfo.InvariantCulture)));

        private static TokenListParser<FilterToken, Token<FilterToken>[]> IntDigits(int count) => Token.EqualTo(FilterToken.Digit).Repeat(count);

        /// <summary>
        /// Parser for "date" expressions.
        /// </summary>
        public static TokenListParser<FilterToken, DateExpression> Date => from year in IntDigits(4)
                                                                           from _ in Dash
                                                                           from month in IntDigits(2)
                                                                           from __ in Dash
                                                                           from day in IntDigits(2)
                                                                           select new DateExpression(int.Parse(TokensToString(year), CultureInfo.InvariantCulture),
                                                                                                     int.Parse(TokensToString(month), CultureInfo.InvariantCulture),
                                                                                                     int.Parse(TokensToString(day), CultureInfo.InvariantCulture));

        private static TokenListParser<FilterToken, Token<FilterToken>> Colon => Token.EqualTo(FilterToken.Colon);

        private static TokenListParser<FilterToken, Token<FilterToken>> Dash => Token.EqualTo(FilterToken.Dash);

        private static TokenListParser<FilterToken, Token<FilterToken>> MinusSign => Dash;

        private static TokenListParser<FilterToken, Token<FilterToken>> PlusSign => Token.EqualToValue(FilterToken.None, "+");

        private static TokenListParser<FilterToken, NumericSign> MinusOrPlusSign => MinusSign.Or(PlusSign).Optional()
                                                                                              .Select(token => token?.ToStringValue() switch
                                                                                              {
                                                                                                  "-" => NumericSign.Minus,
                                                                                                  _ => NumericSign.Plus
                                                                                              });

        /// <summary>
        /// Parser for time expression.
        /// </summary>
        public static TokenListParser<FilterToken, TimeExpression> Time => (from hourDigits in IntDigits(2)
                                                                            from _ in Colon
                                                                            from minuteDigits in IntDigits(2)
                                                                            from __ in Colon
                                                                            from secondDigits in IntDigits(2)
                                                                            from ___ in Token.EqualTo(FilterToken.Dot)
                                                                            from milliseconds in Token.EqualTo(FilterToken.Digit).AtLeastOnce()
                                                                            select new TimeExpression(int.Parse(TokensToString(hourDigits), CultureInfo.InvariantCulture),
                                                                                                      int.Parse(TokensToString(minuteDigits), CultureInfo.InvariantCulture),
                                                                                                      int.Parse(TokensToString(secondDigits), CultureInfo.InvariantCulture),
                                                                                                      int.Parse(TokensToString(milliseconds), CultureInfo.InvariantCulture)))
                                                                           .Try()
                                                                           .Or(from hourDigits in IntDigits(2)
                                                                               from _ in Colon
                                                                               from minuteDigits in IntDigits(2)
                                                                               from __ in Colon
                                                                               from secondDigits in IntDigits(2)
                                                                               select new TimeExpression(int.Parse(TokensToString(hourDigits), CultureInfo.InvariantCulture),
                                                                                                         int.Parse(TokensToString(minuteDigits), CultureInfo.InvariantCulture),
                                                                                                         int.Parse(TokensToString(secondDigits), CultureInfo.InvariantCulture)));

        /// <summary>
        /// Parses all supported expressions
        /// </summary>
        private static TokenListParser<FilterToken, FilterExpression> UnaryExpression => Parse.Ref(() => Not.Try().Cast<FilterToken, NotExpression, FilterExpression>())
                                                                                              .Or(Parse.Ref(() => Group.Try().Cast<FilterToken, GroupExpression, FilterExpression>()))
                                                                                              .Or(Parse.Ref(() => OneOf.Try().Cast<FilterToken, OneOfExpression, FilterExpression>()))
                                                                                              .Or(Parse.Ref(() => Interval.Try().Cast<FilterToken, IntervalExpression, FilterExpression>()))
                                                                                              .Or(Parse.Ref(() => GlobalUniqueIdentifier.Try().Cast<FilterToken, GuidValueExpression, FilterExpression>()))
                                                                                              .Or(Parse.Ref(() => Duration.Try().Cast<FilterToken, DurationExpression, FilterExpression>()))
                                                                                              .Or(Parse.Ref(() => DateAndTime.Try().Cast<FilterToken, DateTimeExpression, FilterExpression>()))
                                                                                              .Or(Parse.Ref(() => Time.Try().Cast<FilterToken, TimeExpression, FilterExpression>()))
                                                                                              .Or(Parse.Ref(() => Date.Try().Cast<FilterToken, DateExpression, FilterExpression>()))
                                                                                              .Or(Parse.Ref(() => Contains.Try().Cast<FilterToken, ContainsExpression, FilterExpression>()))
                                                                                              .Or(Parse.Ref(() => StartsWith.Try().Cast<FilterToken, StartsWithExpression, FilterExpression>()))
                                                                                              .Or(Parse.Ref(() => EndsWith.Try().Cast<FilterToken, EndsWithExpression, FilterExpression>()))
                                                                                              .Or(Parse.Ref(() => Bool.Try().Cast<FilterToken, StringValueExpression, FilterExpression>()))
                                                                                              .Or(Parse.Ref(() => Text.Try().Cast<FilterToken, TextExpression, FilterExpression>()))
                                                                                              .Or(Parse.Ref(() => AlphaNumeric.Try().Cast<FilterToken, ConstantValueExpression, FilterExpression>()))
                                                                                              .Or(Parse.Ref(() => Number.Cast<FilterToken, NumericValueExpression, FilterExpression>()))
            ;
        /// <summary>
        /// Parser for <c>property=value</c> pair.
        /// </summary>
        public static TokenListParser<FilterToken, (PropertyName, FilterExpression)> Criterion => from property in Property
                                                                                                  from _ in Token.EqualTo(FilterToken.Equal)
                                                                                                  from expression in BinaryOrUnaryExpression
                                                                                                  select (new PropertyName(property.Name), expression);

        /// <summary>
        /// Parser for numeric expressions
        /// </summary>
        public static TokenListParser<FilterToken, NumericValueExpression> Number => FloatOrDouble.Try().Or(IntegerOrLong);

        private static TokenListParser<FilterToken, NumericValueExpression> FloatOrDouble
            => (from sign in MinusOrPlusSign.Optional()
                from digitBeforeDot in Token.EqualTo(FilterToken.Digit).AtLeastOnce()
                from _ in Token.EqualTo(FilterToken.Dot)
                from digitAfterDot in Token.EqualTo(FilterToken.Digit).AtLeastOnce()
                from __ in Token.EqualToValueIgnoreCase(FilterToken.Letter, "d")
                select new NumericValueExpression($"{ConvertSignToChar(sign)}{TokensToString(digitBeforeDot)}.{TokensToString(digitAfterDot)}"))
                .Try()
                .Or(
                     from sign in MinusOrPlusSign.Optional()
                     from digitBeforeDot in Token.EqualTo(FilterToken.Digit).AtLeastOnce()
                     from _ in Token.EqualTo(FilterToken.Dot)
                     from digitAfterDot in Token.EqualTo(FilterToken.Digit).AtLeastOnce()
                     from __ in Token.EqualToValueIgnoreCase(FilterToken.Letter, "f")
                     select new NumericValueExpression($"{ConvertSignToChar(sign)}{TokensToString(digitBeforeDot)}.{TokensToString(digitAfterDot)}"))
                .Try()
                .Or(
                    from sign in MinusOrPlusSign.Optional()
                    from digitBeforeDot in Token.EqualTo(FilterToken.Digit).AtLeastOnce()
                    from _ in Token.EqualTo(FilterToken.Dot)
                    from digitAfterDot in Token.EqualTo(FilterToken.Digit).AtLeastOnce()
                    select new NumericValueExpression($"{ConvertSignToChar(sign)}{TokensToString(digitBeforeDot)}.{TokensToString(digitAfterDot)}")
                )
                .Try()
                .Or(
                    from sign in MinusOrPlusSign.Optional()
                    from value in Token.EqualTo(FilterToken.Digit).AtLeastOnce()
                    from __ in Token.EqualToValueIgnoreCase(FilterToken.Letter, "d")
                    select new NumericValueExpression($"{ConvertSignToChar(sign)}{value}")
                );

        private static TokenListParser<FilterToken, NumericValueExpression> IntegerOrLong
            => (from sign in MinusOrPlusSign.Optional()
                from digits in Token.EqualTo(FilterToken.Digit).AtLeastOnce()
                from hint in Token.EqualToValueIgnoreCase(FilterToken.Letter, "L")
                select new NumericValueExpression($"{ConvertSignToChar(sign)}{TokensToString(digits)}"))
                .Try()
                .Or(
                    from sign in MinusOrPlusSign.Optional()
                    from digits in Token.EqualTo(FilterToken.Digit).AtLeastOnce()
                    select new NumericValueExpression($"{ConvertSignToChar(sign)}{TokensToString(digits)}")
                )
            ;

        private static string ConvertSignToChar(NumericSign? sign) => sign switch
        {
            NumericSign.Minus => "-",
            _ => string.Empty,
        };

        /// <summary>
        /// Parser for many <see cref="Criterion"/> separated by <c>&amp;</c>.
        /// </summary>
        public static TokenListParser<FilterToken, (PropertyName, FilterExpression)[]> Criteria => from criteria in Criterion.ManyDelimitedBy(Token.EqualTo(FilterToken.Ampersand))
                                                                                                   select criteria;

        private static TokenListParser<FilterToken, StringValueExpression> Punctuation =>
        (
            from c in Token.EqualTo(FilterToken.Dot)
            select new StringValueExpression(c.ToStringValue())
        ).Or(
                from c in Token.EqualTo(FilterToken.Colon)
                select new StringValueExpression(c.ToStringValue())
        ).Or(
            from c in Token.EqualTo(FilterToken.Dash)
            select new StringValueExpression(c.ToStringValue())
        ).Or(
            from c in Token.EqualTo(FilterToken.Underscore)
            select new StringValueExpression(c.ToStringValue())
        );

        private static TokenListParser<FilterToken, GuidValueExpression> GlobalUniqueIdentifier => from chr1 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from chr2 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from chr3 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from chr4 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from chr5 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from chr6 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from chr7 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from chr8 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from _ in Token.EqualTo(FilterToken.Dash)
                                                                                                   from chr9 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from chr10 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from chr11 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from chr12 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from __ in Token.EqualTo(FilterToken.Dash)
                                                                                                   from chr13 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from chr14 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from chr15 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from chr16 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from ___ in Token.EqualTo(FilterToken.Dash)
                                                                                                   from chr17 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from chr18 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from chr19 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from chr20 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from ____ in Token.EqualTo(FilterToken.Dash)
                                                                                                   from chr21 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from chr22 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from chr23 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from chr24 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from chr25 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from chr26 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from chr27 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from chr28 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from chr29 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from chr30 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from chr31 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   from chr32 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
                                                                                                   select new GuidValueExpression($"{TokensToString(new[] { chr1, chr2, chr3, chr4, chr5, chr6, chr7, chr8 })}-{TokensToString(new[] { chr9, chr10, chr11, chr12 })}-{TokensToString(new[] { chr13, chr14, chr15, chr16 })}-{TokensToString(new[] { chr17, chr18, chr19, chr20 })}-{TokensToString(new[] { chr21, chr22, chr23, chr24, chr25, chr26, chr27, chr28, chr29, chr30, chr31, chr32 })}");

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

                                                                                   select new DurationExpression(years: years is not null ? int.Parse(years.Value, CultureInfo.InvariantCulture) : 0,
                                                                                                                 months: months is not null ? int.Parse(months.Value, CultureInfo.InvariantCulture) : 0,
                                                                                                                 weeks: weeks is not null ? int.Parse(weeks.Value, CultureInfo.InvariantCulture) : 0,
                                                                                                                 days: days is not null ? int.Parse(days.Value, CultureInfo.InvariantCulture) : 0,
                                                                                                                 hours: hours is not null ? int.Parse(hours.Value, CultureInfo.InvariantCulture) : 0,
                                                                                                                 minutes: minutes is not null ? int.Parse(minutes.Value, CultureInfo.InvariantCulture) : 0,
                                                                                                                 seconds: seconds is not null ? int.Parse(seconds.Value, CultureInfo.InvariantCulture) : 0);

        private static TokenListParser<FilterToken, NumericValueExpression> DurationPart(string designator) => from n in IntegerOrLong
                                                                                                               from _ in Token.EqualToValue(FilterToken.Letter, designator).Try()
                                                                                                               select n;

        private static string TokensToString(IEnumerable<Token<FilterToken>> tokens)
        {
            static string TokenToString(Token<FilterToken> token) => token.ToStringValue();

            return string.Concat(tokens.Select(TokenToString));
        }
    }
}
