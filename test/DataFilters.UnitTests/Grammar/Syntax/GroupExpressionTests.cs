using DataFilters.Grammar.Syntax;
using FluentAssertions;
using FsCheck.Xunit;
using FsCheck;

using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using DataFilters.UnitTests.Helpers;

namespace DataFilters.UnitTests.Grammar.Syntax
{
    public class GroupExpressionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public GroupExpressionTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        [Fact]
        public void IsFilterExpression() => typeof(GroupExpression).Should()
                                                                   .BeAssignableTo<FilterExpression>().And
                                                                   .Implement<IEquatable<GroupExpression>>().And
                                                                   .Implement<IHaveComplexity>().And
                                                                   .HaveConstructor(new[] { typeof(FilterExpression) }).And
                                                                   .HaveProperty<FilterExpression>("Expression");

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
            }
        }

        [Fact]
        public void Ctor_Throws_ArgumentNullException_When_Argument_Is_Null()
        {
            // Act
            Action action = () => new GroupExpression(null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>($"The parameter of  {nameof(GroupExpression)}'s constructor cannot be null");
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void ImplementsEqualsCorrectly(GroupExpression first, object other, bool expected, string reason)
        {
            _outputHelper.WriteLine($"First instance : {first}");
            _outputHelper.WriteLine($"Second instance : {other}");

            // Act
            bool actual = first.Equals(other);
            int actualHashCode = first.GetHashCode();

            // Assert
            actual.Should()
                .Be(expected, reason);
            if (expected)
            {
                actualHashCode.Should()
                    .Be(other?.GetHashCode(), reason);
            }
            else
            {
                actualHashCode.Should()
                    .NotBe(other?.GetHashCode(), reason);
            }
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_GroupExpression_Complexity_should_be_equal_to_inner_expression_complexity(GroupExpression group)
            => (group.Complexity == group.Expression.Complexity).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_Group_Expression_which_contains_an_arbitrary_expression_IsEquivalent_should_be_return_true_when_compares_to_that_arbitrary_expression(FilterExpression filterExpression)
        {
            // Arrange
            GroupExpression group = new(filterExpression);

            // Act
            bool actual = group.IsEquivalentTo(filterExpression);

            // Assert
            return actual.ToProperty();
        }
    }
}
