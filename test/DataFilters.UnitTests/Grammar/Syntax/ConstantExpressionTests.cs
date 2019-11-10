﻿using DataFilters.Grammar.Syntax;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace DataFilters.UnitTests.Grammar.Syntax
{
    public class ConstantExpressionTests
    {
        [Fact]
        public void IsFilterExpression() => typeof(ConstantExpression).Should()
            .BeAssignableTo<FilterExpression>().And
            .Implement<IEquatable<ConstantExpression>>().And
            .HaveConstructor(new[] { typeof(string) }).And
            .HaveProperty<string>("Value");

        [Fact]
        public void Ctor_Throws_ArgumentNullException_When_Argument_Is_Null()
        {
            // Act
            Action action = () => new ConstantExpression(null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>($"The parameter of  {nameof(ConstantExpression)}'s constructor cannot be null");
        }

        [Fact]
        public void Ctor_Throws_ArgumentOutyException_When_Argument_Is_Empty()
        {
            // Act
            Action action = () => new ConstantExpression(string.Empty);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentOutOfRangeException>($"The parameter of  {nameof(ConstantExpression)}'s constructor cannot be empty");
        }

        [Fact]
        public void Ctor_Throws_ArgumentOutyException_When_Argument_Is_Whitespace()
        {
            // Act
            Action action = () => new ConstantExpression(" ");

            // Assert
            action.Should()
                .NotThrow($"The parameter of  {nameof(ConstantExpression)}'s constructor can be whitespace");
        }

        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                yield return new object[]
                {
                    new ConstantExpression("prop1"),
                    new ConstantExpression("prop1"),
                    true,
                    "comparing two different instances with same property name"
                };

                yield return new object[]
                {
                    new ConstantExpression("prop1"),
                    new ConstantExpression("prop2"),
                    false,
                    "comparing two different instances with different property name"
                };
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void ImplementsEqualsCorrectly(ConstantExpression first, object other, bool expected, string reason)
        {
            // Act
            bool actual = first.Equals(other);

            // Assert
            actual.Should()
                .Be(expected, reason);
        }
    }
}