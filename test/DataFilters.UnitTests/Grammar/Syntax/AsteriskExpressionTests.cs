namespace DataFilters.UnitTests.Grammar.Syntax
{
    using System;

    using DataFilters.Grammar.Syntax;
    using DataFilters.UnitTests.Helpers;

    using FluentAssertions;

    using FsCheck;
    using FsCheck.Xunit;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    [UnitTest]
    [Feature(nameof(AsteriskExpression))]
    public class AsteriskExpressionTests(ITestOutputHelper outputHelper)
    {
        [Fact]
        public void IsFilterExpression() => typeof(AsteriskExpression).Should()
            .BeAssignableTo<FilterExpression>().And
            .Implement<IEquatable<AsteriskExpression>>().And
            .HaveDefaultConstructor();

        public static TheoryData<AsteriskExpression, object, bool, string> EqualsCases
            => new()
            {
                { AsteriskExpression.Instance, null, false, "Comparing to null" },
                { AsteriskExpression.Instance, AsteriskExpression.Instance, true, "Comparing to null" }
            };

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void TestEquals(AsteriskExpression first, object other, bool expected, string reason)
        {
            outputHelper.WriteLine($"First instance : {first}");
            outputHelper.WriteLine($"Second instance : {other}");

            // Act
            bool actual = first.Equals(other);
            int actualHashCode = first.GetHashCode();

            // Assert
            actual.Should()
                .Be(expected, reason);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_AsteriskExpression_GetComplexity_should_return_1() => AsteriskExpression.Instance.Complexity.Should().Be(1);

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_AsteriskExpression_When_adding_ConstantValueExpression_Should_returns_EndsWithExpression(NonNull<ConstantValueExpression> constantExpressionGenerator)
        {
            // Arrange
            AsteriskExpression asterisk = AsteriskExpression.Instance;
            ConstantValueExpression constantValueExpression = constantExpressionGenerator.Item;
            EndsWithExpression expected = new(constantValueExpression);

            // Act
            EndsWithExpression actual = asterisk + constantValueExpression;

            actual.Should().Be(expected);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_ConstantValueExpression_When_adding_AsteriskExpression_Should_returns_StartsWithWithExpression(NonNull<ConstantValueExpression> constantExpression)
        {
            // Arrange
            AsteriskExpression asterisk = AsteriskExpression.Instance;
            StartsWithExpression expected = new(constantExpression.Item);

            // Act
            StartsWithExpression actual = constantExpression.Item + asterisk;

            actual.Should().Be(expected);
        }
    }
}
