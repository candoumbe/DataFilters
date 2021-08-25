namespace DataFilters.UnitTests.Grammar.Syntax
{
    using DataFilters.Grammar.Syntax;
    using DataFilters.UnitTests.Helpers;

    using FluentAssertions;

    using FsCheck;
    using FsCheck.Fluent;
    using FsCheck.Xunit;

    using System;
    using System.Collections.Generic;

    using Xunit;
    using Xunit.Abstractions;

    public class NotExpressionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public NotExpressionTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void IsFilterExpression() => typeof(NotExpression).Should()
                                                                 .BeAssignableTo<FilterExpression>().And
                                                                 .Implement<IEquatable<NotExpression>>().And
                                                                 .HaveConstructor(new[] { typeof(FilterExpression) }).And
                                                                 .HaveProperty<FilterExpression>("Expression");

        [Fact]
        public void Ctor_should_throws_ArgumentNullException_when_argument_is_null()
        {
            // Act
            Action action = () => new NotExpression(null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>($"The parameter of  {nameof(NotExpression)}'s constructor cannot be null");
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_NotExpression_GetComplexity_should_return_same_complexity_as_embedded_expression(NonNull<NotExpression> notExpression)
            => (notExpression.Item.Complexity == notExpression.Item.Expression.Complexity).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_NotExpression_Equals_should_be_reflexive(NonNull<NotExpression> notExpression)
            => notExpression.Item.Equals(notExpression.Item).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_NotExpression_and_a_filter_expression_that_is_not_null_Equals_should_be_symetric(NonNull<NotExpression> notExpression, NonNull<FilterExpression> filterExpression)
            => (notExpression.Item.Equals(filterExpression.Item) == filterExpression.Item.Equals(notExpression.Item)).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_two_instances_holding_same_Expressions_Equals_should_return_true(NonNull<FilterExpression> expression)
        {
            // Arrange
            NotExpression first = new(expression.Item);
            NotExpression other = new(expression.Item);

            // Act
            return first.Equals(other)
                        .And(first.GetHashCode() == other.GetHashCode());
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Given_argument_is_AndExpression_Constructor_should_wrap_it_inside_a_GroupExpression(NonNull<AndExpression> expression)
            => Given_argument_needs_wrapping_Constructor_should_wrap_it_inside_a_GroupExpression(expression.Item);

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Given_argument_is_OrExpression_Constructor_should_wrap_it_inside_a_GroupExpression(NonNull<OrExpression> expression)
            => Given_argument_needs_wrapping_Constructor_should_wrap_it_inside_a_GroupExpression(expression.Item);

        private static void Given_argument_needs_wrapping_Constructor_should_wrap_it_inside_a_GroupExpression(FilterExpression expression)
        {
            // Act
            NotExpression not = new(expression);

            // Assert
            not.Expression.Should()
                          .BeOfType<GroupExpression>().Which
                          .IsEquivalentTo(expression).Should().BeTrue();
        }
    }
}
