using Candoumbe.Types.Strings;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using System;
using System.Collections.Generic;
#if NET7_0_OR_GREATER
using System.Diagnostics;
#endif
using System.Globalization;
using System.Linq;
using DataFilters.Grammar.Syntax;
using Microsoft.Extensions.Primitives;

namespace DataFilters.Grammar.Parsing;

/// <summary>
/// Parses a tree of <see cref="FilterToken"/> into <see cref="FilterExpression"/>
/// </summary>
public static class FilterTokenParser
{
    /// <summary>
    /// Parser for many <see cref="FilterToken.Letter"/>s or <see cref="FilterToken.Digit"/>s.
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
                                                                                                         where symbolBefore.Length > 0
                                                                                                               || digitsBefore.Length > 0
                                                                                                               || alpha.Length > 0
                                                                                                               || digitsAfter.Length > 0
                                                                                                               || symbolAfter.Length > 0
                                                                                                         let value = new StringSegmentLinkedList(StringSegment.Empty, [
                                                                                                             .. symbolBefore.Select(x => x.Span.ToStringSegment()),
                                                                                                             .. digitsBefore.Select(x => x.Span.ToStringSegment()),
                                                                                                             .. alpha.Select(item => item.Span.ToStringSegment()),
                                                                                                             .. digitsAfter.Select(x => x.Span.ToStringSegment()),
                                                                                                             .. symbolAfter.Select(x => x.Span.ToStringSegment())
                                                                                                         ])
                                                                                                         select value).AtLeastOnce()
                                                                                        let alphaNumericValue = data.Aggregate(new StringSegmentLinkedList(), (mainList, list) => mainList.Append(list)).ToStringValue()
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
                                                                                select AsteriskExpression.Instance;

    /// <summary>
    /// Parser for "starts with" expressions
    /// </summary>
    public static TokenListParser<FilterToken, StartsWithExpression> StartsWith => (from text in Text
                                                                                    from _ in Asterisk
                                                                                    select new StartsWithExpression(text)).Try()
        .Or(
            from data in AlphaNumeric.AtLeastOnce()
            from _ in Asterisk
            select new StartsWithExpression(data.Select(x => x.Value)
                                                .Aggregate(new StringSegmentLinkedList(),
                                                           (current, other) => current.Append(other)))
           );

    /// <summary>
    /// Parser for "ends with" expressions
    /// </summary>
    public static TokenListParser<FilterToken, EndsWithExpression> EndsWith => Asterisk.IgnoreThen(Text)
        .Select(text => new EndsWithExpression(text)).Try()
        .Or(Asterisk.IgnoreThen(AlphaNumeric.AtLeastOnce())
                .Select(data => new EndsWithExpression(data.Select(item => item.Value)
                                                           .Aggregate(new StringSegmentLinkedList(), (current, other) => current.Append(other)))));

    /// <summary>
    /// Parser for "contains" expression
    /// </summary>
    public static TokenListParser<FilterToken, ContainsExpression> Contains => Text.Between(Asterisk, Asterisk)
        .Select(text => new ContainsExpression(text)).Try()
        .Or(
            from _ in Asterisk
            from data in (
                             from symbolBefore in Token.EqualTo(FilterToken.Escaped).Try().Or(Whitespace).Many()
                             from punctuationBefore in Punctuation.Many()
                             from alpha in AlphaNumeric.Many()
                             from punctuationAfter in Punctuation.Many()
                             from symbolAfter in Token.EqualTo(FilterToken.Escaped).Try().Or(Whitespace).Many()
                             where symbolBefore.AtLeastOnce()
                                   || punctuationBefore.AtLeastOnce()
                                   || alpha.AtLeastOnce()
                                   || punctuationAfter.AtLeastOnce()
                                   || symbolAfter.AtLeastOnce()
                             let result = new StringSegmentLinkedList()
                             select new
                             {
                                 value = result.Append(symbolBefore.Select(x => x.Span.ToStringSegment())
                                                           .Aggregate(new StringSegmentLinkedList(), (mainList, segment) => mainList.Append(segment)))
                                     .Append(symbolAfter.Select(x => x.Span.ToStringSegment())
                                                 .Aggregate(new StringSegmentLinkedList(), (mainList, segment) => mainList.Append(segment)))
                                     .Append(punctuationBefore.Select(x => x.Value)
                                                 .Aggregate(new StringSegmentLinkedList(), (mainList, segment) => mainList.Append(segment)))
                                     .Append(alpha.Select(item => item.Value)
                                                 .Aggregate(new StringSegmentLinkedList(), (mainList, segment) => mainList.Append(segment)))
                                     .Append(punctuationAfter.Select(x => x.Value)
                                                 .Aggregate(new StringSegmentLinkedList(), (mainList, segment) => mainList.Append(segment)))
                                     .Append(symbolAfter.Select(x => x.Span.ToStringSegment())
                                                 .Aggregate(new StringSegmentLinkedList(), (mainList, segment) => mainList.Append(segment)))
                             }).AtLeastOnce()
            //from escaped in Token.EqualTo(FilterToken.Backslash).Many()
            from __ in Asterisk
            let result = new StringSegmentLinkedList()
            select new ContainsExpression(EscapedString.From(data.Select(x => x.value)
                                                                 .Aggregate(result,
                                                                            (current, other) => current.Append(other))
                                                                 .ToStringValue()))
           );

    /// <summary>
    /// Parser for logical OR expression.
    /// </summary>
    public static TokenListParser<FilterToken, OrExpression> Or
        => from left in Parse.Ref(() => UnaryExpression)
           from _ in Token.EqualTo(FilterToken.Or)
           from right in Parse.Ref(() => UnaryExpression)
           select left | right;

    /// <summary>
    /// Parser for logical AND expression
    /// </summary>
    public static TokenListParser<FilterToken, AndExpression> And => (from left in Parse.Ref(() => UnaryExpression)
                                                                      from _ in Token.EqualTo(FilterToken.And)
                                                                      from right in Parse.Ref(() => UnaryExpression)
                                                                      select new AndExpression(left, right)).Try()
        .Or(from left in AlphaNumeric
            from _ in Asterisk
            from right in AlphaNumeric
            from __ in Asterisk
            select new AndExpression(new StartsWithExpression(left.Value),
                                     new ContainsExpression(right.Value))).Try()
        .Or(from left in AlphaNumeric
            from _ in Asterisk
            from right in AlphaNumeric
            select new AndExpression(new StartsWithExpression(left.Value),
                                     new EndsWithExpression(right.Value)));

    /// <summary>
    /// Parser for NOT expression
    /// </summary>
    public static TokenListParser<FilterToken, NotExpression> Not => (from bangs in Token.EqualTo(FilterToken.Bang).AtLeastOnce()
                                                                      from expression in Parse.Ref(() => UnaryExpression)
                                                                      select (bangs, expression))
        .Select(tuple =>
                {
                    NotExpression notExpression = !tuple.expression;

                    for (int i = 1; i < tuple.bangs.Length; i++)
                    {
                        notExpression = !notExpression;
                    }

                    return notExpression;
                });

    private static TokenListParser<FilterToken, BracketExpression> Bracket => (
                                                                                  from _ in Token.EqualTo(FilterToken.LeftSquaredBracket)
                                                                                  from rangeValues in BuildRangeBracketValuesParser(Alpha).Or(BuildRangeBracketValuesParser(Digit)).AtLeastOnce()
                                                                                  from __ in Token.EqualTo(FilterToken.RightSquaredBracket)
                                                                                  select new BracketExpression(rangeValues)
                                                                              ).Try()
        .Or
            (
             from _ in Token.EqualTo(FilterToken.LeftSquaredBracket)
             from alpha in Alpha.Try().Or(Digit).AtLeastOnce()
             from __ in Token.EqualTo(FilterToken.RightSquaredBracket)
             select new BracketExpression(new ConstantBracketValue(alpha.Select(chr => chr.ToStringValue())
                                                                       .Aggregate((total, current) => $"{total}{current}"))
                                         )
            );

    /// <summary>
    /// Builds a parser that can extract bracket expression values
    /// </summary>
    /// <param name="token">The parser that can parse values inside a <c>[</c> and <c>]</c></param>
    /// <returns></returns>
    private static TokenListParser<FilterToken, BracketValue> BuildRangeBracketValuesParser(TokenListParser<FilterToken, Token<FilterToken>> token)
        => (from rangeStart in token
            from _ in Token.EqualTo(FilterToken.Dash)
            from rangeEnd in token
            select (BracketValue)new RangeBracketValue(rangeStart.ToStringValue()[0], rangeEnd.ToStringValue()[0]))
            .Try()
            .Or(from values in token.AtLeastOnce()
                select (BracketValue)new ConstantBracketValue(string.Concat(values.Select(value => value.ToStringValue()))));

    /// <summary>
    /// Parses Range expressions
    /// </summary>
    public static TokenListParser<FilterToken, IntervalExpression> Interval
    {
        get
        {
            return // Case [ min TO max ]
                (
                    from _ in Token.EqualTo(FilterToken.LeftSquaredBracket)
                    from min in Constant
                    from __ in RangeSeparator
                    from max in Constant
                    from ___ in Token.EqualTo(FilterToken.RightSquaredBracket)
                    where min is not null || max is not null
                    select new IntervalExpression(
                                                  min: new BoundaryExpression(min switch
                                                  {
                                                      NumericValueExpression constant => constant,
                                                      DateTimeExpression dateTime     => dateTime,
#if NET7_0_OR_GREATER
                            _ => throw new UnreachableException($"Unsupported '{min?.GetType()}' for min value")
#else
                                                      _ => throw new NotSupportedException($"Unsupported '{min?.GetType()}' for min value")
#endif
                                                  }, included: true),
                                                  max: new BoundaryExpression(max switch
                                                  {
                                                      NumericValueExpression constant => constant,
                                                      DateTimeExpression dateTime     => dateTime,
#if NET7_0_OR_GREATER
                            _ => throw new UnreachableException($"Unsupported '{max?.GetType()}' for max value")
#else
                                                      _ => throw new NotSupportedException($"Unsupported '{max?.GetType()}' for max value")
#endif
                                                  }, included: true)
                                                 )
                ).Try()
                // Case ] min TO max ] : lower bound excluded from the interval
                .Or(
                    from _ in Token.EqualTo(FilterToken.RightSquaredBracket)
                    from min in Asterisk.Try().Cast<FilterToken, AsteriskExpression, FilterExpression>()
                        .Or(Constant)
                    from __ in RangeSeparator
                    from max in Constant
                    from ___ in Token.EqualTo(FilterToken.RightSquaredBracket)
                    where min is not null || max is not null
                    select new IntervalExpression(
                                                  min: new BoundaryExpression(min switch
                                                  {
                                                      AsteriskExpression asterisk     => asterisk,
                                                      NumericValueExpression constant => constant,
                                                      DateTimeExpression dateTime     => dateTime,
#if NET7_0_OR_GREATER
                            _ => throw new UnreachableException($"Unsupported '{min?.GetType()}' for min value")
#else
                                                      _ => throw new NotSupportedException($"Unsupported '{min?.GetType()}' for min value")
#endif
                                                  }, included: false),
                                                  max: new BoundaryExpression(max switch
                                                  {
                                                      NumericValueExpression constant => constant,
                                                      DateTimeExpression dateTime     => dateTime,
#if NET7_0_OR_GREATER
                            _ => throw new UnreachableException($"Unsupported '{max?.GetType()}' for max value")
#else
                                                      _ => throw new NotSupportedException($"Unsupported '{max?.GetType()}' for max value")
#endif
                                                  }, included: true)
                                                 )
                   ).Try()
                // Case syntax [ min TO max [ : upper bound excluded from the interval
                .Or(
                    from _ in Token.EqualTo(FilterToken.LeftSquaredBracket)
                    from min in Constant
                    from __ in RangeSeparator
                    from max in Asterisk.Try().Cast<FilterToken, AsteriskExpression, FilterExpression>()
                        .Or(Constant)
                    from ___ in Token.EqualTo(FilterToken.LeftSquaredBracket)
                    where min is not null || max is not null
                    select new IntervalExpression(
                                                  min: new BoundaryExpression(min switch
                                                  {
                                                      NumericValueExpression constant => constant,
                                                      DateTimeExpression dateTime     => dateTime,
#if NET7_0_OR_GREATER
                            _ => throw new UnreachableException($"Unsupported '{min?.GetType()}' for min value")
#else
                                                      _ => throw new NotSupportedException($"Unsupported '{min?.GetType()}' for min value")
#endif
                                                  }, included: true),
                                                  max: new BoundaryExpression(max switch
                                                  {
                                                      AsteriskExpression asterisk     => asterisk,
                                                      NumericValueExpression constant => constant,
                                                      DateTimeExpression dateTime     => dateTime,
#if NET7_0_OR_GREATER
                            _ => throw new UnreachableException($"Unsupported '{max?.GetType()}' for max value")
#else
                                                      _ => throw new NotSupportedException($"Unsupported '{max?.GetType()}' for max value")
#endif
                                                  }, included: false)
                                                 )
                   ).Try()
                // Case  ] min TO max [ : lower and upper bounds excluded from the interval
                .Or(
                    from _ in Token.EqualTo(FilterToken.RightSquaredBracket)
                    from min in Asterisk.Try().Cast<FilterToken, AsteriskExpression, FilterExpression>()
                        .Or(Constant)
                    from __ in RangeSeparator
                    from max in Asterisk.Try().Cast<FilterToken, AsteriskExpression, FilterExpression>()
                        .Or(Constant)
                    from ___ in Token.EqualTo(FilterToken.LeftSquaredBracket)
                    where min is not null || max is not null
                    select new IntervalExpression(
                                                  min: new BoundaryExpression(min switch
                                                  {
                                                      AsteriskExpression asterisk     => asterisk,
                                                      NumericValueExpression constant => constant,
                                                      DateTimeExpression dateTime     => dateTime,
#if NET7_0_OR_GREATER
                            _ => throw new UnreachableException($"Unsupported '{min?.GetType()}' for min value")
#else
                                                      _ => throw new NotSupportedException($"Unsupported '{min?.GetType()}' for min value")
#endif
                                                  }, included: false),
                                                  max: new BoundaryExpression(max switch
                                                  {
                                                      AsteriskExpression asterisk     => asterisk,
                                                      NumericValueExpression constant => constant,
                                                      DateTimeExpression dateTime     => dateTime,
#if NET7_0_OR_GREATER
                            _ => throw new UnreachableException($"Unsupported '{max?.GetType()}' for max value")
#else
                                                      _ => throw new NotSupportedException($"Unsupported '{max?.GetType()}' for max value")
#endif
                                                  }, included: false)
                                                 )
                   );
        }
    }

    private static TokenListParser<FilterToken, FilterExpression> Constant => Parse.OneOf(GlobalUniqueIdentifier.Try().Cast<FilterToken, GuidValueExpression, FilterExpression>(),
                                                                                          DateAndTime.Try().Cast<FilterToken, DateTimeExpression, FilterExpression>(),
                                                                                          Time.Try().Cast<FilterToken, TimeExpression, FilterExpression>(),
                                                                                          Date.Try().Cast<FilterToken, DateExpression, FilterExpression>(),
                                                                                          Duration.Try().Cast<FilterToken, DurationExpression, FilterExpression>(),
                                                                                          Number.Try().Cast<FilterToken, NumericValueExpression, FilterExpression>(),
                                                                                          Bool.Try().Cast<FilterToken, StringValueExpression, FilterExpression>(),
                                                                                          Text.Cast<FilterToken, TextExpression, FilterExpression>(),
                                                                                          AlphaNumeric.Cast<FilterToken, ConstantValueExpression, FilterExpression>());

    private static TokenListParser<FilterToken, Token<FilterToken>> RangeSeparator => from _ in Whitespace.AtLeastOnce()
                                                                                      from rangeSeparator in Token.EqualToValue(FilterToken.Letter, "T")
                                                                                          .Then(_ => Token.EqualToValue(FilterToken.Letter, "O"))
                                                                                      from __ in Whitespace.AtLeastOnce()
                                                                                      select rangeSeparator;

    private static TokenListParser<FilterToken, Token<FilterToken>> Whitespace => Token.EqualTo(FilterToken.Whitespace);

    /// <summary>
    /// Group expression
    /// </summary>
    public static TokenListParser<FilterToken, GroupExpression> Group => from _ in Token.EqualTo(FilterToken.LeftParenthesis)
                                                                         from expression in Parse.Ref(() => BinaryOrUnaryExpression)
                                                                         from __ in Token.EqualTo(FilterToken.RightParenthesis)
                                                                         select new GroupExpression(expression);

    private static TokenListParser<FilterToken, FilterExpression> BinaryOrUnaryExpression => Parse.OneOf(
                                                                                                         Parse.Ref(() => And.Try().Cast<FilterToken, AndExpression, FilterExpression>()),
                                                                                                         Parse.Ref(() => Or.Try().Cast<FilterToken, OrExpression, FilterExpression>()),
                                                                                                         Parse.Ref(() => UnaryExpression)
                                                                                                        );

        /// <summary>
        /// Property name parser
        /// </summary>
        public static TokenListParser<FilterToken, PropertyName> Property => from prop in AlphaNumeric
                                                                             from subProps in (
                                                                                 from _ in Token.EqualTo(FilterToken.LeftSquaredBracket)
                                                                                 from subProp in AlphaNumeric.Between(Token.EqualTo(FilterToken.DoubleQuote), Token.EqualTo(FilterToken.DoubleQuote))
                                                                                 from __ in Token.EqualTo(FilterToken.RightSquaredBracket)
                                                                                 select $"""
                                                                                         ["{subProp.Value}"]
                                                                                         """
                                                                                              ).Many()
                                                                             select new PropertyName(string.Concat(prop.Value, string.Concat(subProps)));

    /// <summary>
    /// Parser for any text between double quotes <c>"</c>
    /// </summary>
    public static TokenListParser<FilterToken, TextExpression> Text => from _ in Token.EqualTo(FilterToken.DoubleQuote)
                                                                       from text in Token.Matching<FilterToken>(token => token != FilterToken.DoubleQuote, "Any character or symbol except double quote character")
                                                                           .AtLeastOnce()
                                                                       from __ in Token.EqualTo(FilterToken.DoubleQuote)
                                                                       select new TextExpression(TokensToString(text));

    private static IEnumerable<char> ConvertRegexToCharArray(IEnumerable<BracketValue> values)
        => values.Select(value =>
                             value switch
                             {
                                 RangeBracketValue rangeValue => Enumerable.Range(rangeValue.Start, rangeValue.End - rangeValue.Start + 1)
                                     .Select(ascii => (char)ascii),
                                 ConstantBracketValue constantValue => [.. constantValue.Value],
#if NET7_0_OR_GREATER
                    _ => throw new UnreachableException("Unexpected regex value")
#else
                                 _ => throw new NotSupportedException("Unexpected regex value")
#endif
                             })
            // Flatten collections
            .SelectMany(chr => chr);

    /// <summary>
    /// Parser for expressions that contains one or more regex parts.
    /// </summary>
    public static TokenListParser<FilterToken, OneOfExpression> OneOf
        => (
               from head in Bracket
               from body in Asterisk.Try().Cast<FilterToken, AsteriskExpression, FilterExpression>()
                   .Or(AlphaNumeric.Cast<FilterToken, ConstantValueExpression, FilterExpression>())
                   .OptionalOrDefault()
               from tail in Bracket
               select (head: (FilterExpression)head, (object)body, tail: (FilterExpression)tail)
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
                select (head, body: (object)body, tail)
               ).Try()
            .Or(
                from head in EndsWith.Try().Cast<FilterToken, EndsWithExpression, FilterExpression>()
                    .Or(StartsWith.Try().Cast<FilterToken, StartsWithExpression, FilterExpression>())
                    .Or(Constant.Try())
                    .Or(Asterisk.Try().Cast<FilterToken, AsteriskExpression, FilterExpression>())
                    .OptionalOrDefault()
                from _ in Token.EqualTo(FilterToken.LeftCurlyBrace)
                from body in Constant.AtLeastOnceDelimitedBy(Token.EqualTo(FilterToken.Or))
                from __ in Token.EqualTo(FilterToken.RightCurlyBrace)
                from tail in EndsWith.Try().Cast<FilterToken, EndsWithExpression, FilterExpression>()
                    .Or(StartsWith.Try().Cast<FilterToken, StartsWithExpression, FilterExpression>())
                    .Or(Constant.Try())
                    .Or(Asterisk.Try().Cast<FilterToken, AsteriskExpression, FilterExpression>())
                    .OptionalOrDefault()
                select (head, body: (object)body.Select(item => item).OfType<ConstantValueExpression>().ToArray(), tail)
               )
            .Select(item =>
                    {
                        switch (item)
                        {
                            // *<bracket>
                            case (AsteriskExpression, BracketExpression bracket, null):
                            {
                                return new OneOfExpression([.. ConvertRegexToCharArray(bracket.Values).Select(chr => new EndsWithExpression(chr.ToString()))]);
                            }
                            // *<bracket><constant>
                            case (AsteriskExpression, BracketExpression bracket, ConstantValueExpression tail):
                            {
                                StringSegmentLinkedList tailValue = tail.Value;
                                return new OneOfExpression([
                                    ..ConvertRegexToCharArray(bracket.Values)
                                        .Select(FilterExpression (chr) => new EndsWithExpression(new StringSegmentLinkedList(chr.ToString()).Append(tailValue)))
                                ]);
                            }
                            // <bracket>*
                            case (null, BracketExpression bracket, AsteriskExpression):
                            {
                                return new OneOfExpression([.. ConvertRegexToCharArray(bracket.Values).Select(chr => new StartsWithExpression(chr.ToString()))]);
                            }
                            // <ends with><bracket>
                            case (EndsWithExpression head, BracketExpression body, null):
                            {
                                return new OneOfExpression([.. ConvertRegexToCharArray(body.Values).Select(FilterExpression (chr) => new EndsWithExpression(new StringSegmentLinkedList().Append(head.Value).Append(chr.ToString())))]);
                            }
                            // <ends with><bracket><constant>
                            case (EndsWithExpression head, BracketExpression body, ConstantValueExpression constant):
                            {
                                return new OneOfExpression([.. ConvertRegexToCharArray(body.Values).Select(chr => new EndsWithExpression(new StringSegmentLinkedList().Append(head.Value).Append(chr.ToString()).Append(constant.Value)))]);
                            }
                            // <starts with><bracket>
                            case (StartsWithExpression head, BracketExpression body, null):
                            {
                                return new OneOfExpression([.. ConvertRegexToCharArray(body.Values).Select(chr => new AndExpression(head, new EndsWithExpression(chr.ToString())))]);
                            }
                            // <bracket><starts with>*
                            case (null, BracketExpression bracket, StartsWithExpression body):
                            {
                                return new OneOfExpression([.. ConvertRegexToCharArray(bracket.Values).Select(chr => new StartsWithExpression(new StringSegmentLinkedList(new StringSegment($"{chr}")).Append(body.Value)))]);
                            }
                            // <constant><bracket><constant>
                            case (ConstantValueExpression bracket, BracketExpression regex, ConstantValueExpression tail):
                            {
                                string bracketValueAsString = bracket.Value.ToStringValue();
                                string tailValueAsString = tail.Value.ToStringValue();
                                return new OneOfExpression([
                                    .. ConvertRegexToCharArray(regex.Values).Select(chr => new StringValueExpression($"{bracketValueAsString}{chr}{tailValueAsString}"))
                                ]);
                            }

                            // <constant><bracket>
                            case (ConstantValueExpression head, BracketExpression bracket, null):
                            {
                                string bracketValueAsString = head.Value.ToStringValue();
                                return new OneOfExpression([
                                    ..ConvertRegexToCharArray(bracket.Values).Select(chr => new StringValueExpression($"{bracketValueAsString}{chr}"))
                                ]);
                            }
                            // <bracket><constant>
                            case (null, BracketExpression bracket, ConstantValueExpression tail):
                            {
                                string tailValueAsString = tail.Value.ToStringValue();
                                return new OneOfExpression([
                                    .. ConvertRegexToCharArray(bracket.Values).Select(chr => new StringValueExpression($"{chr}{tailValueAsString}"))
                                ]);
                            }
                            // <bracket>
                            case (null, BracketExpression bracket, null):
                            {
                                return new OneOfExpression([..ConvertRegexToCharArray(bracket.Values).Select(chr => new StringValueExpression(chr.ToString()))]);
                            }
                            // <bracket><constant><bracket>
                            case (BracketExpression head, ConstantValueExpression body, BracketExpression tail):
                            {
                                string bodyValueAsString = body.Value.ToStringValue();
                                return new OneOfExpression([
                                    ..ConvertRegexToCharArray(head.Values).CrossJoin(ConvertRegexToCharArray(tail.Values))
                                        .Select(tuple => (start: tuple.Item1, end: tuple.Item2))
                                        .Select(tuple => new StringValueExpression($"{tuple.start}{bodyValueAsString}{tuple.end}"))
                                ]);
                            }
                            // <bracket><bracket>
                            case (BracketExpression head, null, BracketExpression tail):
                            {
                                return new OneOfExpression([
                                    ..ConvertRegexToCharArray(head.Values)
                                        .CrossJoin(ConvertRegexToCharArray(tail.Values))
                                        .Select(tuple => (start: tuple.Item1, end: tuple.Item2))
                                        .Select(tuple => new StringValueExpression($"{tuple.start}{tuple.end}"))
                                ]);
                            }
                            // <bracket><ends with>
                            case (null, BracketExpression body, EndsWithExpression tail):
                            {
                                return new OneOfExpression([
                                    .. ConvertRegexToCharArray(body.Values)
                                        .Select(chr => new AndExpression(new StartsWithExpression(chr.ToString()),
                                                                         tail))
                                ]);
                            }
                            // *<one of>
                            case (AsteriskExpression, IEnumerable<ConstantValueExpression> constants, null):
                            {
                                return new OneOfExpression([.. constants.Select(constant => new EndsWithExpression(constant.Value))]);
                            }
                            // <one of>*
                            case (null, IEnumerable<ConstantValueExpression> constants, AsteriskExpression):
                            {
                                return new OneOfExpression([.. constants.Select(constant => new StartsWithExpression(constant.Value))]);
                            }
                            // *<one of>*
                            case (AsteriskExpression, IEnumerable<ConstantValueExpression> constants, AsteriskExpression):
                            {
                                return new OneOfExpression([.. constants.Select(constant => new ContainsExpression(constant.Value))]);
                            }
                            // <one of><ends with>
                            case (null, IEnumerable<ConstantValueExpression> constants, EndsWithExpression endsWith):
                            {
                                return new OneOfExpression([..constants.Select(constant => constant + endsWith)]);
                            }
                            // <one of>
                            case (null, IEnumerable<ConstantValueExpression> constants, null):
                            {
                                return new OneOfExpression([.. constants]);
                            }
                            default:
                            {
                                throw new NotSupportedException($"Unsupported {nameof(OneOf)} expression :  {item}");
                            }
                        }
                    });

    /// <summary>
    /// Parser for Date and Time
    /// </summary>
    /// <returns>a <see cref="DateTimeExpression"/></returns>
    public static TokenListParser<FilterToken, DateTimeExpression> DateAndTime => (from date in Date
                                                                                   from separator in Token.EqualToValue(FilterToken.Letter, "T")
                                                                                       .Or(Token.EqualTo(FilterToken.Whitespace))
                                                                                   from time in Time
                                                                                   from offset in Offset.OptionalOrDefault()
                                                                                   select new DateTimeExpression(date,
                                                                                                                 new TimeExpression(hours: time.Hours,
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
            from sign in MinusOrPlusSign.OptionalOrDefault()
            from hourDigits in IntDigits(2)
            from _ in Colon
            from minuteDigits in IntDigits(2)
            select new OffsetExpression(sign,
                                        uint.Parse(TokensToString(hourDigits).ToStringValue(), CultureInfo.InvariantCulture),
                                        uint.Parse(TokensToString(minuteDigits).ToStringValue(), CultureInfo.InvariantCulture)));

    private static TokenListParser<FilterToken, Token<FilterToken>[]> IntDigits(int count) => Token.EqualTo(FilterToken.Digit).Repeat(count);

    /// <summary>
    /// Parser for "date" expressions.
    /// </summary>
    public static TokenListParser<FilterToken, DateExpression> Date => from year in IntDigits(4)
                                                                       from _ in Dash
                                                                       from month in IntDigits(2)
                                                                       from __ in Dash
                                                                       from day in IntDigits(2)
                                                                       select new DateExpression(int.Parse(TokensToString(year).ToStringValue(), CultureInfo.InvariantCulture),
                                                                                                 int.Parse(TokensToString(month).ToStringValue(), CultureInfo.InvariantCulture),
                                                                                                 int.Parse(TokensToString(day).ToStringValue(), CultureInfo.InvariantCulture));

    private static TokenListParser<FilterToken, Token<FilterToken>> Colon => Token.EqualTo(FilterToken.Colon);

    private static TokenListParser<FilterToken, Token<FilterToken>> Dash => Token.EqualTo(FilterToken.Dash);

    private static TokenListParser<FilterToken, Token<FilterToken>> MinusSign => Dash;

    private static TokenListParser<FilterToken, Token<FilterToken>> PlusSign => Token.EqualToValue(FilterToken.None, "+");

    private static TokenListParser<FilterToken, NumericSign> MinusOrPlusSign => MinusSign.Or(PlusSign).Optional()
        .Select(token => token switch
        {
            { Kind: FilterToken.Dash } => NumericSign.Minus,
            _                          => NumericSign.Plus
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
                                                                        select new TimeExpression(int.Parse(TokensToString(hourDigits).ToStringValue(), CultureInfo.InvariantCulture),
                                                                                                  int.Parse(TokensToString(minuteDigits).ToStringValue(), CultureInfo.InvariantCulture),
                                                                                                  int.Parse(TokensToString(secondDigits).ToStringValue(), CultureInfo.InvariantCulture),
                                                                                                  int.Parse(TokensToString(milliseconds).ToStringValue(), CultureInfo.InvariantCulture)))
        .Try()
        .Or(from hourDigits in IntDigits(2)
            from _ in Colon
            from minuteDigits in IntDigits(2)
            from __ in Colon
            from secondDigits in IntDigits(2)
            select new TimeExpression(int.Parse(TokensToString(hourDigits).ToStringValue(), CultureInfo.InvariantCulture),
                                      int.Parse(TokensToString(minuteDigits).ToStringValue(), CultureInfo.InvariantCulture),
                                      int.Parse(TokensToString(secondDigits).ToStringValue(), CultureInfo.InvariantCulture)));

    /// <summary>
    /// Parses all supported unary expressions
    /// </summary>
    private static TokenListParser<FilterToken, FilterExpression> UnaryExpression => Parse.OneOf(Parse.Ref(() => OneOf.Try().Cast<FilterToken, OneOfExpression, FilterExpression>()),
                                                                                                 Parse.Ref(() => Not.Try().Cast<FilterToken, NotExpression, FilterExpression>()),
                                                                                                 Parse.Ref(() => Group.Try().Cast<FilterToken, GroupExpression, FilterExpression>()),
                                                                                                 Parse.Ref(() => Interval.Try().Cast<FilterToken, IntervalExpression, FilterExpression>()),
                                                                                                 Parse.Ref(() => GlobalUniqueIdentifier.Try().Cast<FilterToken, GuidValueExpression, FilterExpression>()),
                                                                                                 Parse.Ref(() => Duration.Try().Cast<FilterToken, DurationExpression, FilterExpression>()),
                                                                                                 Parse.Ref(() => DateAndTime.Try().Cast<FilterToken, DateTimeExpression, FilterExpression>()),
                                                                                                 Parse.Ref(() => Time.Try().Cast<FilterToken, TimeExpression, FilterExpression>()),
                                                                                                 Parse.Ref(() => Date.Try().Cast<FilterToken, DateExpression, FilterExpression>()),
                                                                                                 // Special case of <constant>*<constant> which should be turned into an AndExpression
                                                                                                 // even though from a user perspective it's just a text filter with no specific logic in it
                                                                                                 Parse.Ref(() => StartsAndEndsWith.Try().Cast<FilterToken, AndExpression, FilterExpression>()),
                                                                                                 Parse.Ref(() => Contains.Try().Cast<FilterToken, ContainsExpression, FilterExpression>()),
                                                                                                 Parse.Ref(() => EndsWith.Try().Cast<FilterToken, EndsWithExpression, FilterExpression>()),
                                                                                                 Parse.Ref(() => StartsWith.Try().Cast<FilterToken, StartsWithExpression, FilterExpression>()),
                                                                                                 Parse.Ref(() => Bool.Try().Cast<FilterToken, StringValueExpression, FilterExpression>()),
                                                                                                 Parse.Ref(() => Text.Try().Cast<FilterToken, TextExpression, FilterExpression>()),
                                                                                                 Parse.Ref(() => AlphaNumeric.Try().Cast<FilterToken, ConstantValueExpression, FilterExpression>()),
                                                                                                 Parse.Ref(() => Number.Cast<FilterToken, NumericValueExpression, FilterExpression>()));

    /// <summary>
    /// Handles a special case of &lt;constant&gt;*&lt;constant&gt; which should be turned into a <see cref="AndExpression"/>
    /// even though from user's perspective, it's just a text filter with no specific logic in it.
    /// </summary>
    private static TokenListParser<FilterToken, AndExpression> StartsAndEndsWith
        => from alphaBeforeAsterisk in AlphaNumeric
           from __ in Asterisk
           from alphaAfterAsterisk in AlphaNumeric
           select new StartsWithExpression(alphaBeforeAsterisk.Value) & new EndsWithExpression(alphaAfterAsterisk.Value);

    /// <summary>
    /// Parser for a <c>property=&lt;expression&gt;</c> pair.
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
            from beforeDotDigits in Token.EqualTo(FilterToken.Digit).AtLeastOnce()
            from _ in Token.EqualTo(FilterToken.Dot)
            from afterDotDigits in Token.EqualTo(FilterToken.Digit).AtLeastOnce()
            from __ in Token.EqualToValueIgnoreCase(FilterToken.Letter, "e")
            from exponentSign in MinusOrPlusSign.Optional()
            from afterExponentSignDigits in IntegerOrLong
            let value = new StringSegmentLinkedList(ConvertSignToChar(sign))
                .Append(TokensToString(beforeDotDigits))
                .Append(".")
                .Append(TokensToString(afterDotDigits))
                .Append("E")
                .Append(ConvertSignToChar(exponentSign, true))
                .Append(afterExponentSignDigits.EscapedParseableString)
            select new NumericValueExpression(value))
            .Try()
            .Or(
                from sign in MinusOrPlusSign.Optional()
                from beforeDotDigits in Token.EqualTo(FilterToken.Digit).AtLeastOnce()
                from __ in Token.EqualToValueIgnoreCase(FilterToken.Letter, "e")
                from exponentSign in MinusOrPlusSign.Optional()
                from afterExponentSignDigits in IntegerOrLong
                let value = new StringSegmentLinkedList(ConvertSignToChar(sign))
                    .Append(TokensToString(beforeDotDigits))
                    .Append("E")
                    .Append(ConvertSignToChar(exponentSign, true))
                    .Append(afterExponentSignDigits.EscapedParseableString)
                select new NumericValueExpression(value))
            .Try()
            .Or(from sign in MinusOrPlusSign.Optional()
                from digitBeforeDot in Token.EqualTo(FilterToken.Digit).AtLeastOnce()
                from _ in Token.EqualTo(FilterToken.Dot)
                from digitAfterDot in Token.EqualTo(FilterToken.Digit).AtLeastOnce()
                from __ in Token.EqualToValueIgnoreCase(FilterToken.Letter, "d")
                let value = new StringSegmentLinkedList(ConvertSignToChar(sign))
                    .Append(TokensToString(digitBeforeDot))
                    .Append(".")
                    .Append(TokensToString(digitAfterDot))
                select new NumericValueExpression(value))
            .Try()
            .Or(
                from sign in MinusOrPlusSign.Optional()
                from digitBeforeDot in Token.EqualTo(FilterToken.Digit).AtLeastOnce()
                from _ in Token.EqualTo(FilterToken.Dot)
                from digitAfterDot in Token.EqualTo(FilterToken.Digit).AtLeastOnce()
                from __ in Token.EqualToValueIgnoreCase(FilterToken.Letter, "f")
                let value = new StringSegmentLinkedList(ConvertSignToChar(sign))
                    .Append(TokensToString(digitBeforeDot))
                    .Append(".")
                    .Append(TokensToString(digitAfterDot))
                select new NumericValueExpression(value))
            .Try()
            .Or(
                from sign in MinusOrPlusSign.Optional()
                from digitBeforeDot in Token.EqualTo(FilterToken.Digit).AtLeastOnce()
                from _ in Token.EqualTo(FilterToken.Dot)
                from digitAfterDot in Token.EqualTo(FilterToken.Digit).AtLeastOnce()
                let value = new StringSegmentLinkedList(ConvertSignToChar(sign))
                    .Append(TokensToString(digitBeforeDot))
                    .Append(".")
                    .Append(TokensToString(digitAfterDot))
                select new NumericValueExpression(value)
               )
            .Try()
            .Or(
                from sign in MinusOrPlusSign.Optional()
                from digits in Token.EqualTo(FilterToken.Digit).AtLeastOnce()
                from __ in Token.EqualToValueIgnoreCase(FilterToken.Letter, "d")
                let value = new StringSegmentLinkedList(ConvertSignToChar(sign))
                    .Append(TokensToString(digits))
                select new NumericValueExpression(value)
               );

    private static TokenListParser<FilterToken, NumericValueExpression> IntegerOrLong
        => (from sign in MinusOrPlusSign.Optional()
            from digits in Token.EqualTo(FilterToken.Digit).AtLeastOnce()
            from hint in Token.EqualToValueIgnoreCase(FilterToken.Letter, "L")
            select new NumericValueExpression($"{ConvertSignToChar(sign)}{TokensToString(digits)}"))
            .Try()
            .Or(from sign in MinusOrPlusSign.Optional()
                from digits in Token.EqualTo(FilterToken.Digit).AtLeastOnce()
                from hint in Token.EqualToValueIgnoreCase(FilterToken.Letter, "L")
                let value = new StringSegmentLinkedList(ConvertSignToChar(sign))
                    .Append(TokensToString(digits))
                select new NumericValueExpression(value))
            .Try()
            .Or(from sign in MinusOrPlusSign.Optional()
                from digits in Token.EqualTo(FilterToken.Digit).AtLeastOnce()
                from hint in Token.EqualToValueIgnoreCase(FilterToken.Letter, "L")
                let value = new StringSegmentLinkedList(ConvertSignToChar(sign))
                    .Append(TokensToString(digits))
                select new NumericValueExpression(value))
            .Try()
            .Or(
                from sign in MinusOrPlusSign.Optional()
                from digits in Token.EqualTo(FilterToken.Digit).AtLeastOnce()
                let value = new StringSegmentLinkedList(ConvertSignToChar(sign))
                    .Append(TokensToString(digits))
                select new NumericValueExpression(value)
               );

    private static string ConvertSignToChar(NumericSign? sign, bool mustOutputSign = false)
        => sign switch
        {
            NumericSign.Minus => "-",
            _                 => mustOutputSign ? "+" : string.Empty
        };

    /// <summary>
    /// Parser for many <see cref="Criterion"/> separated by <c>&amp;</c>.
    /// </summary>
    public static TokenListParser<FilterToken, (PropertyName, FilterExpression)[]> Criteria
        => from criteria in Criterion.ManyDelimitedBy(Token.EqualTo(FilterToken.Ampersand))
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

    private static TokenListParser<FilterToken, GuidValueExpression> GlobalUniqueIdentifier
        => from chr1 in Token.EqualTo(FilterToken.Letter).Try().Or(Token.EqualTo(FilterToken.Digit))
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
           let value = new StringSegmentLinkedList(StringSegment.Empty)
               .Append(TokensToString([chr1, chr2, chr3, chr4, chr5, chr6, chr7, chr8]))
               .Append("-")
               .Append(TokensToString([chr9, chr10, chr11, chr12]))
               .Append("-")
               .Append(TokensToString([chr13, chr14, chr15, chr16]))
               .Append("-")
               .Append(TokensToString([chr17, chr18, chr19, chr20]))
               .Append("-")
               .Append(TokensToString([chr21, chr22, chr23, chr24, chr25, chr26, chr27, chr28, chr29, chr30, chr31, chr32]))
           select new GuidValueExpression(value);

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
                                                                               select new DurationExpression(years: years is not null ? int.Parse(years.Value.ToStringValue(), CultureInfo.InvariantCulture) : 0,
                                                                                                             months: months is not null ? int.Parse(months.Value.ToStringValue(), CultureInfo.InvariantCulture) : 0,
                                                                                                             weeks: weeks is not null ? int.Parse(weeks.Value.ToStringValue(), CultureInfo.InvariantCulture) : 0,
                                                                                                             days: days is not null ? int.Parse(days.Value.ToStringValue(), CultureInfo.InvariantCulture) : 0,
                                                                                                             hours: hours is not null ? int.Parse(hours.Value.ToStringValue(), CultureInfo.InvariantCulture) : 0,
                                                                                                             minutes: minutes is not null ? int.Parse(minutes.Value.ToStringValue(), CultureInfo.InvariantCulture) : 0,
                                                                                                             seconds: seconds is not null ? int.Parse(seconds.Value.ToStringValue(), CultureInfo.InvariantCulture) : 0);

    private static TokenListParser<FilterToken, NumericValueExpression> DurationPart(string designator) => from n in IntegerOrLong
                                                                                                           from _ in Token.EqualToValue(FilterToken.Letter, designator).Try()
                                                                                                           select n;

    private static StringSegmentLinkedList TokensToString(IReadOnlyList<Token<FilterToken>> tokens)
    {
        return new StringSegmentLinkedList(tokens[0].Span.ToStringSegment(),
                                           [.. tokens.Skip(1).Select(TokenToStringSegment)]);

        static StringSegment TokenToStringSegment(Token<FilterToken> token) => token.Span.ToStringSegment();
    }
}