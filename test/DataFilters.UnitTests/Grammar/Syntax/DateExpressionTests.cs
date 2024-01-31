namespace DataFilters.UnitTests.Grammar.Syntax
{
    using System;
    using DataFilters.Grammar.Syntax;
    using DataFilters.UnitTests.Helpers;
    using FluentAssertions;
    using FsCheck;
    using FsCheck.Fluent;
    using FsCheck.Xunit;
    using Xunit;
    using Xunit.Abstractions;

    public class DateExpressionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public DateExpressionTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        [Fact]
        public void IsFilterExpression() => typeof(DateExpression).Should()
                                            .BeAssignableTo<FilterExpression>().And
                                            .Implement<IEquatable<DateExpression>>().And
                                            .Implement<IBoundaryExpression>();

        [Property]
        public void Given_DateExpression_instance_instance_eq_instance_should_be_true(PositiveInt year, PositiveInt month, PositiveInt day)
        {
            // Arrange
            DateExpression instance = new(year.Item, month.Item, day.Item);

            // Act
            bool actual = instance.Equals(instance);

            // Assert
            actual.Should()
                  .BeTrue();
        }

        [Property]
        public void Given_DateExpression_instance_instance_eq_null_should_be_false(PositiveInt year, PositiveInt month, PositiveInt day)
        {
            // Arrange
            DateExpression instance = new(year.Item, month.Item, day.Item);

            // Act
            bool actual = instance.Equals(null);

            // Assert
            actual.Should()
                  .BeFalse();
        }

        [Property]
        public void Given_two_DateExpression_instances_first_and_other_with_same_data_first_eq_other_should_be_true(PositiveInt year, PositiveInt month, PositiveInt day)
        {
            // Arrange
            DateExpression first = new(year.Item, month.Item, day.Item);
            DateExpression other = new(year.Item, month.Item, day.Item);

            // Act
            bool actual = first.Equals(other);
            int firstHashCode = first.GetHashCode();
            int otherHashCode = other.GetHashCode();

            // Assert
            actual.Should().BeTrue();
            firstHashCode.Should().Be(otherHashCode);
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Given_DateExpression_GetComplexity_should_return_1(NonNull<DateExpression> date)
            => date.Item.Complexity.Should().Be(1);

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Equals_should_be_commutative(NonNull<DateExpression> first, FilterExpression second)
            => (first.Item.Equals(second) == second.Equals(first.Item)).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Equals_should_be_reflexive(NonNull<DateExpression> expression)
            => expression.Item.Should().Be(expression.Item);

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Equals_should_be_symetric(NonNull<DateExpression> expression, NonNull<FilterExpression> otherExpression)
            => expression.Item.Equals(otherExpression.Item).Should().Be(otherExpression.Item.Equals(expression.Item));

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void IsEquivalent_should_be_commutative(NonNull<DateExpression> first, FilterExpression second)
            => first.Item.IsEquivalentTo(second).Should().Be(second.IsEquivalentTo(first.Item));

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void IsEquivalentTo_should_be_reflexive(NonNull<DateExpression> expression)
            => expression.Item.IsEquivalentTo(expression.Item).Should().BeTrue();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void IsEquivalentTo_should_be_symetric(NonNull<DateExpression> expression, NonNull<FilterExpression> otherExpression)
            => expression.Item.IsEquivalentTo(otherExpression.Item).Should().Be(otherExpression.Item.IsEquivalentTo(expression.Item));

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Given_left_and_right_operands_Equal_operator_should_return_same_result_as_using_Equals(NonNull<DateExpression> left, NonNull<DateExpression> right)
            => (left.Item == right.Item).Should().Be(left.Item.Equals(right.Item));

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Given_left_and_right_operands_NotEquals_operator_should_return_opposite_result_of_Equals(NonNull<DateExpression> left, NonNull<DateExpression> right)
            => (left.Item != right.Item).Should().Be(!left.Item.Equals(right.Item));

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Given_years_month_and_days_When_either_years_or_months_or_days_is_less_than_1_Then_constructor_should_throw_ArgumentOutOfRangeException(int years, int months, int days)
        {
            // Act
            Action invokingConstructor = () => new DateExpression(years, months, days);

            // Assert
            object _ = (years, months, days) switch
            {
                (int y, int m, int d) when y < 1 || m < 1 || d < 1 => invokingConstructor.Should().Throw<ArgumentOutOfRangeException>("because years, months and days must be greater than or equal to 1"),
                _ => invokingConstructor.Should().NotThrow("because years, months and days are greater than or equal to 1")
            };
        }
    }
}
