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
    using Xunit.Categories;

    [UnitTest]
    [Feature(nameof(DataFilters.Grammar.Syntax))]
    [Feature(nameof(IntervalExpression))]
    public class BoundaryExpressionTests
    {
        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_a_BoundaryExpression_instance_should_be_equals_to_itself(BoundaryExpression input)
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

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_reflexive(NonNull<BoundaryExpression> expression)
            => expression.Item.Equals(expression.Item).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_symetric(NonNull<BoundaryExpression> group, NonNull<FilterExpression> otherExpression)
            => (group.Item.Equals(otherExpression.Item) == otherExpression.Item.Equals(group.Item)).ToProperty();

        public static IEnumerable<object[]> EqualsBehaviourCases
        {
            get
            {
                yield return new object[]
                {
                    new BoundaryExpression(new DateExpression(), true),
                    new BoundaryExpression(new DateExpression(), true),
                    true,
                    $"Two instances with {nameof(DateExpression)} that are equal"
                };

                yield return new object[]
                {
                    new BoundaryExpression(new DateTimeExpression(new (), new (), new()), true),
                    new BoundaryExpression(new DateTimeExpression(new (), new (), new()), true),
                    true,
                    $"Two instances with {nameof(DateExpression)} that are equal"
                };
            }
        }

        [Theory]
        [MemberData(nameof(EqualsBehaviourCases))]
        public void Equals_should_behave_as_expected(BoundaryExpression expression, object other, bool expected, string reason)
        {
            // Act
            bool actual = expression.Equals(other);

            // Assert
            actual.Should()
                  .Be(expected, reason);
        }

    }
}
