using DataFilters.Grammar.Syntax;
using DataFilters.UnitTests.Helpers;

using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;

using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace DataFilters.UnitTests.Grammar.Syntax
{
    public class NotExpressionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public NotExpressionTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

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
                    new NotExpression(new ConstantValueExpression("prop1")),
                    new NotExpression(new ConstantValueExpression("prop1")),
                    true,
                    "comparing two different instances with same expression"
                };

                yield return new object[]
                {
                    new NotExpression(new ConstantValueExpression("prop1")),
                    new NotExpression(new ConstantValueExpression("prop2")),
                    false,
                    "comparing two different instances with different property name"
                };
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void ImplementsEqualsCorrectly(NotExpression first, object other, bool expected, string reason)
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

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators)})]
        public Property Given_NotExpression_GetComplexity_should_return_same_complexity_as_embedded_expression(NotExpression notExpression)
            => (notExpression.Complexity == notExpression.Expression.Complexity).ToProperty();
    }
}
