namespace DataFilters.UnitTests.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using DataFilters.Grammar.Parsing;
    using DataFilters.Grammar.Syntax;
    using FsCheck;
    using FsCheck.Fluent;
    using static GeneratorHelper;

    public static class ExpressionsGenerators
    {
        public static Arbitrary<DateTimeExpression> DateTimeExpressions()
        {
            return GetArbitraryFor<DateTime>()
                                .Filter(dateTime => dateTime.Hour >= 0
                                                    && dateTime.Minute >= 0
                                                    && dateTime.Second >= 0
                                                    && dateTime.Millisecond >= 0)
                                .Generator
                                .Zip(TimeExpressions().Generator)
                                .Select(val => (date: val.Item1, time: val.Item2))
                                .Zip(OffsetExpressions().Generator)
                                .Select(val => (val.Item1.date, val.Item1.time, offset: val.Item2))
                                .Where(val => val.time != null || val.offset != null)
                                .Select(dateTime => new DateTimeExpression(date: new(year: dateTime.date.Year, month: dateTime.date.Month, day: dateTime.date.Day),
                                                                           time: new(hours: dateTime.date.Hour, minutes: dateTime.date.Minute, seconds: dateTime.date.Second, milliseconds: dateTime.date.Millisecond),
                                                                           offset: dateTime.offset))
                                .ToArbitrary();
        }

        public static Arbitrary<TimeExpression> TimeExpressions()
        {
            return GetArbitraryFor<TimeSpan>()
                        .Filter(timespan => timespan.Hours >= 0 && timespan.Minutes >= 0 && timespan.Seconds >= 0 && timespan.Milliseconds >= 0)
                        .Generator
                        .Select(timespan => new TimeExpression(hours: timespan.Hours,
                                                               minutes: timespan.Minutes,
                                                               seconds: timespan.Seconds,
                                                               milliseconds: timespan.Milliseconds))
                        .ToArbitrary();
        }

        public static Arbitrary<OffsetExpression> OffsetExpressions()
        {
            Gen<int> hours = Gen.Choose(0, 23);
            Gen<int> minutes = Gen.Choose(0, 59);
            Gen<NumericSign> sign = Gen.OneOf(Gen.Constant(NumericSign.Plus), Gen.Constant(NumericSign.Minus));

            return hours.Zip(minutes, (hh, mm) => (hours: hh, minutes: mm))
                        .Zip(sign, (offset, s) => (offset.hours, offset.minutes, sign: s))
                        .Select(val => (val.hours, val.minutes, val.sign))
                        .Select(val => new OffsetExpression(val.sign, (uint)val.hours, (uint)val.minutes))
                                               .OrNull()
                                               .ToArbitrary();
        }

        public static Arbitrary<DateExpression> DateExpressions()
        {
            Gen<DateExpression> dateTimeGenerator = GetArbitraryFor<DateTime>()
                                        .Generator
                                        .Select(dateTime => new DateExpression(year: dateTime.Year, month: dateTime.Month, day: dateTime.Day));

#if NET6_0_OR_GREATER
            Gen<DateExpression> dateOnlyGenerator = GetArbitraryFor<DateTime>()
                .Generator
                .Select(DateOnly.FromDateTime)
                .Select(date => new DateExpression(year: date.Year, month: date.Month, day: date.Day));
#endif

#if !NET6_0_OR_GREATER
            return dateTimeGenerator.ToArbitrary();
#else
            return Gen.OneOf(dateTimeGenerator, dateOnlyGenerator)
                .ToArbitrary();
#endif
        }

        public static Arbitrary<ConstantValueExpression> ConstantValueExpressions()
        {
            IList<Gen<ConstantValueExpression>> generators = new List<Gen<ConstantValueExpression>>
            {
                TextExpressions().Generator.Select(value => (ConstantValueExpression) value),
                StringValueExpressions().Generator.Select(value => (ConstantValueExpression) value),
                GetArbitraryFor<bool>().Generator.Select(value => (ConstantValueExpression) new StringValueExpression(value.ToString())),
                GetArbitraryFor<Guid>().Generator.Select(value => (ConstantValueExpression) new GuidValueExpression(value.ToString("d"))),
                NumericValueExpressions().Generator.Select(value => (ConstantValueExpression) value)
            };

            return Gen.OneOf(generators)
                      .ToArbitrary();
        }

        public static Arbitrary<NumericValueExpression> NumericValueExpressions()
        {
            IEnumerable<Gen<NumericValueExpression>> generators = new Gen<NumericValueExpression>[]
            {
                GetArbitraryFor<int>().Generator.Select(value => new NumericValueExpression(value.ToString(CultureInfo.InvariantCulture))),
                GetArbitraryFor<long>().Generator.Select(value => new NumericValueExpression(value.ToString(CultureInfo.InvariantCulture))),
                GetArbitraryFor<NormalFloat>().Generator.Select(value => new NumericValueExpression(value.Item.ToString(CultureInfo.InvariantCulture)))
            };

            return Gen.OneOf(generators)
                      .ToArbitrary();
        }

        public static Arbitrary<StringValueExpression> StringValueExpressions()
            => GetArbitraryFor<NonEmptyString>().Generator
                                                .Select(value => value.Item.Any(chr => FilterTokenizer.SpecialCharacters.Contains(chr))
                                                    ? new TextExpression(value.Item)
                                                    : new StringValueExpression(value.Item)
                                                )
                                                .ToArbitrary();

        public static Arbitrary<DurationExpression> DurationExpressions()
        {
            Arbitrary<(((PositiveInt years, PositiveInt months, PositiveInt days) date, (PositiveInt hours, PositiveInt minutes, PositiveInt seconds) time) dateTime, PositiveInt milliseconds)> arb = GetArbitraryFor<PositiveInt>().Generator
                                                                                                                                                              .Three()
                                                                                                                                                              .Two()
                                                                                                                                                              .Zip(GetArbitraryFor<PositiveInt>().Generator)
                                                                                                                                                              .ToArbitrary();

            return arb.Generator.Select(tuple => new DurationExpression(years: tuple.dateTime.date.years.Item,
                                                                        months: tuple.dateTime.date.months.Item,
                                                                        weeks: tuple.dateTime.date.days.Item,
                                                                        days: tuple.dateTime.time.hours.Item,
                                                                        hours: tuple.dateTime.time.minutes.Item,
                                                                        minutes: tuple.dateTime.time.seconds.Item,
                                                                        seconds: tuple.milliseconds.Item))
                    .ToArbitrary();
        }

        public static Arbitrary<EndsWithExpression> EndsWithExpressions()
            => Gen.OneOf(GetArbitraryFor<NonEmptyString>().Generator
                                                          .Select(nonWhiteSpaceString => new EndsWithExpression(nonWhiteSpaceString.Item)),
                         TextExpressions().Generator.Select(text => new EndsWithExpression(text))
                        )
                        .ToArbitrary();

        public static Arbitrary<StartsWithExpression> StartsWithExpressions()
            => Gen.OneOf(GetArbitraryFor<NonEmptyString>().Generator
                                                          .Select(nonWhiteSpaceString => new StartsWithExpression(nonWhiteSpaceString.Item)),
                         TextExpressions().Generator.Select(text => new StartsWithExpression(text))
                        )
                        .ToArbitrary();

        public static Arbitrary<OrExpression> OrExpressions() => Gen.Sized(SafeOrExpressionGenerator).ToArbitrary();

        public static Arbitrary<AndExpression> AndExpressions() => Gen.Sized(SafeAndExpressionGenerator).ToArbitrary();

        public static Arbitrary<GroupExpression> GroupExpressions() => Gen.Sized(SafeGroupExpressionGenerator).ToArbitrary();

        private static Gen<GroupExpression> SafeGroupExpressionGenerator(int size)
        {
            Gen<GroupExpression> gen;
            switch (size)
            {
                case 0:
                    {
                        gen = GenerateFilterExpressions().Generator
                                                         .Select(expr => new GroupExpression(expr));
                        break;
                    }

                default:
                    {
                        Gen<GroupExpression> subtree = SafeGroupExpressionGenerator(size / 2);
                        gen = Gen.OneOf(GenerateFilterExpressions().Generator.Select(expr => new GroupExpression(expr)),
                                        subtree.Select(expr => new GroupExpression(expr)));
                        break;
                    }
            }

            return gen;
        }

        /// <summary>
        /// Generates an arbitrary random <see cref="FilterExpression"/>.
        /// </summary>
        public static Arbitrary<FilterExpression> GenerateFilterExpressions()
        {
            Gen<FilterExpression>[] generators =
            [
                EndsWithExpressions().Generator.Select(item => (FilterExpression) item),
                StartsWithExpressions().Generator.Select(item => (FilterExpression) item),
                ContainsExpressions().Generator.Select(item => (FilterExpression) item),
                IntervalExpressions().Generator.Select(item => (FilterExpression) item),
                DateExpressions().Generator.Select(item => (FilterExpression) item),
                DateTimeExpressions().Generator.Select(item => (FilterExpression) item),
                TimeExpressions().Generator.Select(item => (FilterExpression) item),
                DurationExpressions().Generator.Select(item => (FilterExpression) item),
                ConstantValueExpressions().Generator.Select(item => (FilterExpression) item),
                GroupExpressions().Generator.Select(item => (FilterExpression) item)
            ];

            return Gen.OneOf(generators).ToArbitrary();
        }

        /// <summary>
        /// Generates an arbitrary random <see cref="BinaryFilterExpression"/>.
        /// </summary>
        /// <returns></returns>
        public static Arbitrary<BinaryFilterExpression> BinaryFilterExpressions()
        {
            Gen<BinaryFilterExpression>[] generators =
            [
                AndExpressions().Generator.Select(item => (BinaryFilterExpression) item),
                OrExpressions().Generator.Select(item => (BinaryFilterExpression) item)
            ];

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
                                        subtree.Two().Select(tuple => CreateFilterExpression((tuple.Item1, tuple.Item2),
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
                                                         .Select(tuple => CreateFilterExpression((tuple.Item1, tuple.Item2),
                                                                                                 tuple => new AndExpression(tuple.Item1, tuple.Item2)));
                        break;
                    }

                default:
                    {
                        Gen<AndExpression> subtree = SafeAndExpressionGenerator(size / 2);
                        gen = Gen.OneOf(GenerateFilterExpressions().Generator.Two()
                                                                             .Select(tuple => CreateFilterExpression((tuple.Item1, tuple.Item2), tuple => new AndExpression(tuple.Item1, tuple.Item2))),
                                        subtree.Two().Select(tuple => CreateFilterExpression((tuple.Item1, tuple.Item2), tuple => new AndExpression(tuple.Item1, tuple.Item2))));
                        break;
                    }
            }

            return gen;
        }

        public static Arbitrary<NotExpression> NotExpressions()
        {
            return Gen.Sized(SafeNotExpressionGenerator).ToArbitrary();

            static Gen<NotExpression> SafeNotExpressionGenerator(int size)
            {
                Gen<FilterExpression>[] generators =
                [
                    AndExpressions().Generator.Select(expr => (FilterExpression)expr),
                    OrExpressions().Generator.Select(expr => (FilterExpression)expr),
                    ConstantValueExpressions().Generator.Select(expr => (FilterExpression)expr),
                    DurationExpressions().Generator.Select(expr => (FilterExpression)expr),
                    DateExpressions().Generator.Select(expr => (FilterExpression)expr),
                    TimeExpressions().Generator.Select(expr => (FilterExpression)expr),
                    DateTimeExpressions().Generator.Select(expr => (FilterExpression)expr),
                    IntervalExpressions().Generator.Select(expr => (FilterExpression)expr)
                ];

                Gen<NotExpression> gen;
                switch (size)
                {
                    case 0:
                        {
                            gen = Gen.OneOf(generators).Select(expr => new NotExpression(expr));
                            break;
                        }

                    default:
                        {
                            Gen<NotExpression> subtree = SafeNotExpressionGenerator(size / 2);
                            gen = Gen.OneOf(Gen.OneOf(generators).Select(exp => new NotExpression(exp)),
                                            subtree.Select(expr => new NotExpression(expr)));
                            break;
                        }
                }

                return gen;
            }
        }

        private static TFilterExpression CreateFilterExpression<TFilterExpression>((FilterExpression, FilterExpression) input, Func<(FilterExpression, FilterExpression), TFilterExpression> func)
            => func.Invoke(input);

        public static Arbitrary<IntervalExpression> IntervalExpressions()
        {
            Gen<bool> boolGenerator = GetArbitraryFor<bool>().Generator;
            (Gen<IBoundaryExpression> gen, Gen<bool> included)[] datesGen = new[]
            {
                (DateExpressions().Generator.Select(item => (IBoundaryExpression) item), boolGenerator),
                (DateTimeExpressions().Generator.Select(item => (IBoundaryExpression) item), boolGenerator)
            };

            (Gen<IBoundaryExpression> gen, Gen<bool> included)[] numericsGen = new[]
            {
                (GetArbitraryFor<short>().Generator.Select(value => (IBoundaryExpression) new NumericValueExpression(value.ToString())), boolGenerator),
                (GetArbitraryFor<int>().Generator.Select(value => (IBoundaryExpression) new NumericValueExpression(value.ToString())), boolGenerator),
                (GetArbitraryFor<long>().Generator.Select(value => (IBoundaryExpression) new NumericValueExpression(value.ToString())), boolGenerator),
                (GetArbitraryFor<NormalFloat>().Generator.Select(value => (IBoundaryExpression) new NumericValueExpression(value.Item.ToString("G19", CultureInfo.InvariantCulture))), boolGenerator)
            };

            (Gen<IBoundaryExpression> gen, Gen<bool> included) timeGen = (TimeExpressions().Generator.Select(item => (IBoundaryExpression)item), boolGenerator);
            (Gen<IBoundaryExpression> gen, Gen<bool> included) asteriskGen = (Gen.Constant((IBoundaryExpression)AsteriskExpression.Instance), Gen.Constant(false));

            IEnumerable<Gen<IntervalExpression>> generatorsWithMinAndMax = datesGen.CrossJoin(datesGen)
                                                                                   .Concat(datesGen.CrossJoin(new[] { timeGen }))
                                                                                   .Select(tuple => (min: tuple.Item1, max: tuple.Item2))
                                                                                   .Select(tuple => CreateIntervalExpressionGenerator((tuple.min.gen, tuple.min.included),
                                                                                                                                      (tuple.max.gen, tuple.max.included)))
                                                                                   .Concat(numericsGen.CrossJoin(numericsGen)
                                                                                           .Select(tuple => (min: tuple.Item1, max: tuple.Item2))
                                                                                           .Select(tuple => CreateIntervalExpressionGenerator((tuple.min.gen, tuple.min.included),
                                                                                                                                              (tuple.max.gen, tuple.max.included))))
                                         ;

            IEnumerable<Gen<IntervalExpression>> generatorsWithMinOrMaxOnly = datesGen.CrossJoin(new[] { asteriskGen })
                                                                                      .Concat(new[] { asteriskGen }.CrossJoin(datesGen))
                                                                                      .Select(tuple => (min: tuple.Item1, max: tuple.Item2))
                                                                                      .Select(tuple => CreateIntervalExpressionGenerator((tuple.min.gen, tuple.min.included),
                                                                                                                                         (tuple.max.gen, tuple.max.included)));

            return Gen.OneOf(generatorsWithMinAndMax.Concat(generatorsWithMinOrMaxOnly))
                      .ToArbitrary();
        }

        public static Arbitrary<BoundaryExpression> BoundariesExpressions()
        {
            Gen<bool> boolGenerator = GetArbitraryFor<bool>().Generator;

            IList<Gen<BoundaryExpression>> generators = new List<Gen<BoundaryExpression>>
            {
                CreateBoundaryGenerator(DateExpressions().Generator.Select(item => (IBoundaryExpression) item), boolGenerator),
                CreateBoundaryGenerator(DateTimeExpressions().Generator.Select(item => (IBoundaryExpression) item), boolGenerator),
                CreateBoundaryGenerator(TimeExpressions().Generator.Select(item => (IBoundaryExpression) item), boolGenerator),
                CreateBoundaryGenerator(Gen.Constant((IBoundaryExpression)AsteriskExpression.Instance), Gen.Constant(false)),
            };

            return Gen.OneOf(generators)
                      .ToArbitrary();
        }

        private static Gen<IntervalExpression> CreateIntervalExpressionGenerator((Gen<IBoundaryExpression> boundaryGenerator, Gen<bool> includedGenerator) genMin, (Gen<IBoundaryExpression> boundaryGenerator, Gen<bool> includedGenerator) genMax)
        {
            return CreateBoundaryGenerator(genMin.boundaryGenerator, genMin.includedGenerator)
                    .Zip(CreateBoundaryGenerator(genMax.boundaryGenerator, genMax.includedGenerator))
                    .Select(tuple => (min: tuple.Item1, max: tuple.Item2))
                    .Select(tuple => new IntervalExpression(min: new BoundaryExpression(tuple.min.Expression, tuple.min.Included),
                                                            max: new BoundaryExpression(tuple.max.Expression, tuple.max.Included)));
        }

        private static Gen<BoundaryExpression> CreateBoundaryGenerator(Gen<IBoundaryExpression> boundaryGenerator, Gen<bool> includedGenerator)
        {
            return boundaryGenerator.Zip(includedGenerator)
                                    .Select(tuple => new BoundaryExpression(expression: tuple.Item1,
                                                                            included: tuple.Item2));
        }

        public static Arbitrary<ContainsExpression> ContainsExpressions()
            => Gen.OneOf(GetArbitraryFor<NonEmptyString>().Generator
                                                          .Select(nonWhiteSpaceString => new ContainsExpression(nonWhiteSpaceString.Item)),
                         TextExpressions().Generator.Select(text => new ContainsExpression(text))
                        )
                        .ToArbitrary();

        public static Arbitrary<BracketValue> GenerateRegexValues()
        {
            Gen<BracketValue> regexRangeGenerator = RangeBracketValues().Convert(range => (BracketValue)range,
                                                                                 bracket => (RangeBracketValue)bracket)
                                                                                                          .Generator;
            Gen<BracketValue> regexConstantGenerator = ConstantBracketValues().Convert(range => (BracketValue)range,
                                                                                       bracket => (ConstantBracketValue)bracket)
                                                                                         .Generator;

            return Gen.OneOf(regexConstantGenerator, regexRangeGenerator).ToArbitrary();
        }

        private static Arbitrary<ConstantBracketValue> ConstantBracketValues()
            => GetArbitraryFor<string>().Filter(input => !string.IsNullOrWhiteSpace(input)
                                                        && input.Length > 1
                                                        && input.All(char.IsLetterOrDigit))
                                        .Generator
                                        .Select(item => new ConstantBracketValue(item))
                                        .ToArbitrary();

        private static Arbitrary<RangeBracketValue> RangeBracketValues()
        {
            return GetArbitraryFor<char>()
                              .Filter(char.IsLetterOrDigit).Generator
                              .Two()
                              .Select(tuple => (start: tuple.Item1, end: tuple.Item2))
                              .Where(tuple => (tuple.start < tuple.end) && (TupleContainsLetter(tuple) || TupleContainsDigits(tuple)))
                              .Select(tuple => new RangeBracketValue(tuple.start, tuple.end))
                              .ToArbitrary();

            static bool TupleContainsLetter((char start, char end) tuple) => char.IsLetter(tuple.start) && char.IsLetter(tuple.end)
                                                                            && ((char.IsLower(tuple.start) && char.IsLower(tuple.end)) || (char.IsUpper(tuple.start) && char.IsUpper(tuple.end)));

            static bool TupleContainsDigits((char start, char end) tuple) => char.IsDigit(tuple.start) && char.IsDigit(tuple.end);
        }

        /// <summary>
        /// <see cref="BracketExpression"/> generator
        /// </summary>
        /// <returns><see cref="Arbitrary{BracketExpression}"/></returns>
        public static Arbitrary<BracketExpression> BracketExpressions()
            => Gen.OneOf(RangeBracketValues().Generator.Select(x => (BracketValue)x),
                         ConstantBracketValues().Generator.Select(x => (BracketValue)x))
                .ArrayOf()
                .Select(brackets => new BracketExpression(brackets))
                .ToArbitrary();

        public static Arbitrary<TextExpression> TextExpressions()
            => ArbMap.Default.ArbFor<NonEmptyString>()
                             .Generator
                             .Select(val => new TextExpression(val.Item))
                             .ToArbitrary();
    }
}