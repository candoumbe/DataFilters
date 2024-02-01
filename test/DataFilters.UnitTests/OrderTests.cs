namespace DataFilters.UnitTests
{
    using System;
    using DataFilters.TestObjects;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;
    using static DataFilters.OrderDirection;

    public class OrderTests(ITestOutputHelper outputHelper)
    {
        [Theory]
        [InlineData("", "Expression is empty")]
        [InlineData(" ", "Expression is whitespace only")]
        [InlineData(null, "Expression is null")]
        public void Ctor_Throws_ArgumentException_If_Expression_Is_Null(string expression, string reason)
        {
            // Act
            Action action = () => _ = new Order<object>(expression);

            // Assert
            action.Should()
                .Throw<ArgumentException>(reason)
                .Where(ex => !string.IsNullOrWhiteSpace(ex.ParamName), "Param name cannot be null")
                .Where(ex => !string.IsNullOrWhiteSpace(ex.Message), "Message cannot be null");
        }

        public static TheoryData<Order<SuperHero>, object, bool, string> EqualsCases
        {
            get
            {
                TheoryData<Order<SuperHero>, object, bool, string> cases = [];
                cases.Add
                (
                    new Order<SuperHero>("Name"),
                    new Order<SuperHero>("Name"),
                    true,
                    $"Two distinct {nameof(Order<SuperHero>)} instances with same properties must be equal"
                );

                Order<SuperHero> sort = new("Name");
                cases.Add
                (
                    sort,
                    sort,
                    true,
                    $"A {nameof(Order<SuperHero>)} instance is equal to itself"
                );

                cases.Add
                (
                    new Order<SuperHero>("Name", Descending),
                    new Order<SuperHero>("Name"),
                    false,
                    $"Two distinct {nameof(Order<SuperHero>)} instances with same {nameof(Order<SuperHero>.Expression)} but different {nameof(Order<SuperHero>.Direction)} must not be equal"
                );

                return cases;
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void EqualsTests(Order<SuperHero> first, object second, bool expected, string reason)
        {
            outputHelper.WriteLine($"{nameof(first)} : '{first}'");
            outputHelper.WriteLine($"{nameof(second)} : '{second}'");

            // Act
            bool actual = first.Equals(second);

            // Assert
            actual.Should()
                .Be(expected, reason);
        }
    }
}
