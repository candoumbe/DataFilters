using DataFilters.Grammar.Syntax;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace DataFilters.UnitTests.Grammar.Syntax
{
    public class PropertyNameExpressionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public PropertyNameExpressionTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void IsFilterExpression() => typeof(PropertyNameExpression).Should()
            .BeAssignableTo<FilterExpression>().And
            .Implement<IEquatable<PropertyNameExpression>>().And
            .HaveConstructor(new[] { typeof(string) }).And
            .HaveProperty<string>("Name");

        [Fact]
        public void Ctor_Throws_ArgumentNullException_When_Argument_Is_Null()
        {
            // Act
            Action action = () => new PropertyNameExpression(null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>("The parameter of the constructor cannot be null");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Ctor_Throws_ArgumentOutyException_When_Argument_Is_Null(string name)
        {
            // Act
            Action action = () => new PropertyNameExpression(name);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentOutOfRangeException>("The parameter of the constructor cannot be empty or whitespace only");
        }

        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                yield return new object[]
                {
                    new PropertyNameExpression("prop1"),
                    new PropertyNameExpression("prop1"),
                    true,
                    "comparing two different instances with same property name"
                };

                yield return new object[]
                {
                    new PropertyNameExpression("prop1"),
                    new PropertyNameExpression("prop2"),
                    false,
                    "comparing two different instances with different property name"
                };
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void ImplementsEqualsCorrectly(PropertyNameExpression first, object other, bool expected, string reason)
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
    }
}
