using DataFilters.Grammar.Syntax;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Categories;

namespace DataFilters.UnitTests.Grammar.Syntax
{
    [UnitTest]
    [Feature(nameof(AndExpression))]
    public class AndExpressionTests
    {
        [Fact]
        public void IsFilterExpression() => typeof(AndExpression).Should()
            .BeAssignableTo<FilterExpression>().And
            .Implement<IEquatable<AndExpression>>().And
            .HaveConstructor(new[] { typeof(FilterExpression), typeof(FilterExpression) }).And
            .HaveProperty<FilterExpression>("Left").And
            .HaveProperty<FilterExpression>("Right");

        public static IEnumerable<object[]> ArgumentNullExceptionCases
        {
            get
            {
                FilterExpression[] left = { new StartsWithExpression("ce"), null };
                FilterExpression[] right = { new StartsWithExpression("ce"), null };

                return left.CrossJoin(left, (left, right) => (left, right))
                    .Where(tuple => tuple.left == null || tuple.right is null)
                    .Select(tuple => new object[] { tuple.left, tuple.right });
            }
        }

        [Theory]
        [MemberData(nameof(ArgumentNullExceptionCases))]
        public void Ctor_Throws_ArgumentNullException_When_Argument_Is_Null(FilterExpression left, FilterExpression right)
        {
            // Act
            Action action = () => new AndExpression(left, right);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>("The parameter of the constructor cannot be null");
        }

        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                yield return new object[]
                {
                    new AndExpression(new StartsWithExpression("prop1"), new StartsWithExpression("prop2")),
                    new AndExpression(new StartsWithExpression("prop1"), new StartsWithExpression("prop2")),
                    true,
                    "comparing two different instances with same property name"
                };

                yield return new object[]
                {
                    new AndExpression(new StartsWithExpression("prop1"), new StartsWithExpression("prop2")),
                    new AndExpression(new StartsWithExpression("prop1"), new StartsWithExpression("prop3")),
                    false,
                    "comparing two different instances with different property name"
                };
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void ImplementsEqualsCorrectly(AndExpression first, object other, bool expected, string reason)
        {
            // Act
            bool actual = first.Equals(other);

            // Assert
            actual.Should()
                .Be(expected, reason);
        }
    }
}
