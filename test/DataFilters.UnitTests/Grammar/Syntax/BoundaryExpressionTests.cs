namespace DataFilters.UnitTests.Grammar.Syntax
{
    using System;
    using DataFilters.Grammar.Syntax;
    using DataFilters.UnitTests.Helpers;
    using FluentAssertions;
    using FsCheck;
    using FsCheck.Xunit;
    using Xunit;
    using Xunit.Categories;

    [UnitTest]
    [Feature(nameof(DataFilters.Grammar.Syntax))]
    [Feature(nameof(IntervalExpression))]
    public class BoundaryExpressionTests
    {
        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_a_BoundaryExpression_instance_should_be_equals_to_itself(BoundaryExpression input)
        {
            // Act
            bool actual = input.Equals(input);

            // Assert
            actual.Should().BeTrue("'equals' implementation should be reflexive");
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void A_BoundaryExpression_instance_should_not_be_equal_to_null(BoundaryExpression input)
        {
            // Act
            bool actual = input.Equals(null);

            // Assert
            actual.Should().BeFalse("a not null boundaryexpression is not equal to null");
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Two_BoundaryExpressions_instances_with_same_expression_and_included_should_be_equal(BoundaryExpression source)
        {
            // Arrange
            BoundaryExpression first = new(source.Expression, source.Included);
            BoundaryExpression other = new(source.Expression, source.Included);

            // Act
            bool actual = first.Equals(other);
            
            // Assert
            actual.Should().BeTrue();
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Ctor_should_throw_ArgumentNullException_when_expression_is_null(bool included)
        {
            // Act
            Action buildInstanceWithNullExpression = () => new BoundaryExpression(null, included);

            // Assert
            buildInstanceWithNullExpression.Should()
                                           .ThrowExactly<ArgumentNullException>();
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Equals_should_be_reflexive(NonNull<BoundaryExpression> expression)
        {
            // Act
            bool actual = expression.Item.Equals(expression.Item);

            // Assert
            actual.Should().BeTrue("'equals' implementation must be reflexive");
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Equals_should_be_symmetric(NonNull<BoundaryExpression> group, NonNull<FilterExpression> otherExpression)
        {
            // Act
            bool groupEqualOtherExpression = group.Item.Equals(otherExpression.Item);
            bool otherExpressionEqualGroup = otherExpression.Item.Equals(group);

            // Assert
            groupEqualOtherExpression.Should().Be(otherExpressionEqualGroup, "'equals' impelemntation must be symetric");
        }

        public static TheoryData<BoundaryExpression, object, bool, string> EqualsBehaviourCases
            => new()
            {
                {
                    new BoundaryExpression(new DateExpression(), true),
                    new BoundaryExpression(new DateExpression(), true),
                    true,
                    $"Two instances with {nameof(DateExpression)} that are equal"
                },
                {
                    new BoundaryExpression(new DateTimeExpression(new (), new (), new()), true),
                    new BoundaryExpression(new DateTimeExpression(new (), new (), new()), true),
                    true,
                    $"Two instances with {nameof(DateExpression)} that are equal"
                }
            };

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
