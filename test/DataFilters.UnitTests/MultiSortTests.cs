using FluentAssertions;
using System.Collections.Generic;

using Xunit;
using Xunit.Abstractions;
using static DataFilters.SortDirection;

namespace DataFilters.UnitTests
{
    public class MultiSortTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public MultiSortTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                {
                    MultiSort<SuperHero> first = new MultiSort<SuperHero>(
                        new Sort<SuperHero>(expression: nameof(SuperHero.Name)),
                        new Sort<SuperHero>(expression: nameof(SuperHero.Age), direction: Descending)
                    );

                    yield return new object[]
                    {
                        first,
                        first,
                        true,
                        $"A {nameof(MultiSort<SuperHero>)} instance is equal to itself"
                    };
                }
                {
                    MultiSort<SuperHero> first = new MultiSort<SuperHero>
                    (
                        new Sort<SuperHero>(expression: nameof(SuperHero.Name)),
                        new Sort<SuperHero>(expression: nameof(SuperHero.Age), direction: Descending)
                    );

                    MultiSort<SuperHero> second = new MultiSort<SuperHero>
                    (
                        new Sort<SuperHero>(expression: nameof(SuperHero.Name)),
                        new Sort<SuperHero>(expression: nameof(SuperHero.Age), direction: Descending)
                    );

                    yield return new object[]
                    {
                        first,
                        second,
                        true,
                        $"Two distinct {nameof(MultiSort<SuperHero>)} instances holding same data in same order"
                    };
                }

                {
                    MultiSort<SuperHero> first = new MultiSort<SuperHero>
                    (
                        new Sort<SuperHero>(expression: nameof(SuperHero.Name)),
                        new Sort<SuperHero>(expression: nameof(SuperHero.Age), direction: Descending)
                    );

                    MultiSort<SuperHero> second = new MultiSort<SuperHero>
                    (
                        new Sort<SuperHero>(expression: nameof(SuperHero.Age), direction: Descending),
                        new Sort<SuperHero>(expression: nameof(SuperHero.Name))
                    );

                    yield return new object[]
                    {
                        first,
                        second,
                        false,
                        $"Two distinct {nameof(MultiSort<SuperHero>)} instances holding same data but not same order"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void EqualsTests(ISort<SuperHero> first, object second, bool expected, string reason)
        {
            _outputHelper.WriteLine($"{nameof(first)} : '{first}'");
            _outputHelper.WriteLine($"{nameof(second)} : '{second}'");

            // Act
            bool actual = first.Equals(second);
            int actualHashCode = first.GetHashCode();

            // Assert
            actual.Should()
                .Be(expected, reason);
            if (actual)
            {
                actualHashCode.Should()
                    .Be(second?.GetHashCode(), reason);
            }
            else
            {
                actualHashCode.Should()
                    .NotBe(second?.GetHashCode(), reason);
            }
        }
    }
}
