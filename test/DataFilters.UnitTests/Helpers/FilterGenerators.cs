using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using FsCheck;
using FsCheck.Fluent;

namespace DataFilters.UnitTests.Helpers
{
    using static GeneratorHelper;

    /// <summary>
    /// Helper class which contains usefull FsCheck generators.
    /// </summary>
    internal static class FilterGenerators
    {
        private readonly static Faker Faker = new();

        /// <summary>
        /// Generates a <see cref="Arbitrary{Filter}"/> where the value of the filter is a string
        /// </summary>
        internal static Arbitrary<Filter> FiltersOverString()
        {
#if NETCOREAPP3_1
            IEnumerable<FilterOperator> operators = EnumExtensions.GetValues<FilterOperator>();
#else
            IEnumerable<FilterOperator> operators = Enum.GetValues<FilterOperator>();
#endif
            Gen<FilterOperator> genOperator = Gen.OneOf(operators.Select(op => Gen.Constant(op)));

            return genOperator.Zip(GetArbitraryFor<string>().Generator)
                              .Select(tuple => (Op: tuple.Item1, Value: tuple.Item2))
                              .Select(tuple => new Filter(Faker.Hacker.Noun(), tuple.Op, tuple.Value))
                              .ToArbitrary();
        }

        /// <summary>
        /// Generates a <see cref="Arbitrary{Filter}"/> where the value of the filter is a string
        /// </summary>
        internal static Arbitrary<Filter> FiltersOverNumericValues()
        {
            Gen<FilterOperator> genOperator = Gen.OneOf(Gen.Constant(FilterOperator.LessThan),
                                                        Gen.Constant(FilterOperator.LessThanOrEqualTo),
                                                        Gen.Constant(FilterOperator.EqualTo),
                                                        Gen.Constant(FilterOperator.GreaterThan),
                                                        Gen.Constant(FilterOperator.GreaterThanOrEqual),
                                                        Gen.Constant(FilterOperator.NotEqualTo));

            return Gen.OneOf(genOperator.Zip(GetArbitraryFor<int>().Generator)
                                                         .Select(tuple => (Op: tuple.Item1, Value: tuple.Item2))
                                                         .Select(tuple => new Filter(Faker.Hacker.Noun(), tuple.Op, tuple.Value)),
                            genOperator.Zip(GetArbitraryFor<float>().Generator)
                                                         .Select(tuple => (Op: tuple.Item1, Value: tuple.Item2))
                                                         .Select(tuple => new Filter(Faker.Hacker.Noun(), tuple.Op, tuple.Value)),
                            genOperator.Zip(GetArbitraryFor<long>().Generator)
                                                         .Select(tuple => (Op: tuple.Item1, Value: tuple.Item2))
                                                         .Select(tuple => new Filter(Faker.Hacker.Noun(), tuple.Op, tuple.Value))
                                                         )
                                                         .ToArbitrary()
                                                         ;
        }

        /// <summary>
        /// Generates a <see cref="Arbitrary{Filter}"/> where the value of the filter is a string
        /// </summary>
        internal static Arbitrary<Filter> FiltersOverGenericValues<T>()
        {
#if NETCOREAPP3_1
            IEnumerable<FilterOperator> operators = EnumExtensions.GetValues<FilterOperator>();
#else
            IEnumerable<FilterOperator> operators = Enum.GetValues<FilterOperator>();
#endif
            Gen<FilterOperator> genOperator = Gen.OneOf(operators.Select(op => Gen.Constant(op)));

            return genOperator.Zip(GetArbitraryFor<T>().Generator)
                                                         .Select(tuple => (Op: tuple.Item1, Value: tuple.Item2))
                                                         .Select(tuple => new Filter(Faker.Hacker.Noun(), tuple.Op, tuple.Value))
                                                         .ToArbitrary();
        }

