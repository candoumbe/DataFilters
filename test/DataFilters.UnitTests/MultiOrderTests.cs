namespace DataFilters.UnitTests;

using DataFilters.TestObjects;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using static DataFilters.OrderDirection;

public class MultiOrderTests(ITestOutputHelper outputHelper)
{
    public static TheoryData<MultiOrder<SuperHero>, object, bool, string> EqualsCases
    {
        get
        {
            TheoryData<MultiOrder<SuperHero>, object, bool, string> cases = [];
            {
                MultiOrder<SuperHero> first = new(
                    new Order<SuperHero>(expression: nameof(SuperHero.Nickname)),
                    new Order<SuperHero>(expression: nameof(SuperHero.Age), direction: Descending)
                );

                cases.Add
                (
                    first,
                    first,
                    true,
                    $"A {nameof(MultiOrder<SuperHero>)} instance is equal to itself"
                );
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

                cases.Add
                (
                    first,
                    second,
                    true,
                    $"Two distinct {nameof(MultiOrder<SuperHero>)} instances holding same data in same order"
                );
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

                cases.Add
                (
                    first,
                    second,
                    false,
                    $"Two distinct {nameof(MultiOrder<SuperHero>)} instances holding same data but not same order"
                );
            }

            return cases;
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
    }
}