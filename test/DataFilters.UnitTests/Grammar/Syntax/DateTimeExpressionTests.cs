namespace DataFilters.UnitTests.Grammar.Syntax;

using System;
using DataFilters.Grammar.Syntax;
using DataFilters.UnitTests.Helpers;
using FluentAssertions;
using FluentAssertions.Extensions;
using FsCheck;
using FsCheck.Xunit;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

[Feature(nameof(DataFilters.Grammar.Syntax))]
public class DateTimeExpressionTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public void IsFilterExpression() => typeof(DateTimeExpression).Should()
        .BeAssignableTo<FilterExpression>().And
        .Implement<IEquatable<DateTimeExpression>>().And
        .HaveConstructor(new[] { typeof(DateExpression), typeof(TimeExpression), typeof(OffsetExpression) }).And
        .HaveConstructor(new[] { typeof(DateExpression), typeof(TimeExpression) }).And
        .HaveConstructor(new[] { typeof(TimeExpression) }).And
        .HaveConstructor(new[] { typeof(DateExpression) }).And
        .HaveProperty<DateExpression>("Date").And
        .HaveProperty<TimeExpression>("Time").And
        .HaveProperty<OffsetExpression>("Offset");

    [Fact]
    public void Given_date_and_time_parameters_are_null_Constructor_should_throws_ArgumentException()
    {
        // Act
        Action ctor = () => new DateTimeExpression(null, null);

        // Assert
        ctor.Should()
            .ThrowExactly<ArgumentException>("both date and time are null");
    }

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void A_non_null_DateTimeExpression_instance_should_never_be_equal_to_null(DateTimeExpression instance)
    {
        // Act
        bool actual = instance.Equals(null);

        // Assert
        actual.Should()
            .BeFalse();
    }

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Two_DateTimeExpression_instances_which_have_same_data_should_be_equal(DateExpression date, TimeExpression time, OffsetExpression offset)
    {
        // Arrange
        DateTimeExpression first = new(date, time, offset);
        DateTimeExpression other = new(date, time, offset);

        // Act
        bool actual = first.Equals(other);

        // Assert
        actual.Should()
            .BeTrue();
        first.GetHashCode().Should()
            .Be(other.GetHashCode(), "Two instances that are equal should have same hashcode");
    }

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void A_DateTimeExpression_built_from_variables_obtained_from_deconstructing_a_DateTimeExpression_should_equal_the_original_DateTimeExpression_value(DateTimeExpression source)
    {
        outputHelper.WriteLine(message: $"DateTimeExpression is {source}");
        (DateExpression date, TimeExpression time, OffsetExpression offset, _) = source;

        // Act
        DateTimeExpression clone = new(date, time, offset);

        // Assert
        clone.Should()
            .Be(source);
    }

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Given_DateTimeExpression_Complexity_Should_be_the_sum_of_complexity_of_each_member(NonNull<DateExpression> date, NonNull<TimeExpression> time, NonNull<OffsetExpression> offset)
    {
        // Arrange
        DateTimeExpression dateTimeExpression = new(date.Item, time.Item, offset.Item);
        double expected = date.Item.Complexity + time.Item.Complexity + offset.Item.Complexity;

        // Act
        double actual = dateTimeExpression.Complexity;

        // Assert
        actual.Should()
            .BeGreaterThanOrEqualTo(3).And
            .Be(expected);
    }

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Given_DateTimeExpression_where_only_date_part_is_set_When_comparing_to_DateExpression_IsEquivalentTo_should_be_true(NonNull<DateExpression> dateExpression)
    {
        // Arrange
        DateTimeExpression dateTimeExpression = new(dateExpression.Item);

        // Act
        bool actual = dateTimeExpression.IsEquivalentTo(dateExpression.Item);

        // Assert
        actual.Should().BeTrue();
    }

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Given_DateTimeExpression_where_only_time_part_is_set_When_comparing_to_that_TimeExpression(NonNull<TimeExpression> timeExpression)
    {
        // Arrange
        DateTimeExpression dateTimeExpression = new(timeExpression.Item);

        // Act
        bool actual = dateTimeExpression.IsEquivalentTo(timeExpression.Item);

        // Assert
        actual.Should().BeTrue();
        dateTimeExpression.Complexity.Should().BeGreaterThan(timeExpression.Item.Complexity);
    }

    public static TheoryData<DateTimeExpression, object, bool, string> EqualsCases
        => new()
        {
            {
                new DateTimeExpression(new TimeExpression()),
                new TimeExpression(),
                true,
                $"{nameof(DateTimeExpression.Date)} and {nameof(DateTimeExpression.Date)} are null and TimeExpression are equal"
            },
            {
                new DateTimeExpression(new (), new (), new()),
                new DateTimeExpression(new (), new (), new()),
                true,
                $"Two instances with {nameof(DateTimeExpression)} that are equal"
            },
            {
                new DateTimeExpression(new(2090, 10, 10), new (03,00,40, 583), OffsetExpression.Zero),
                new DateTimeExpression(new(2090, 10, 10), new (03,00,40, 583), OffsetExpression.Zero),
                true,
                $"Two instances with {nameof(DateTimeExpression.Date)}, {nameof(DateTimeExpression.Time)}, {nameof(DateTimeExpression.Offset)} are equal and not null"
            }
        };

    [Theory]
    [MemberData(nameof(EqualsCases))]
    public void Equals_should_work_as_expected(DateTimeExpression dateTime, object obj, bool expected, string reason)
    {
        // Act
        bool actual = dateTime.Equals(obj);

        // Assert
        actual.Should()
            .Be(expected, reason);
    }

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Equals_should_be_commutative(NonNull<DateTimeExpression> first, FilterExpression second)
        => first.Item.Equals(second).Should().Be(second.Equals(first.Item));

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Equals_should_be_reflexive(NonNull<DateTimeExpression> expression)
        => expression.Item.Should().Be(expression.Item);

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Equals_should_be_symetric(NonNull<DateTimeExpression> expression, NonNull<FilterExpression> otherExpression)
        => expression.Item.Equals(otherExpression.Item).Should().Be(otherExpression.Item.Equals(expression.Item));

    public static TheoryData<DateTimeExpression, object, object> EqualsTransitivityCases
        => new()
        {
            {
                new DateTimeExpression(1.January(2010)),
                new DateTimeExpression(1.January(2010)),
                new DateTimeExpression(1.January(2010))
            },
            {
                new DateTimeExpression(new TimeExpression(1)),
                new DateTimeExpression(new TimeExpression(minutes: 60)),
                new DateTimeExpression(new TimeExpression(seconds: 3600))
            },
            {
                new DateTimeExpression(new TimeExpression(1)),
                new DateTimeExpression(new TimeExpression(minutes: 60)),
                new DateTimeExpression(new TimeExpression(seconds: 3600))
            }
        };

    [Theory]
    [MemberData(nameof(EqualsTransitivityCases))]
    public void Equals_should_be_transitive(DateTimeExpression first, object second, object third)
    {
        // Arrange
        bool firstEqualsSecond = first.Equals(second);
        bool secondEqualsThird = second.Equals(third);

        // Act
        bool actual = first.Equals(third);

        // Assert
        actual.Should()
            .Be(firstEqualsSecond && secondEqualsThird);
    }

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Given_left_and_right_operands_Equal_operator_should_return_same_result_as_using_Equals(NonNull<DateTimeExpression> left, NonNull<DateTimeExpression> right)
    {
        // Act
        bool actual = left.Item == right.Item;

        // Assert
        actual.Should()
            .Be(left.Item.Equals(right.Item));
    }

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Given_left_and_right_operands_NotEqual_operator_should_return_opposite_result_of_Equal_operator(NonNull<DateTimeExpression> left, NonNull<DateTimeExpression> right)
    {
        // Act
        bool actual = left.Item != right.Item;

        // Assert
        actual.Should()
            .Be(!left.Item.Equals(right.Item));
    }
}