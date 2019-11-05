using DataFilters.Grammar.Syntax;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace DataFilters.UnitTests.Grammar.Syntax
{
    public class StartsWithExpressionTests
    {
        [Fact]
        public void IsFilterExpression() => typeof(StartsWithExpression).Should()
                                                                          .BeAssignableTo<FilterExpression>().And
                                                .Implement<IEquatable<StartsWithExpression>>().And
                                                                          .HaveConstructor(new[] { typeof(string) }).And
            .HaveProperty<string>("Value");

        [Fact]
        public void Ctor_Throws_ArgumentNullException_When_Argument_Is_Null()
        {
            // Act
            Action action = () => new StartsWithExpression(null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>("The parameter of the constructor cannot be null");
        }

        [Fact]
        public void Ctor_Throws_ArgumentOutOfRangeException_When_Argument_Is_Empty()
        {
            // Act
            Action action = () => new StartsWithExpression(string.Empty);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentOutOfRangeException>("The parameter of the constructor cannot be empty");
        }

        [Fact]
        public void Ctor_DoesNot_Throws_ArgumentOutOfRangeException_When_Argument_Is_WhitespaceOnly()
        {
            // Act
            Action action = () => new StartsWithExpression("  ");

            // Assert
            action.Should()
                .NotThrow<ArgumentOutOfRangeException>("The parameter of the constructor can be whitespace only");
            action.Should()
                .NotThrow("The parameter of the constructor can be whitespace only");
        }

        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                yield return new object[]
                {
                    new StartsWithExpression("prop1"),
                    new StartsWithExpression("prop1"),
                    true,
                    "comparing two different instances with same property name"
                };

                yield return new object[]
                {
                    new StartsWithExpression("prop1"),
                    new StartsWithExpression("prop2"),
                    false,
                    "comparing two different instances with different property name"
                };
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void ImplementsEqualsCorrectly(StartsWithExpression first, object other, bool expected, string reason)
        {
            // Act
            bool actual = first.Equals(other);

            // Assert
            actual.Should()
                .Be(expected, reason);
        }
    }
}
