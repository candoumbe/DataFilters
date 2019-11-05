using DataFilters.Grammar.Syntax;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace DataFilters.UnitTests.Grammar.Syntax
{
    public class EndsWithExpressionTests
    {
        [Fact]
        public void IsFilterExpression() => typeof(EndsWithExpression).Should()
                                                                          .BeAssignableTo<FilterExpression>().And
                                                .Implement<IEquatable<EndsWithExpression>>().And
                                                                          .HaveConstructor(new[] { typeof(string) }).And
            .HaveProperty<string>("Value");

        [Fact]
        public void Ctor_Throws_ArgumentNullException_When_Argument_Is_Null()
        {
            // Act
            Action action = () => new EndsWithExpression(null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>($"The value of {nameof(EndsWithExpression)}'s constructor cannot be null");
        }

        [Fact]
        public void Ctor_Throws_ArgumentOutOfRangeException_When_Argument_Is_Empty()
        {
            // Act
            Action action = () => new EndsWithExpression(string.Empty);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentOutOfRangeException>($"The value of {nameof(EndsWithExpression)}'s constructor cannot be empty");
        }

        [Fact]
        public void Ctor_DoesNot_Throws_ArgumentOutOfRangeException_When_Argument_Is_WhitespaceOnly()
        {
            // Act
            Action action = () => new EndsWithExpression("  ");

            // Assert
            action.Should()
                .NotThrow($"The value of {nameof(EndsWithExpression)}'s constructor can be whitespace only");
        }

        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                yield return new object[]
                {
                    new EndsWithExpression("prop1"),
                    new EndsWithExpression("prop1"),
                    true,
                    "comparing two different instances with same property name"
                };

                yield return new object[]
                {
                    new EndsWithExpression("prop1"),
                    new EndsWithExpression("prop2"),
                    false,
                    "comparing two different instances with different property name"
                };
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void ImplementsEqualsCorrectly(EndsWithExpression first, object other, bool expected, string reason)
        {
            // Act
            bool actual = first.Equals(other);

            // Assert
            actual.Should()
                .Be(expected, reason);
        }
    }
}
