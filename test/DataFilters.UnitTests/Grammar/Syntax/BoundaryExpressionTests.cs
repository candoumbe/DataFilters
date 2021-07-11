namespace DataFilters.UnitTests.Grammar.Syntax
{
    using DataFilters.Grammar.Syntax;
    using DataFilters.UnitTests.Helpers;

    using FluentAssertions;

    using FsCheck;
    using FsCheck.Xunit;

    using System;
    using System.Collections.Generic;

    using Xunit;
    using Xunit.Categories;

    [UnitTest]
    [Feature(nameof(DataFilters.Grammar.Syntax))]
    [Feature(nameof(RangeExpression))]
    public class BoundaryExpressionTests
    {
        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                yield return new object[]
                {
                    new BoundaryExpression(new ConstantValueExpression("10"), included: true),
                    new BoundaryExpression(new ConstantValueExpression("10"), included: true),
                    true,
                    $"both {nameof(BoundaryExpression)}'s {nameof(BoundaryExpression.Expression)} and {nameof(BoundaryExpression.Included)} are equals"
                };

                {
                    BoundaryExpression instance = new(new ConstantValueExpression("10"), included: true);
                    yield return new object[]
                    {
                        instance,
                        instance,
                        true,
                        $"{nameof(BoundaryExpression)} instance is always equals to itself."
                    };
                }

                yield return new object[]
                {
                    new BoundaryExpression(new ConstantValueExpression("10"), included: true),
                    new BoundaryExpression(new ConstantValueExpression("10"), included: false),
                    false,
                    $"both {nameof(BoundaryExpression.Included)} are different"
                };

                yield return new object[]
                {
                    new BoundaryExpression(new ConstantValueExpression("10"), included: true),
                    new BoundaryExpression(new DateExpression(), included: true),
                    false,
                    $"both {nameof(BoundaryExpression.Expression)} are different"
                };

                yield return new object[]
                {
                    new BoundaryExpression(new ConstantValueExpression("10"), included: true),
                    null,
                    false,
                    "the argument is null"
                };
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void Override_Equals_properly(BoundaryExpression first, object second, bool expected, string reason)
        {
            // Act
            bool actual = first.Equals(second);

            // Asser
            actual.Should()
                  .Be(expected, reason);

            if (expected)
            {
                int actualHashcode = first.GetHashCode();
                actualHashcode.Should()
                              .Be(second?.GetHashCode(), reason);
            }
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Ctor_should_throw_ArgumentNullException_when_expression_is_null(bool included) => Prop.Throws<ArgumentNullException, BoundaryExpression>(new Lazy<BoundaryExpression>(() => new BoundaryExpression(null, included)));
    }
}
