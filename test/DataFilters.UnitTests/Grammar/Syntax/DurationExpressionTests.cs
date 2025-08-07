
using System;
using DataFilters.Grammar.Syntax;
using DataFilters.UnitTests.Helpers;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace DataFilters.UnitTests.Grammar.Syntax;
[UnitTest]
public class DurationExpressionTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public void IsFilterExpression() => typeof(DurationExpression).Should()
        .BeDerivedFrom<FilterExpression>();

    [Property]
    public void Throws_ArgumentOutOfRangeException_when_any_ctor_input_is_negative(int years, int months, int weeks, int days, int hours, int minutes, int seconds)
    {
        Action invokingConstructor = () => _ = new DurationExpression(years, months, weeks, days, hours, minutes, seconds);

        object __ = (years, months, weeks, days, hours, minutes, seconds) switch
        {
            var tuple when tuple.years < 0
                           || tuple.months < 0
                           || tuple.weeks < 0
                           || tuple.days < 0
                           || tuple.hours < 0
                           || tuple.minutes < 0
                           || tuple.seconds < 0 => invokingConstructor.Should().ThrowExactly<ArgumentOutOfRangeException>(),
            _ => invokingConstructor.Should().NotThrow()
        };
    }

    [Property]
    public void Set_properties_accordingly(PositiveInt years, PositiveInt months, PositiveInt weeks, PositiveInt days, PositiveInt hours, PositiveInt minutes, PositiveInt seconds)
    {
        DurationExpression duration = new(years.Item, months.Item, weeks.Item, days.Item, hours.Item, minutes.Item, seconds.Item);

        duration.Years.Should().Be(years.Item);
        duration.Months.Should().Be(months.Item);
        duration.Days.Should().Be(days.Item);
        duration.Hours.Should().Be(hours.Item);
        duration.Minutes.Should().Be(minutes.Item);
        duration.Seconds.Should().Be(seconds.Item);
    }

    [Property]
    public void Equals_depends_on_values_only(PositiveInt years, PositiveInt months, PositiveInt weeks, PositiveInt days, PositiveInt hours, PositiveInt minutes, PositiveInt seconds)
    {
        DurationExpression first = new(years.Item, months.Item, weeks.Item, days.Item, hours.Item, minutes.Item, seconds.Item);
        DurationExpression other = new(years.Item, months.Item, weeks.Item, days.Item, hours.Item, minutes.Item, seconds.Item);

        // Act
        bool actual = first.Equals(other);
        int firstHashCode = first.GetHashCode();
        int otherHashCode = other.GetHashCode();

        // Assert
        actual.Should().BeTrue();
        firstHashCode.Should().Be(otherHashCode);
    }

    [Property]
    public void Two_durations_that_are_equal_are_equivalent(PositiveInt years, PositiveInt months, PositiveInt weeks, PositiveInt days, PositiveInt hours, PositiveInt minutes, PositiveInt seconds)
    {
        DurationExpression first = new(years.Item, months.Item, weeks.Item, days.Item, hours.Item, minutes.Item, seconds.Item);
        DurationExpression other = new(years.Item, months.Item, weeks.Item, days.Item, hours.Item, minutes.Item, seconds.Item);

        // Act
        bool firstEqualsOther = first.Equals(other);
        bool firstIsEquivalentToOther = first.IsEquivalentTo(other);

        // Assert
        firstEqualsOther.Should().BeTrue();
        firstIsEquivalentToOther.Should().BeTrue();
    }

    [Property]
    public void Two_durations_are_equivalent_when_they_represent_same_timespan()
    {
        // Arrange
        DurationExpression first = new(hours: 24);
        DurationExpression other = new(days: 1);

        // Act
        bool firstEqualsOther = first.Equals(other);
        bool firstIsEquivalentToOther = first.IsEquivalentTo(other);

        // Assert
        firstEqualsOther.Should().BeFalse("two durations initialized with timespans that are not in the same unit of time");
        firstIsEquivalentToOther.Should().BeTrue("two durations initialized from timespans that are equal should be equivalent");
    }

    [Property]
    public void Equals_to_null_always_returns_false(PositiveInt years, PositiveInt months, PositiveInt weeks, PositiveInt days, PositiveInt hours, PositiveInt minutes, PositiveInt seconds)
    {
        // Arrange
        DurationExpression duration = new(years.Item, months.Item, weeks.Item, days.Item, hours.Item, minutes.Item, seconds.Item);

        // Act
        bool durationEqualsToNull = duration.Equals(null);

        // Assert
        durationEqualsToNull.Should().BeFalse();
    }

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Equals_should_be_commutative(NonNull<DurationExpression> first, FilterExpression second)
        => first.Item.Equals(second).Should().Be(second.Equals(first.Item));

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Equals_should_be_reflexive(NonNull<DurationExpression> expression)
        => expression.Item.Equals(expression.Item).Should().BeTrue();

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Equals_should_be_symetric(NonNull<DurationExpression> expression, NonNull<FilterExpression> otherExpression)
        => expression.Item.Equals(otherExpression.Item).Should().Be(otherExpression.Item.Equals(expression.Item));

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Given_DurationExpression_GetComplexity_should_return_1(DurationExpression duration)
        => duration.Complexity.Should().Be(1);

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void IsEquivalentTo_should_be_transitive(DurationExpression duration, NonNull<FilterExpression> other, NonNull<FilterExpression> third)
    {
        // Arrange
        outputHelper.WriteLine($"Duration : {duration}");
        outputHelper.WriteLine($"Other : {other.Item.EscapedParseableString}");
        outputHelper.WriteLine($"Third : {third.Item.EscapedParseableString}");

        bool first = duration.IsEquivalentTo(other.Item);
        bool second = other.Item.IsEquivalentTo(third.Item);

        // Act
        bool actual = duration.IsEquivalentTo(third.Item);

        // Assert
        if (first && second)
        {
            actual.Should().BeTrue();
        }
    }

    public static TheoryData<DurationExpression, FilterExpression, FilterExpression, (bool expected, string reason)> IsEquivalentToCases
        => new()
        {
            {
                new DurationExpression(),
                new StartsWithExpression("a"),
                new StartsWithExpression("a"),
                (
                    expected: false,
                    reason: "A is not equivalent to B and B is equivalent to C => A is not equivalent to C"
                )
            }
        };

    [Theory]
    [MemberData(nameof(IsEquivalentToCases))]
    public void IsEquivalent_should_behave_as_expected(DurationExpression duration, FilterExpression other, FilterExpression third, (bool expected, string reason) expectation)
    {
        // Arrange
        outputHelper.WriteLine($"Duration : {duration}");
        outputHelper.WriteLine($"Other : {other.EscapedParseableString}");
        outputHelper.WriteLine($"Third : {third.EscapedParseableString}");

        bool durationIsEquivalentToOther = duration.IsEquivalentTo(other);
        bool otherIsEquivalentToThird = other.IsEquivalentTo(third);

        outputHelper.WriteLine($"{duration} is equivalent to {other} : {durationIsEquivalentToOther}");
        outputHelper.WriteLine($"{other} is equivalent to {third} : {otherIsEquivalentToThird}");

        // Act
        bool actual = duration.IsEquivalentTo(third);

        // Assert
        actual.Should().Be(expectation.expected, expectation.reason);
    }
}