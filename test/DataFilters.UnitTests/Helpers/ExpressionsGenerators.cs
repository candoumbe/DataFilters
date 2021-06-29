using System;
using System.Collections.Generic;
using System.Linq;

using DataFilters.Grammar.Syntax;

using FsCheck;

namespace DataFilters.UnitTests.Helpers
{
    public static class ExpressionsGenerators
    {
        public static Arbitrary<DateTimeExpression> GenerateDateTimeExpression()
        {
            return Arb.Default.DateTime()
                                .Filter(dateTime => dateTime.Hour >= 0 && dateTime.Minute >= 0 && dateTime.Second >= 0 && dateTime.Millisecond >= 0)
                                .Convert(convertTo: dateTime => new DateTimeExpression(date: new(year: dateTime.Year, month: dateTime.Month, day: dateTime.Day),
                                                                                                time: new(hours: dateTime.Hour, minutes: dateTime.Minute, seconds: dateTime.Second)),
                                           convertFrom: dateTimeExpression => new DateTime(year: dateTimeExpression.Date.Year,
                                                                                           month: dateTimeExpression.Date.Month,
                                                                                           day: dateTimeExpression.Date.Day,
                                                                                           hour: dateTimeExpression.Time.Hours,
                                                                                           minute: dateTimeExpression.Time.Hours,
                                                                                           second: dateTimeExpression.Time.Seconds)
            );
        }

        public static Arbitrary<TimeExpression> GenerateTimeExpression()
        {
            return Arb.Default.TimeSpan()
                        .Filter(timespan => timespan.Hours >= 0 && timespan.Minutes >= 0 && timespan.Seconds >= 0 && timespan.Milliseconds >= 0)
                        .Convert(convertTo: timespan => new TimeExpression(hours: timespan.Hours,
                                                                                            minutes: timespan.Minutes,
                                                                                            seconds: timespan.Seconds,
                                                                                            milliseconds: timespan.Milliseconds),
                                                  convertFrom: timeExpression => new TimeSpan(hours: timeExpression.Hours,
                                                                                              minutes: timeExpression.Minutes,
                                                                                              seconds: timeExpression.Seconds));
        }

        public static Arbitrary<DateExpression> GenerateDateExpression()
        {
            return Arb.Default.DateTime()
                            .Filter(dateTime => dateTime.Date.Year >= 0 && dateTime.Date.Month >= 0 && dateTime.Date.Day >= 0)
                            .Convert(convertTo: dateTime => new DateExpression(year: dateTime.Year, month: dateTime.Month, day: dateTime.Day),
                                     convertFrom: dateExpression => new DateTime(year: dateExpression.Year, month: dateExpression.Month, day: dateExpression.Day));
        }

        public static Arbitrary<ConstantValueExpression> GenerateConstantValueExpression()
        {
            IList<Gen<ConstantValueExpression>> generators = new List<Gen<ConstantValueExpression>>
            {
                Arb.Default.Bool().Generator.Select(value => new ConstantValueExpression(value)),
                Arb.Default.NonWhiteSpaceString().Generator.Select(value => new ConstantValueExpression(value.Item)),
                Arb.Default.Int16().Generator.Select(value => new ConstantValueExpression(value)),
                Arb.Default.Int32().Generator.Select(value => new ConstantValueExpression(value)),
                Arb.Default.Int64().Generator.Select(value => new ConstantValueExpression(value)),
                Arb.Default.DateTime().Generator.Select(value => new ConstantValueExpression(value)),
                Arb.Default.DateTimeOffset().Generator.Select(value => new ConstantValueExpression(value)),
                Arb.Default.Byte().Generator.Select(value => new ConstantValueExpression(value)),
                Arb.Default.Guid().Generator.Select(value => new ConstantValueExpression(value))
            };

            return Gen.OneOf(generators)
                      .ToArbitrary();
        }

        public static Arbitrary<DurationExpression> GenerateDurationExpression()
        {
            Arbitrary<Tuple<Tuple<Tuple<PositiveInt, PositiveInt, PositiveInt>, Tuple<PositiveInt, PositiveInt, PositiveInt>>, PositiveInt>> arb = Arb.Default.PositiveInt().Generator
                                                                                                                                                              .Three()
                                                                                                                                                              .Two()
                                                                                                                                                              .Zip(Arb.Default.PositiveInt().Generator)
                                                                                                                                                              .ToArbitrary();

            return arb.Convert(convertTo: tuple => new DurationExpression(years: tuple.Item1.Item1.Item1.Item,
                                           months: tuple.Item1.Item1.Item2.Item,
                                           weeks: tuple.Item1.Item1.Item3.Item,
                                           days: tuple.Item1.Item2.Item1.Item,
                                           hours: tuple.Item1.Item2.Item2.Item,
                                           minutes: tuple.Item1.Item2.Item3.Item,
                                           seconds: tuple.Item2.Item),
                               convertFrom: _ => default);
        }

