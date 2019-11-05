using DataFilters.Grammar.Syntax;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace DataFilters.UnitTests.Grammar.Syntax
{
    public class OneOfExpressionTests
    {
        [Fact]
        public void IsFilterExpression() => typeof(OneOfExpression).Should()
            .BeAssignableTo<FilterExpression>().And
            .Implement<IEquatable<OneOfExpression>>().And
            .HaveConstructor(new[] { typeof(FilterExpression[]) }).And
            .HaveProperty<IEnumerable<FilterExpression>>("Values");

        [Fact]
        public void Ctor_Throws_ArgumentNullException_When_Argument_Is_Null()
        {
            // Act
            Action action = () => new OneOfExpression(null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>($"The parameter {nameof(OneOfExpression)}'s constructor cannot be null");
        }

        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                yield return new object[]
                {
                    new OneOfExpression(new ConstantExpression("prop1"), new ConstantExpression("prop2")),
                    new OneOfExpression(new ConstantExpression("prop1"), new ConstantExpression("prop2")),
                    true,
                    "comparing two different instances with same data"
                };

                yield return new object[]
                {
                    new OneOfExpression(new ConstantExpression("prop1"), new ConstantExpression("prop2")),
                    new OneOfExpression(new ConstantExpression("prop2"), new ConstantExpression("prop1")),
                    true,
                    "comparing two different instances with same data but the order does not matter"
                };

                yield return new object[]
                {
                    new OneOfExpression(new ConstantExpression("prop1"), new ConstantExpression("prop2")),
                    new OneOfExpression(new ConstantExpression("prop1"), new ConstantExpression("prop3")),
                    false,
                    "comparing two different instances with different data"
                };
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void ImplementsEqualsCorrectly(OneOfExpression first, object other, bool expected, string reason)
        {
            // Act
            bool actual = first.Equals(other);

            // Assert
            actual.Should()
                .Be(expected, reason);
        }
    }
}
