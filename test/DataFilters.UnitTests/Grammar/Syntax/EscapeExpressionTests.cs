using DataFilters.Grammar.Syntax;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace DataFilters.UnitTests.Grammar.Syntax
{
    public class EscapeExpressionTests
    {
        [Fact]
        public void IsFilterExpression() => typeof(EscapeExpression).Should()
            .BeAssignableTo<FilterExpression>().And
            .Implement<IEquatable<EscapeExpression>>().And
            .HaveConstructor(new[] { typeof(char) }).And
            .HaveProperty<FilterExpression>("Value");

        [Fact]
        public void Ctor_Throws_ArgumentNullException_When_Argument_Is_Null()
        {
            // Act
            Action action = () => new EscapeExpression(default);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentOutOfRangeException>($"The parameter of  {nameof(EscapeExpression)}'s constructor cannot be default(char)");
        }

        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                yield return new object[]
                {
                    new EscapeExpression('\\'),
                    new EscapeExpression('\\'),
                    true,
                    "comparing two different instances with same value"
                };

                yield return new object[]
                {
                    new EscapeExpression('*'),
                    new EscapeExpression('\\'),
                    false,
                    "comparing two different instances with different values"
                };

                yield return new object[]
                {
                    new EscapeExpression('*'),
                    null,
                    false,
                    "comparing to null should return false"
                };
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void ImplementsEqualsCorrectly(EscapeExpression first, object other, bool expected, string reason)
        {
            // Act
            bool actual = first.Equals(other);

            // Assert
            actual.Should()
                .Be(expected, reason);
        }
    }
}
