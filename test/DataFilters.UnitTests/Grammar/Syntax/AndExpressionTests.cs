﻿
namespace DataFilters.UnitTests.Grammar.Syntax
{
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

    [UnitTest]
    [Feature(nameof(AndExpression))]
    public class AndExpressionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public AndExpressionTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        [Fact]
        public void IsFilterExpression() => typeof(AndExpression).Should()
                                                                 .BeAssignableTo<FilterExpression>().And
                                                                 .Implement<IEquatable<AndExpression>>().And
                                                                 .Implement<IHaveComplexity>().And
                                                                 .HaveConstructor(new[] { typeof(FilterExpression), typeof(FilterExpression) }).And
                                                                 .HaveProperty<FilterExpression>("Left").And
                                                                 .HaveProperty<FilterExpression>("Right");

        public static IEnumerable<object[]> ArgumentNullExceptionCases
        {
            get
            {
                FilterExpression[] left = { new StartsWithExpression("ce"), null };

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
            Action action = () => new AndExpression(left, right);

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
                    new AndExpression(new StartsWithExpression("prop1"), new StartsWithExpression("prop2")),
                    new AndExpression(new StartsWithExpression("prop1"), new StartsWithExpression("prop2")),
                    true,
                    "comparing two different instances with same property name"
                };

                yield return new object[]
                {
                    new AndExpression(new StartsWithExpression("prop1"), new StartsWithExpression("prop2")),
                    new AndExpression(new StartsWithExpression("prop1"), new StartsWithExpression("prop3")),
                    false,
                    "comparing two different instances with different property name"
                };
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void ImplementsEqualsCorrectly(AndExpression first, object other, bool expected, string reason)
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

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_AndExpression_GetComplexity_should_return_left_complexity_multiply_by_right_complexity(AndExpression and)
            => (and.Complexity == and.Left.Complexity * and.Right.Complexity).ToProperty();

        public static IEnumerable<object[]> SimplifyCases
        {
            get
            {
                yield return new object[]
                {
                    new AndExpression(new ConstantValueExpression("val"), new ConstantValueExpression("val")),
                    new ConstantValueExpression("val")
                };

                yield return new object[]
                {
                    new AndExpression(new ConstantValueExpression("val"), new OrExpression(new ConstantValueExpression("val"), new ConstantValueExpression("val"))),
                    new ConstantValueExpression("val")
                };

                yield return new object[]
                {
                    new AndExpression(new OrExpression(new ConstantValueExpression("val"), new ConstantValueExpression("val")), new ConstantValueExpression("val")),
                    new ConstantValueExpression("val")
                };
            }
        }

        [Theory]
        [MemberData(nameof(SimplifyCases))]
        public void Given_AndExpression_Simplify_should_return_the_expected_expression(AndExpression andExpression, FilterExpression expected)
        {
            // Act
            FilterExpression actual = andExpression.Simplify();

            // Assert
            actual.Should()
                  .Be(expected);
        }
    }
}
