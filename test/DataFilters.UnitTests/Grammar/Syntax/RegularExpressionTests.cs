using DataFilters.Grammar.Syntax;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace DataFilters.UnitTests.Grammar.Syntax
{
    public class RegularExpressionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public RegularExpressionTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void IsFilterExpression() => typeof(RegularExpression).Should()
            .BeAssignableTo<FilterExpression>().And
            .Implement<IEquatable<RegularExpression>>().And
            .HaveConstructor(new[] { typeof(string) }).And
            .HaveProperty<string>("Value")
            ;

        [Fact]
        public void Ctor_Throws_ArgumentNullException_If_Value_Is_Null()
        {
            // Act
            Action action = () => new RegularExpression(null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>($"{nameof(RegularExpression)}.{nameof(RegularExpression.Value)} cannot be null");
        }

        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                yield return new object[]
                {
                    new RegularExpression("prop2"),
                    new RegularExpression("prop2"),
                    true,
                    "comparing two different instances with same values"
                };

                yield return new object[]
                {
                    new RegularExpression("regex_pattern"),
                    new RegularExpression("another_regex_pattern"),
                    false,
                    "comparing two different instances with different values"
                };

                yield return new object[]
                {
                    new RegularExpression("regex_pattern"),
                    null,
                    false,
                    "comparing two different instances with different values"
                };
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void ImplementsEqualsCorrectly(RegularExpression first, object other, bool expected, string reason)
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
