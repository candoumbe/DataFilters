namespace DataFilters.UnitTests.Grammar.Syntax
{
    using DataFilters.Grammar.Syntax;
    using DataFilters.UnitTests.Helpers;

    using FluentAssertions;

    using FsCheck;
    using FsCheck.Xunit;

    using System;

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
        public void Ctor_Throws_ArgumentNullException_When_Argument_Is_Null()
        {
            // Act
            Action action = () => new NotExpression(null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>($"The parameter of  {nameof(NotExpression)}'s constructor cannot be null");
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_NotExpression_GetComplexity_should_return_same_complexity_as_embedded_expression(NotExpression notExpression)
            => (notExpression.Complexity == notExpression.Expression.Complexity).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_NotExpression_Equals_should_return_true_when_comparing_to_itself(NotExpression notExpression)
            => notExpression.Equals(notExpression).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Two_instances_holding_same_Expressions_should_be_equal(FilterExpression expression)
        {
            // Arrange
            NotExpression first = new(expression);
            NotExpression other = new(expression);

            // Act
            Property actual = first.Equals(other)
                                   .And(first.GetHashCode() == other.GetHashCode());

            return actual;
        }
    }
}
