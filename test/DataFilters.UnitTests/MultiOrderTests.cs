namespace DataFilters.UnitTests
{
    using System.Collections.Generic;
    using DataFilters.TestObjects;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;
    using static DataFilters.OrderDirection;

    public class MultiOrderTests(ITestOutputHelper outputHelper)
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S1199:Nested code blocks should not be used")]
        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                {
                    MultiOrder<SuperHero> first = new(
                        new Order<SuperHero>(expression: nameof(SuperHero.Nickname)),
                        new Order<SuperHero>(expression: nameof(SuperHero.Age), direction: Descending)
                    );

                    yield return new object[]
                    {
                        first,
                        first,
                        true,
                        $"A {nameof(MultiOrder<SuperHero>)} instance is equal to itself"
                    };
                }
                {
                    MultiOrder<SuperHero> first = new(
                        new Order<SuperHero>(expression: nameof(SuperHero.Nickname)),
                        new Order<SuperHero>(expression: nameof(SuperHero.Age), direction: Descending)
                    );

                    MultiOrder<SuperHero> second = new
                    (
                        new Order<SuperHero>(expression: nameof(SuperHero.Nickname)),
                        new Order<SuperHero>(expression: nameof(SuperHero.Age), direction: Descending)
                    );

                    yield return new object[]
                    {
                        first,
                        second,
                        true,
                        $"Two distinct {nameof(MultiOrder<SuperHero>)} instances holding same data in same order"
                    };
                }

                {
                    MultiOrder<SuperHero> first = new
                    (
                        new Order<SuperHero>(expression: nameof(SuperHero.Nickname)),
                        new Order<SuperHero>(expression: nameof(SuperHero.Age), direction: Descending)
                    );

                    MultiOrder<SuperHero> second = new
                    (
                        new Order<SuperHero>(expression: nameof(SuperHero.Age), direction: Descending),
                        new Order<SuperHero>(expression: nameof(SuperHero.Nickname))
                    );

                    yield return new object[]
                    {
                        first,
                        second,
                        false,
                        $"Two distinct {nameof(MultiOrder<SuperHero>)} instances holding same data but not same order"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void EqualsTests(IOrder<SuperHero> first, object second, bool expected, string reason)
        {
            outputHelper.WriteLine($"{nameof(first)} : '{first}'");
            outputHelper.WriteLine($"{nameof(second)} : '{second}'");

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
