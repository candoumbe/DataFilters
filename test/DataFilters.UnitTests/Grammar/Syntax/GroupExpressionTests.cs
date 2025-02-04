﻿namespace DataFilters.UnitTests.Grammar.Syntax
{
    using System;
    using DataFilters.Grammar.Syntax;
    using DataFilters.UnitTests.Helpers;
    using FluentAssertions;
    using FsCheck;
    using FsCheck.Xunit;
    using Xunit;
    using Xunit.Abstractions;

    public class GroupExpressionTests(ITestOutputHelper outputHelper)
    {
        [Fact]
        public void IsFilterExpression() => typeof(GroupExpression).Should()
                                                                   .BeAssignableTo<FilterExpression>().And
                                                                   .Implement<IEquatable<GroupExpression>>().And
                                                                   .Implement<IHaveComplexity>().And
                                                                   .Implement<IParseableString>();

        [Fact]
        public void Ctor_Throws_ArgumentNullException_When_Argument_Is_Null()
        {
            // Act
            Action action = () => _ = new GroupExpression(null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>($"The parameter of  {nameof(GroupExpression)}'s constructor cannot be null");
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_current_instance_is_not_null_and_other_is_null_Equals_should_return_false(NonNull<GroupExpression> group)
        {
            // Act
            bool actual = group.Item.Equals(null);

            // Assert
            actual.Should()
                  .BeFalse();
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Equals_should_be_reflexive(NonNull<GroupExpression> group)
            => group.Item.Equals(group.Item).Should().BeTrue();

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Equals_should_be_symmetric(NonNull<GroupExpression> group, NonNull<FilterExpression> otherExpression)
            => group.Item.Equals(otherExpression.Item).Should().Be(otherExpression.Item.Equals(group.Item));

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_GroupExpression_Complexity_should_be_linear_to_inner_expression_complexity(GroupExpression group) => group.Complexity.Should().Be(0.1 + group.Expression.Complexity);

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_GroupExpression_which_contains_an_arbitrary_expression_When_comparing_to_that_arbitrary_expression_IsEquivalent_should_return_true(NonNull<FilterExpression> filterExpression)
        {
            // Arrange
            GroupExpression group = new(filterExpression.Item);

            // Act
            bool actual = group.IsEquivalentTo(filterExpression.Item);

            // Assert
            actual.Should().BeTrue();
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_GroupExpression_which_contains_an_arbitrary_expression_When_comparing_to_that_arbitrary_expression_IsEquivalentTo_should_return_true(NonNull<FilterExpression> filterExpression)
        {
            // Arrange
            GroupExpression group = new(filterExpression.Item);

            // Act
            bool actual = group.IsEquivalentTo(filterExpression.Item);

            // Assert
            actual.Should().BeTrue();
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_GroupExpression_which_contains_an_arbitrary_expression_When_wrapping_that_group_inside_a_group_expression_Should_not_change_its_meaning(NonNull<GroupExpression> expression)
        {
            // Arrange
            GroupExpression group = new(expression.Item);

            // Act
            bool isEquivalent = group.IsEquivalentTo(expression.Item);

            // Assert
            isEquivalent.Should().BeTrue();
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_two_GroupExpression_instances_that_wrap_equivalent_expressions_IsEquivalent_should_be_true(NonNull<FilterExpression> filterExpression)
        {
            // Arrange
            GroupExpression first = new(filterExpression.Item);
            GroupExpression other = new(filterExpression.Item);

            // Act
            bool actual = first.IsEquivalentTo(other);

            // Assert
            actual.Should().BeTrue();
        }

        public static TheoryData<GroupExpression, object, bool, string> EqualsCases
            => new()
            {
                {
                    new GroupExpression(new StartsWithExpression("prop1")),
                    new GroupExpression(new StartsWithExpression("prop1")),
                    true,
                    "comparing two different instances with same property name"
                },
                {
                    new GroupExpression(new StartsWithExpression("prop1")),
                    new GroupExpression(new StartsWithExpression("prop2")),
                    false,
                    "comparing two different instances with different inner expressions"
                },
                {
                    new GroupExpression(new DateTimeExpression(new(2090, 10, 10), new (03,00,40, 583), OffsetExpression.Zero)),
                    new GroupExpression(new DateTimeExpression(new(2090, 10, 10), new (03,00,40, 583), OffsetExpression.Zero)),
                    true,
                    "Two instances with inner expressions that are equal"
                },
                {
                    new GroupExpression(new NumericValueExpression("0")),
                    new GroupExpression(new StringValueExpression("0")),
                    true,
                    "Two instances with inner expressions that are equal"
                }
            };

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void Equals_should_behave_as_expected(GroupExpression expression, object obj, bool expected, string reason)
        {
            // Act
            bool actual = expression.Equals(obj);

            // Assert
            actual.Should()
                  .Be(expected, reason);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_GroupExpression_wraps_an_arbitrary_FilterExpression_When_that_group_is_wrapped_inside_a_group_expression_Should_not_change_its_meaning(NonNull<FilterExpression> filterExpression)
        {
            // Arrange
            GroupExpression group = new(filterExpression.Item);
            GroupExpression extraGroup = new(group);

            // Act
            bool isEquivalent = group.IsEquivalentTo(extraGroup);

            // Assert
            isEquivalent.Should().BeTrue();
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_any_BinaryExpression_When_that_expression_is_wrapped_inside_a_group_expression_Should_not_change_its_meaning(NonNull<BinaryFilterExpression> filterExpression)
        {
            // Arrange
            GroupExpression group = new(filterExpression.Item);

            // Act
            bool isEquivalent = group.IsEquivalentTo(filterExpression.Item);

            // Assert
            isEquivalent.Should().BeTrue();
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void IsEquivalent_should_be_reflexive(GroupExpression group)
            => group.IsEquivalentTo(group).Should().BeTrue();

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void IsEquivalent_should_be_symmetric(GroupExpression group, NonNull<FilterExpression> other)
        {
            outputHelper.WriteLine($"Group : {group}");
            outputHelper.WriteLine($"Other : {other}");

            group.IsEquivalentTo(other.Item).Should().Be(other.Item.IsEquivalentTo(group));
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void IsEquivalent_should_be_transitive(GroupExpression group, NonNull<FilterExpression> other, NonNull<FilterExpression> third)
        {
            // Arrange
            outputHelper.WriteLine($"Group : {group}");
            outputHelper.WriteLine($"Other : {other.Item.EscapedParseableString}");
            outputHelper.WriteLine($"Third : {third.Item.EscapedParseableString}");

            bool first = group.IsEquivalentTo(other.Item);
            bool second = other.Item.IsEquivalentTo(third.Item);

            // Act
            bool actual = group.IsEquivalentTo(third.Item);

            // Assert
            if (first && second)
            {
                actual.Should().BeTrue();
            }
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)], Replay = "(13485810522331674394,9508666507375819251")]
        public void Given_a_non_null_filter_expression_When_wrapped_inside_a_group_expression_Should_not_change_its_meaning(NonNull<FilterExpression> filterExpressionGenerator, PositiveInt count)
        {
            // Arrange
            int depth = count.Item / 2;
            FilterExpression filterExpression = filterExpressionGenerator.Item;
            GroupExpression initialGroup = new(filterExpression);
            GroupExpression otherGroup = new(filterExpression);

            for (int i = 0; i < depth; i++)
            {
                otherGroup = new GroupExpression(otherGroup);
            }

            // Act
            bool isEquivalent = initialGroup.IsEquivalentTo(otherGroup);

            // Assert
            outputHelper.WriteLine($"{nameof(initialGroup)}: {initialGroup:d}");
            outputHelper.WriteLine($"{nameof(otherGroup)}: {otherGroup:d}");
            isEquivalent.Should().BeTrue();
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_a_two_groups_that_wraps_the_same_expression_Then_they_should_be_equal(NonNull<FilterExpression> filterExpressionGenerator, PositiveInt count)
        {
            // Arrange
            int depth = count.Item / 2;
            FilterExpression filterExpression = filterExpressionGenerator.Item;
            GroupExpression initialGroup = new(filterExpression);
            GroupExpression otherGroup = new(filterExpression);

            for (int i = 0; i < depth; i++)
            {
                initialGroup = new GroupExpression(initialGroup);
                otherGroup = new GroupExpression(otherGroup);
            }

            // Act
            bool isEquivalent = initialGroup.IsEquivalentTo(otherGroup);

            // Assert
            isEquivalent.Should().BeTrue();
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_a_non_null_FilterExpression_that_is_wrapped_inside_a_GroupExpression_instance_When_calling_Simplify_Then_the_resulting_FilterExpression_should_be_equivalent_to_the_starting_FilterExpression(NonNull<FilterExpression> filterExpressionGenerator)
        {
            // Arrange
            FilterExpression expected = filterExpressionGenerator.Item;
            GroupExpression group = new(expected);

            // Act
            FilterExpression actual = group.Simplify();

            // Assert
            actual.Should().NotBeOfType<GroupExpression>();
            actual.IsEquivalentTo(expected).Should().BeTrue();
        }

        public static TheoryData<GroupExpression, FilterExpression> SimplifyCases
            => new TheoryData<GroupExpression, FilterExpression>()
            {
                {
                    new GroupExpression(new StringValueExpression("prop")),
                    new StringValueExpression("prop")
                },
                {
                    new GroupExpression(new OrExpression(new StringValueExpression("prop"), new StringValueExpression("prop"))),
                    new StringValueExpression("prop")
                }
            };

        [Theory]
        [MemberData(nameof(SimplifyCases))]
        public void Given_GroupExpression_When_calling_Simplify_Then_should_return_expected_outcome(GroupExpression input, FilterExpression expected)
        {
            // Act
            FilterExpression actual = input.Simplify();
            
            // Assert
            actual.Should().Be(expected);
        }

        public static TheoryData<GroupExpression, FilterExpression, bool, string> IsEquivalentToCases
            => new()
            {
                {
                    new GroupExpression(new IntervalExpression(new BoundaryExpression(new NumericValueExpression("-1"), true), new BoundaryExpression(new NumericValueExpression("-1"), true))),
                    new GroupExpression(new IntervalExpression(new BoundaryExpression(new NumericValueExpression("-1"), true), new BoundaryExpression(new NumericValueExpression("-1"), true))),
                    true,
                    "left and right are expressions with exactly same values"
                }
            };
        
        [Theory]
        [MemberData(nameof(IsEquivalentToCases))]
        public void Given_left_is_a_GroupExpression_and_right_is_an_expression_Then_IsEquivalent_should_returns_expected_result(GroupExpression left, FilterExpression right, bool expected, string reason)
        {
            // Act
            bool actual = left.IsEquivalentTo(right);
 
            // Assert
            actual.Should().Be(expected, reason);
        }
    }
}
