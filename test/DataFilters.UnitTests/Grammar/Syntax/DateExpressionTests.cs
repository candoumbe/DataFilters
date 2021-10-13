namespace DataFilters.UnitTests.Grammar.Syntax
{
    using DataFilters.Grammar.Syntax;
    using FluentAssertions;
    using FsCheck.Xunit;
    using FsCheck;

    using System;
    using Xunit;
    using Xunit.Abstractions;
    using DataFilters.UnitTests.Helpers;
    using FsCheck.Fluent;

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
        public Property Given_two_DateExpression_instances_first_and_other_with_same_data_first_eq_other_should_be_true(PositiveInt year, PositiveInt month, PositiveInt day)
        {
            // Arrange
            DateExpression first = new(year.Item, month.Item, day.Item);
            DateExpression other = new(year.Item, month.Item, day.Item);

            // Act
            bool actual = first.Equals(other);

            // Assert
            return actual.And(first.GetHashCode() == other.GetHashCode());
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_DateExpression_GetComplexity_should_return_1(DateExpression date) => (date.Complexity == 1).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_commutative(NonNull<TextExpression> first, FilterExpression second)
            => (first.Item.Equals(second) == second.Equals(first.Item)).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_reflexive(NonNull<TextExpression> expression)
            => expression.Item.Equals(expression.Item).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_symetric(NonNull<TextExpression> expression, NonNull<FilterExpression> otherExpression)
            => (expression.Item.Equals(otherExpression.Item) == otherExpression.Item.Equals(expression.Item)).ToProperty();

    }
}
