﻿using DataFilters.Grammar.Syntax;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace DataFilters.UnitTests.Grammar.Syntax
{
    public class ConstantExpressionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public ConstantExpressionTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        [Fact]
        public void IsFilterExpression() => typeof(ConstantValueExpression).Should()
            .BeAssignableTo<FilterExpression>().And
            .Implement<IEquatable<ConstantValueExpression>>().And
            .HaveConstructor(new[] { typeof(string) }).And
            .HaveProperty<string>("Value");

        [Fact]
        public void Ctor_Throws_ArgumentNullException_When_Argument_Is_Null()
        {
            // Act
            Action action = () => new ConstantValueExpression(null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>($"The parameter of  {nameof(ConstantValueExpression)}'s constructor cannot be null");
        }

        [Fact]
        public void Ctor_Throws_ArgumentOutyException_When_Argument_Is_Empty()
        {
            // Act
            Action action = () => new ConstantValueExpression(string.Empty);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentOutOfRangeException>($"The parameter of  {nameof(ConstantValueExpression)}'s constructor cannot be empty");
        }

        [Fact]
        public void Ctor_Throws_ArgumentOutyException_When_Argument_Is_Whitespace()
        {
            // Act
            Action action = () => new ConstantValueExpression(" ");

            // Assert
            action.Should()
                .NotThrow($"The parameter of  {nameof(ConstantValueExpression)}'s constructor can be whitespace");
        }

        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                yield return new object[]
                {
                    new ConstantValueExpression("prop1"),
                    new ConstantValueExpression("prop1"),
                    true,
                    "comparing two different instances with same property name"
                };

                yield return new object[]
                {
                    new ConstantValueExpression("prop1"),
                    new ConstantValueExpression("prop2"),
                    false,
                    "comparing two different instances with different property name"
                };
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void ImplementsEqualsCorrectly(ConstantValueExpression first, object other, bool expected, string reason)
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