        public static Arbitrary<EndsWithExpression> GenerateEndsWithExpression()
            => Arb.Default.NonEmptyString()
                          .Convert(convertTo: nonWhiteSpaceString => new EndsWithExpression(nonWhiteSpaceString.Item),
                                   convertFrom: expression => NonEmptyString.NewNonEmptyString(expression.Value));

        public static Arbitrary<StartsWithExpression> GenerateStartsWithExpression()
            => Arb.Default.NonEmptyString()
                          .Convert(convertTo: nonWhiteSpaceString => new StartsWithExpression(nonWhiteSpaceString.Item),
                                   convertFrom: expression => NonEmptyString.NewNonEmptyString(expression.Value));

        /// <summary>
        /// Generates an arbitrary random <see cref="FilterExpression"/>.
        /// </summary>
        public static Arbitrary<FilterExpression> GenerateFilterExpressions()
        {
            IList<Gen<FilterExpression>> generators = new List<Gen<FilterExpression>>
            {
                GenerateEndsWithExpression().Generator.Select(item => (FilterExpression) item),
                GenerateStartsWithExpression().Generator.Select(item => (FilterExpression) item),
                GenerateContainsExpression().Generator.Select(item => (FilterExpression) item),
                GenerateRangeExpression().Generator.Select(item => (FilterExpression) item)
            };

            return Gen.OneOf(generators).ToArbitrary();
        }

        public static Arbitrary<RangeExpression> GenerateRangeExpression()
        {
            IList<Gen<IBoundaryExpression>> generators = new List<Gen<IBoundaryExpression>>
            {
                GenerateDateExpression().Generator.Select(item => (IBoundaryExpression) item),
                GenerateDateTimeExpression().Generator.Select(item => (IBoundaryExpression) item)
            };

            return Gen.OneOf(generators)
                      .Zip(Arb.Default.Bool().Generator)
                      .Two()
                      .Select(tuple => new RangeExpression(min: new BoundaryExpression(expression: tuple.Item1.Item1, included: tuple.Item1.Item2),
                                                           max: new BoundaryExpression(expression: tuple.Item2.Item1, included: tuple.Item2.Item2)))
                      .ToArbitrary();
        }

        public static Arbitrary<ContainsExpression> GenerateContainsExpression()
            => Arb.Default.NonEmptyString()
                          .Convert(convertTo: input => new ContainsExpression(input.Item),
                                   convertFrom: expression => NonEmptyString.NewNonEmptyString(expression.Value));

        public static Arbitrary<BracketValue> GenerateRegexValues()
        {
            Gen<BracketValue> regexRangeGenerator = BuildRegexRangeValueGenerator().Convert(range => (BracketValue)range,
                                                                                            bracket => (RangeBracketValue)bracket)
                                                                                                          .Generator;
            Gen<BracketValue> regexConstantGenerator = BuildRegexConstantValueGenerator().Convert(range => (BracketValue)range,
                                                                                                  bracket => (ConstantBracketValue)bracket)
                                                                                         .Generator;

            return Gen.OneOf(regexConstantGenerator, regexRangeGenerator).ToArbitrary();
        }

        public static Arbitrary<ConstantBracketValue> BuildRegexConstantValueGenerator() => Arb.Default.String()
                                                                                                   .Filter(input => !string.IsNullOrWhiteSpace(input)
                                                                                                                    && input.Length > 1
                                                                                                                    && input.All(chr => char.IsLetterOrDigit(chr)))
                                                                                                   .Generator
                                                                                                   .Select(item => new ConstantBracketValue(item))
                                                                                                   .ToArbitrary();

        public static Arbitrary<RangeBracketValue> BuildRegexRangeValueGenerator()
        {
            return Arb.Default.Char()
                              .Filter(chr => char.IsLetterOrDigit(chr)).Generator
                              .Two()
                              .Select(tuple => (start: tuple.Item1, end: tuple.Item2))
                              .Where(tuple => (tuple.start < tuple.end) && (TupleContainsLetter(tuple) || TupleContainsDigits(tuple)))
                              .Select(tuple => new RangeBracketValue(tuple.start, tuple.end))
                              .ToArbitrary();

            static bool TupleContainsLetter((char start, char end) tuple) => char.IsLetter(tuple.start) && char.IsLetter(tuple.end)
                                                                            && (( char.IsLower(tuple.start) && char.IsLower(tuple.end)  )|| (char.IsUpper(tuple.start) && char.IsUpper(tuple.end)));

            static bool TupleContainsDigits((char start, char end) tuple) => char.IsDigit(tuple.start) && char.IsDigit(tuple.end);
        }
    }
}