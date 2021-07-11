namespace DataFilters.UnitTests.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DataFilters.Grammar.Syntax;

    using FsCheck;

    public static class ExpressionsGenerators
    {
        public static Arbitrary<DateTimeExpression> DateTimeExpressions()
        {
            return Arb.Default.DateTime()
                                .Filter(dateTime => dateTime.Hour >= 0 && dateTime.Minute >= 0 && dateTime.Second >= 0 && dateTime.Millisecond >= 0)
                                .Generator
                                .Select(dateTime => new DateTimeExpression(date: new(year: dateTime.Year, month: dateTime.Month, day: dateTime.Day),
                                                                                                time: new(hours: dateTime.Hour, minutes: dateTime.Minute, seconds: dateTime.Second)))
                                .ToArbitrary();
        }

        public static Arbitrary<TimeExpression> TimeExpressions()
        {
            return Arb.Default.TimeSpan()
                        .Filter(timespan => timespan.Hours >= 0 && timespan.Minutes >= 0 && timespan.Seconds >= 0 && timespan.Milliseconds >= 0)
                        .Generator
                        .Select(timespan => new TimeExpression(hours: timespan.Hours,
                                                                                            minutes: timespan.Minutes,
                                                                                            seconds: timespan.Seconds,
                                                                                            milliseconds: timespan.Milliseconds))
                        .ToArbitrary();
        }

        public static Arbitrary<DateExpression> DateExpressions()
        {
            return Arb.Default.DateTime()
                            .Filter(dateTime => dateTime.Date.Year >= 0 && dateTime.Date.Month >= 0 && dateTime.Date.Day >= 0)
                            .Generator
                            .Select(dateTime => new DateExpression(year: dateTime.Year, month: dateTime.Month, day: dateTime.Day))
                            .ToArbitrary();
        }

        public static Arbitrary<ConstantValueExpression> ConstantValueExpressions()
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
                Arb.Default.Guid().Generator.Select(value => new ConstantValueExpression(value)),
                Arb.Default.Decimal().Generator.Select(value => new ConstantValueExpression(value)),
                Arb.Default.Float().Generator.Select(value => new ConstantValueExpression(value))
            };

            return Gen.OneOf(generators)
                      .ToArbitrary();
        }

        public static Arbitrary<DurationExpression> DurationExpressions()
        {
            Arbitrary<Tuple<Tuple<Tuple<PositiveInt, PositiveInt, PositiveInt>, Tuple<PositiveInt, PositiveInt, PositiveInt>>, PositiveInt>> arb = Arb.Default.PositiveInt().Generator
                                                                                                                                                              .Three()
                                                                                                                                                              .Two()
                                                                                                                                                              .Zip(Arb.Default.PositiveInt().Generator)
                                                                                                                                                              .ToArbitrary();

            return arb.Generator.Select(tuple => new DurationExpression(years: tuple.Item1.Item1.Item1.Item,
                                           months: tuple.Item1.Item1.Item2.Item,
                                           weeks: tuple.Item1.Item1.Item3.Item,
                                           days: tuple.Item1.Item2.Item1.Item,
                                           hours: tuple.Item1.Item2.Item2.Item,
                                           minutes: tuple.Item1.Item2.Item3.Item,
                                           seconds: tuple.Item2.Item))
                    .ToArbitrary();
        }

        public static Arbitrary<EndsWithExpression> EndsWithExpressions()
            => Arb.Default.NonEmptyString()
                          .Generator
                          .Select(nonWhiteSpaceString => new EndsWithExpression(nonWhiteSpaceString.Item))
                          .ToArbitrary();

        public static Arbitrary<StartsWithExpression> StartsWithExpressions()
            => Arb.Default.NonEmptyString()
                          .Generator
                          .Select(nonWhiteSpaceString => new StartsWithExpression(nonWhiteSpaceString.Item))
                          .ToArbitrary();

        public static Arbitrary<OrExpression> OrExpressions() => Gen.Sized(SafeOrExpressionGenerator).ToArbitrary();

        public static Arbitrary<AndExpression> AndExpressions() => Gen.Sized(SafeAndExpressionGenerator).ToArbitrary();

        /// <summary>
        /// Generates an arbitrary random <see cref="FilterExpression"/>.
        /// </summary>
        public static Arbitrary<FilterExpression> GenerateFilterExpressions()
        {
            IList<Gen<FilterExpression>> generators = new List<Gen<FilterExpression>>
            {
                EndsWithExpressions().Generator.Select(item => (FilterExpression) item),
                StartsWithExpressions().Generator.Select(item => (FilterExpression) item),
                ContainsExpressions().Generator.Select(item => (FilterExpression) item),
                RangeExpressions().Generator.Select(item => (FilterExpression) item),
                DateExpressions().Generator.Select(item => (FilterExpression) item),
                DateTimeExpressions().Generator.Select(item => (FilterExpression) item),
                TimeExpressions().Generator.Select(item => (FilterExpression) item),
                DurationExpressions().Generator.Select(item => (FilterExpression) item)
            };

            return Gen.OneOf(generators).ToArbitrary();
        }

        private static Gen<OrExpression> SafeOrExpressionGenerator(int size)
        {
            Gen<OrExpression> gen;
            switch (size)
            {
                case 0:
                    {
                        gen = GenerateFilterExpressions().Generator.Two()
                                                         .Select(tuple => CreateFilterExpression((tuple.Item1, tuple.Item2), tuple => new OrExpression(tuple.Item1, tuple.Item2)));
                        break;
                    }

                default:
                    {
                        Gen<OrExpression> subtree = SafeOrExpressionGenerator(size / 2);
                        gen = Gen.OneOf(GenerateFilterExpressions().Generator.Two()
                                                         .Select(tuple => CreateFilterExpression((tuple.Item1, tuple.Item2),
                                                                                                 tuple => new OrExpression(tuple.Item1, tuple.Item2))),
                                        Gen.Two(subtree).Select(tuple => CreateFilterExpression((tuple.Item1, tuple.Item2),
                                                                                                tuple => new OrExpression(tuple.Item1, tuple.Item2))));
                        break;
                    }
            }

            return gen;
        }

        private static Gen<AndExpression> SafeAndExpressionGenerator(int size)
        {
            Gen<AndExpression> gen;
            switch (size)
            {
                case 0:
                    {
                        gen = GenerateFilterExpressions().Generator.Two()
                                                         .Select(tuple => CreateFilterExpression((tuple.Item1, tuple.Item2), tuple => new AndExpression(tuple.Item1, tuple.Item2)));
                        break;
                    }

                default:
                    {
                        Gen<AndExpression> subtree = SafeAndExpressionGenerator(size / 2);
                        gen = Gen.OneOf(GenerateFilterExpressions().Generator.Two()
                                                         .Select(tuple => CreateFilterExpression((tuple.Item1, tuple.Item2), tuple => new AndExpression(tuple.Item1, tuple.Item2))),
                                        Gen.Two(subtree).Select(tuple => CreateFilterExpression((tuple.Item1, tuple.Item2), tuple => new AndExpression(tuple.Item1, tuple.Item2))));
                        break;
                    }
            }

            return gen;
        }

        private static TFilterExpression CreateFilterExpression<TFilterExpression>((FilterExpression, FilterExpression) input, Func<(FilterExpression, FilterExpression), TFilterExpression> func)
            => func.Invoke(input);

        public static Arbitrary<RangeExpression> RangeExpressions()
        {
            IList<Gen<IBoundaryExpression>> generators = new List<Gen<IBoundaryExpression>>
            {
                DateExpressions().Generator.Select(item => (IBoundaryExpression) item),
                DateTimeExpressions().Generator.Select(item => (IBoundaryExpression) item)
            };

            return Gen.OneOf(generators)
                      .Zip(Arb.Default.Bool().Generator)
                      .Two()
                      .Select(tuple => new RangeExpression(min: new(expression: tuple.Item1.Item1, included: tuple.Item1.Item2),
                                                           max: new(expression: tuple.Item2.Item1, included: tuple.Item2.Item2)))
                      .ToArbitrary();
        }

        public static Arbitrary<ContainsExpression> ContainsExpressions()
            => Arb.Default.NonEmptyString()
                          .Convert(convertTo: input => new ContainsExpression(input.Item),
                                   convertFrom: expression => NonEmptyString.NewNonEmptyString(expression.Value));

        public static Arbitrary<BracketValue> GenerateRegexValues()
        {
            Gen<BracketValue> regexRangeGenerator = RangeBracketValueGenerator().Convert(range => (BracketValue)range,
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

        public static Arbitrary<RangeBracketValue> RangeBracketValueGenerator()
        {
            return Arb.Default.Char()
                              .Filter(chr => char.IsLetterOrDigit(chr)).Generator
                              .Two()
                              .Select(tuple => (start: tuple.Item1, end: tuple.Item2))
                              .Where(tuple => (tuple.start < tuple.end) && (TupleContainsLetter(tuple) || TupleContainsDigits(tuple)))
                              .Select(tuple => new RangeBracketValue(tuple.start, tuple.end))
                              .ToArbitrary();

            static bool TupleContainsLetter((char start, char end) tuple) => char.IsLetter(tuple.start) && char.IsLetter(tuple.end)
                                                                            && ((char.IsLower(tuple.start) && char.IsLower(tuple.end)) || (char.IsUpper(tuple.start) && char.IsUpper(tuple.end)));

            static bool TupleContainsDigits((char start, char end) tuple) => char.IsDigit(tuple.start) && char.IsDigit(tuple.end);
        }
    }
}