using DataFilters.Grammar.Syntax;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace DataFilters.UnitTests.Grammar.Syntax
{
    public class EqualExpressionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public EqualExpressionTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        [Fact]
        public void IsFilterExpression() => typeof(EqualExpression).Should()
                                                                   .BeAssignableTo<FilterExpression>().And
                                                                   .Implement<IEquatable<EqualExpression>>().And
                                                                   .HaveDefaultConstructor();

        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                yield return new object[] { new EqualExpression(), null, false, "Comparing to null" };
                yield return new object[] { new EqualExpression(), new EqualExpression(), true, "Comparing to null" };
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void TestEquals(EqualExpression first, object other, bool expected, string reason)
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

        public static IEnumerable<object[]> EquivalentToCases
        {
            get
            {
                yield return new object[] { new EqualExpression(), null, false, "Comparing to null" };
                yield return new object[] { new EqualExpression(), new EqualExpression(), true, "Comparing to null" };
            }
        }

        [Theory]
        [MemberData(nameof(EquivalentToCases))]
        public void EquivalentTo(EqualExpression first, EqualExpression other, bool expected, string reason)
        {
            // Act
            bool actual = first.IsEquivalentTo(other);

            // Assert
            actual.Should()
                  .Be(expected, reason);
        }
    }
}