        /// <summary>
        /// Generates an arbitrary random <see cref="FilterExpression"/>.
        /// </summary>
        public static Arbitrary<IFilter> GenerateFilters()
        {
            Gen<IFilter>[] generators =
            [
                EndsWithFilter().Generator.Select(filter => (IFilter)filter),
                StartsWithFilter().Generator.Select(filter => (IFilter)filter),
                ContainsFilter().Generator.Select(filter => (IFilter)filter),
                EqualsWithFilter().Generator.Select(filter => (IFilter)filter),
                Gen.Sized(SafeFilterGenerator).Select(filter => (IFilter)filter),
                GreaterThanFilter<DateTime>(),
                GreaterThanOrEqualFilter<DateTime>(),
                LessThanFilter<DateTime>(),
                LessThanOrEqualFilter<DateTime>(),
                FiltersOverNumericValues().Generator.Select(filter => (IFilter)filter)
            ];

            return Gen.OneOf(generators).ToArbitrary();
        }

        internal static Arbitrary<Filter> EndsWithFilter() => GenerateFilterOverStrings(FilterOperator.EndsWith);

        internal static Arbitrary<Filter> StartsWithFilter() => GenerateFilterOverStrings(FilterOperator.StartsWith);

        internal static Arbitrary<Filter> ContainsFilter() => GenerateFilterOverStrings(FilterOperator.Contains);

        internal static Arbitrary<Filter> EqualsWithFilter() => GenerateFilterOverStrings(FilterOperator.EqualTo);

        internal static Arbitrary<Filter> GenerateFilterOverStrings(FilterOperator op)
            => GetArbitraryFor<NonEmptyString>().Generator
                                                .Select(value => new Filter(field: Faker.Hacker.Noun(), op, value))
                                                .ToArbitrary();

        private static Gen<MultiFilter> SafeFilterGenerator(int size)
        {
            Gen<MultiFilter> gen;
            Gen<FilterLogic> generateLogic = Gen.OneOf(Gen.Constant(FilterLogic.And),
                                                       Gen.Constant(FilterLogic.Or));
            switch (size)
            {
                case 0:
                    {
                        gen = GenerateFilters().Generator.Two()
                                               .Select(tuple => new[] { tuple.Item1, tuple.Item2 })
                                               .Zip(generateLogic)
                                               .Select(tuple => new MultiFilter { Logic = tuple.Item2, Filters = tuple.Item1 });
                        break;
                    }

                default:
                    {
                        Gen<MultiFilter> subtree = SafeFilterGenerator(size / 2);

                        gen = Gen.OneOf(GenerateFilters().Generator.Two()
                                               .Select(tuple => new[] { tuple.Item1, tuple.Item2 })
                                               .Zip(generateLogic)
                                               .Select(tuple => new MultiFilter { Logic = tuple.Item2, Filters = tuple.Item1 }),
                                        subtree);
                        break;
                    }
            }

            return gen;
        }

        private static Gen<IFilter> GreaterThanFilter<TValue>()
            => GenerateFilterWithSpecifiedOperatorAndValue<TValue>(FilterOperator.GreaterThan);

        private static Gen<IFilter> GreaterThanOrEqualFilter<TValue>()
            => GenerateFilterWithSpecifiedOperatorAndValue<TValue>(FilterOperator.GreaterThanOrEqual);

        private static Gen<IFilter> LessThanOrEqualFilter<TValue>()
            => GenerateFilterWithSpecifiedOperatorAndValue<TValue>(FilterOperator.LessThanOrEqualTo);

        private static Gen<IFilter> LessThanFilter<TValue>()
            => GenerateFilterWithSpecifiedOperatorAndValue<TValue>(FilterOperator.LessThanOrEqualTo);

        private static Gen<IFilter> GenerateFilterWithSpecifiedOperatorAndValue<TValue>(FilterOperator op)
            => GetArbitraryFor<TValue>().Generator
                                        .Select(value => (IFilter)new Filter(Faker.Hacker.Noun(), op, value));
    }
}
