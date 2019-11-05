using DataFilters.Grammar.Syntax;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace DataFilters.UnitTests.Grammar.Syntax
{
    public class NotExpressionTests
    {
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

        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                yield return new object[]
                {
                    new NotExpression(new ConstantExpression("prop1")),
                    new NotExpression(new ConstantExpression("prop1")),
                    true,
                    "comparing two different instances with same expression"
                };

                yield return new object[]
                {
                    new NotExpression(new ConstantExpression("prop1")),
                    new NotExpression(new ConstantExpression("prop2")),
                    false,
                    "comparing two different instances with different property name"
                };
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void ImplementsEqualsCorrectly(NotExpression first, object other, bool expected, string reason)
        {
            // Act
            bool actual = first.Equals(other);

            // Assert
            actual.Should()
                .Be(expected, reason);
        }
    }
}
