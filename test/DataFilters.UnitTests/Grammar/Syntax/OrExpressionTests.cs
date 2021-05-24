using DataFilters.Grammar.Syntax;
using DataFilters.UnitTests.Helpers;

using FluentAssertions;

using FsCheck;
using FsCheck.Xunit;

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace DataFilters.UnitTests.Grammar.Syntax
{
    [UnitTest]
    public class OrExpressionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public OrExpressionTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void IsFilterExpression() => typeof(OrExpression).Should()
            .BeAssignableTo<FilterExpression>().And
            .Implement<IEquatable<OrExpression>>().And
            .HaveConstructor(new[] { typeof(FilterExpression), typeof(FilterExpression) }).And
            .HaveProperty<FilterExpression>("Left").And
            .HaveProperty<FilterExpression>("Right");

        public static IEnumerable<object[]> ArgumentNullExceptionCases
        {
            get
            {
                FilterExpression[] left = { new StartsWithExpression("ce"), null };
                FilterExpression[] right = { new StartsWithExpression("ce"), null };

                return left.CrossJoin(left, (left, right) => (left, right))
                    .Where(tuple => tuple.left == null || tuple.right is null)
                    .Select(tuple => new object[] { tuple.left, tuple.right });
            }
        }

        [Theory]
        [MemberData(nameof(ArgumentNullExceptionCases))]
        public void Ctor_Throws_ArgumentNullException_When_Argument_Is_Null(FilterExpression left, FilterExpression right)
        {
            // Act
            Action action = () => new OrExpression(left, right);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>("The parameter of the constructor cannot be null");
        }

        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                yield return new object[]
                {
                    new OrExpression(new StartsWithExpression("prop1"), new StartsWithExpression("prop2")),
                    new OrExpression(new StartsWithExpression("prop1"), new StartsWithExpression("prop2")),
                    true,
                    "comparing two different instances with same property name"
                };

                yield return new object[]
                {
                    new OrExpression(new StartsWithExpression("prop1"), new StartsWithExpression("prop2")),
                    new OrExpression(new StartsWithExpression("prop1"), new StartsWithExpression("prop3")),
                    false,
                    "comparing two different instances with different property name"
                };
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void ImplementsEqualsCorrectly(OrExpression first, object other, bool expected, string reason)
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

        public static IEnumerable<object[]> IsEquivalentToCases
        {
            get
            {
                yield return new object[]
                {
                    new OrExpression(new StartsWithExpression("prop1"), new StartsWithExpression("prop2")),
                    new OrExpression(new StartsWithExpression("prop1"), new StartsWithExpression("prop2")),
                    true,
                    $"both {nameof(OrExpression)} instances are identical"
                };

                yield return new object[]
                {
                    new OrExpression(new StartsWithExpression("prop1"), new StartsWithExpression("prop2")),
                    new OrExpression(new StartsWithExpression("prop1"), new StartsWithExpression("prop3")),
                    false,
                    $"the two {nameof(OrExpression)} instances contain different data."
                };

                yield return new object[]
                {
                    new OrExpression(new StartsWithExpression("prop1"), new StartsWithExpression("prop2")),
                    new OrExpression(new StartsWithExpression("prop2"), new StartsWithExpression("prop1")),
                    true,
                    $"both {nameof(OrExpression)} contains same data but not in the same order"
                };
            }
        }

        [Theory]
        [MemberData(nameof(IsEquivalentToCases))]
        public void Implements_IsEquivalentTo_Correctly(OrExpression first, FilterExpression other, bool expected, string reason)
        {
            _outputHelper.WriteLine($"First instance : {first}");
            _outputHelper.WriteLine($"Second instance : {other}");

            // Act
            bool actual = first.IsEquivalentTo(other);

            // Assert
            actual.Should()
                .Be(expected, reason);
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_ConstantExpression_equals_left_and_left_equals_right_expression_IsEquivalentTo_should_be_true(ConstantValueExpression filterExpression)
            => Given_FilterExpression_equals_left_and_right_IsEquivalentTo_should_return_true(filterExpression);

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_DateExpression_equals_left_and_left_equals_right_expression_IsEquivalentTo_should_be_true(DateExpression filterExpression)
            => Given_FilterExpression_equals_left_and_right_IsEquivalentTo_should_return_true(filterExpression);

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_DateTimeExpression_equals_left_and_left_equals_right_expression_IsEquivalentTo_should_be_true(DateTimeExpression filterExpression)
            => Given_FilterExpression_equals_left_and_right_IsEquivalentTo_should_return_true(filterExpression);

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_AsteriskExpression_equals_left_and_left_equals_right_expression_IsEquivalentTo_should_be_true(AsteriskExpression filterExpression)
            => Given_FilterExpression_equals_left_and_right_IsEquivalentTo_should_return_true(filterExpression);

        private static Property Given_FilterExpression_equals_left_and_right_IsEquivalentTo_should_return_true(FilterExpression filterExpression)
        {
            // Arrange
            OrExpression orExpression = new(filterExpression, filterExpression);

            // Act
            return orExpression.IsEquivalentTo(filterExpression)
                               .ToProperty();
        }
    }
}
