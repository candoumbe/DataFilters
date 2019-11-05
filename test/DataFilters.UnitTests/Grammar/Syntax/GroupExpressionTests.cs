using DataFilters.Grammar.Syntax;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace DataFilters.UnitTests.Grammar.Syntax
{
    public class GroupExpressionTests
    {
        [Fact]
        public void IsFilterExpression() => typeof(GroupExpression).Should()
            .BeAssignableTo<FilterExpression>().And
            .Implement<IEquatable<GroupExpression>>().And
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
            // Act
            bool actual = first.Equals(other);

            // Assert
            actual.Should()
                .Be(expected, reason);
        }
    }
}
