namespace DataFilters.UnitTests.Grammar.Syntax
{
    using DataFilters.Grammar.Syntax;
    using DataFilters.UnitTests.Helpers;

    using FluentAssertions;

    using FsCheck;
    using FsCheck.Fluent;
    using FsCheck.Xunit;

    using System;

    using Xunit.Categories;

    [UnitTest]
    [Feature(nameof(DataFilters.Grammar.Syntax))]
    [Feature(nameof(IntervalExpression))]
    public class BoundaryExpressionTests
    {
        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property A_BoundaryExpression_instance_should_be_equals_to_itself(BoundaryExpression input)
            => input.Equals(input).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property A_BoudnaryExpression_instance_should_not_be_equal_to_null(BoundaryExpression input)
            => (!input.Equals(null)).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Two_BoundaryExpression_instances_with_same_expression_and_included_should_be_equal(BoundaryExpression source)
        {
            // Arrange
            BoundaryExpression first = new(source.Expression, source.Included);
            BoundaryExpression other = new(source.Expression, source.Included);

            return first.Equals(other)
                .And(first.GetHashCode() == other.GetHashCode());
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Ctor_should_throw_ArgumentNullException_when_expression_is_null(bool included)
        {
            // Act
            Action buildInstanceWithNullExpression = () => new BoundaryExpression(null, included);

            // Assert
            buildInstanceWithNullExpression.Should()
                                           .ThrowExactly<ArgumentNullException>();
        }
    }
}
