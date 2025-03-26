namespace DataFilters.UnitTests.Grammar.Syntax;

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

        _ = expected switch
        {
            true => actualHashCode.Should()
                .Be(other?.GetHashCode(), reason),
            _ => actualHashCode.Should()
                .NotBe(other?.GetHashCode(), reason)
        };
    }

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Given_AsteriskExpression_GetComplexity_should_return_1() => AsteriskExpression.Instance.Complexity.Should().Be(1);

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Given_AsteriskExpression_When_adding_ConstantValueExpression_Should_returns_EndsWithExpression(NonNull<ConstantValueExpression> constantExpression)
    {
        // Arrange
        AsteriskExpression asterisk = AsteriskExpression.Instance;
        EndsWithExpression expected = new(constantExpression.Item.Value);

        // Act
        EndsWithExpression actual = asterisk + constantExpression.Item;

        actual.Should().Be(expected);
    }

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Given_ConstantValueExpresion_When_adding_AsteriskExpression_Should_returns_StartsWithWithExpression(NonNull<ConstantValueExpression> constantExpression)
    {
        // Arrange
        AsteriskExpression asterisk = AsteriskExpression.Instance;
        StartsWithExpression expected = new(constantExpression.Item.Value);

        // Act
        StartsWithExpression actual = constantExpression.Item + asterisk;

        actual.Should().Be(expected);
    }
}