﻿namespace DataFilters.UnitTests.Grammar.Syntax
{
    using DataFilters.Grammar.Syntax;
    using FluentAssertions;
    using FsCheck.Xunit;
    using FsCheck;

    using System;
    using System.Collections.Generic;
    using Xunit;
    using Xunit.Abstractions;
    using DataFilters.UnitTests.Helpers;
    using FsCheck.Fluent;

    public class GroupExpressionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public GroupExpressionTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

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
            Action action = () => new GroupExpression(null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>($"The parameter of  {nameof(GroupExpression)}'s constructor cannot be null");
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Given_current_instance_is_not_null_and_other_is_null_Equals_should_return_false(NonNull<GroupExpression> group)
        {
            // Act
            bool actual = group.Item.Equals(null);

            // Assert
            actual.Should()
                  .BeFalse();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_reflexive(NonNull<GroupExpression> group)
            => group.Item.Equals(group.Item).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_symetric(NonNull<GroupExpression> group, NonNull<FilterExpression> otherExpression)
            => (group.Item.Equals(otherExpression.Item) == otherExpression.Item.Equals(group.Item)).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_GroupExpression_Complexity_should_be_equal_to_inner_expression_complexity(GroupExpression group)
            => (group.Complexity == group.Expression.Complexity).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_GroupExpression_which_contains_an_arbitrary_expression_When_comparing_to_that_arbitrary_expression_IsEquivalent_should_return_true(NonNull<FilterExpression> filterExpression)
        {
            // Arrange
            GroupExpression group = new(filterExpression.Item);

            // Act
            bool actual = group.IsEquivalentTo(filterExpression.Item);

            // Assert
            return actual.ToProperty();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_GroupExpression_which_contains_an_arbitrary_expression_When_comparing_to_that_arbitrary_expression_Equals_should_return_true(NonNull<FilterExpression> filterExpression)
        {
            // Arrange
            GroupExpression group = new(filterExpression.Item);

            // Act
            bool actual = group.Equals(filterExpression.Item);

            // Assert
            return actual.ToProperty();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_two_GroupExpression_instances_that_wrap_equivalent_expressions_IsEquivalent_should_be_true(NonNull<FilterExpression> filterExpression)
        {
            // Arrange
            GroupExpression first = new(filterExpression.Item);
            GroupExpression other = new(filterExpression.Item);

            // Act
            bool actual = first.IsEquivalentTo(other);

            // Assert
            return actual.ToProperty();
        }

        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                yield return new object[]
                {
                    new GroupExpression(new StartsWithExpression("prop1")),
                    new GroupExpression(new StartsWithExpression("prop1")),
                    true,
                    "comparing two different instances with same property name"
                };

                yield return new object[]
                {
                    new GroupExpression(new StartsWithExpression("prop1")),
                    new GroupExpression(new StartsWithExpression("prop2")),
                    false,
                    "comparing two different instances with different inner expressions"
                };

                yield return new object[]
                {
                    new GroupExpression(new DateTimeExpression(new(2090, 10, 10), new (03,00,40, 583), OffsetExpression.Zero)),
                    new GroupExpression(new DateTimeExpression(new(2090, 10, 10), new (03,00,40, 583), OffsetExpression.Zero)),
                    true,
                    "Two instances with inner expressions that are equal"
                };
            }
        }

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
    }
}
