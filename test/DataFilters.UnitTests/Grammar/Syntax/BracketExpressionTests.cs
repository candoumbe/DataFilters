namespace DataFilters.UnitTests.Grammar.Syntax
{
    using System;
    using System.Linq;
    using DataFilters.Grammar.Syntax;
    using DataFilters.UnitTests.Helpers;
    using FluentAssertions;
    using FsCheck;
    using FsCheck.Fluent;
    using FsCheck.Xunit;
    using Xunit;
    using Xunit.Abstractions;

    public class BracketExpressionTests(ITestOutputHelper outputHelper)
    {
        [Fact]
        public void IsFilterExpression() => typeof(BracketExpression).Should()
            .BeAssignableTo<FilterExpression>().And
            .Implement<IEquatable<BracketExpression>>().And
            .Implement<IHaveComplexity>();

        [Fact]
        public void Ctor_Throws_ArgumentNullException_If_Value_Is_Null()
        {
            // Act
            Action action = () => new BracketExpression(null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>($"{nameof(BracketExpression)}.{nameof(BracketExpression.Values)} cannot be null");
        }

        public static TheoryData<BracketExpression, object, bool, string> EqualsCases
            => new()
            {
                {
                    new BracketExpression(new ConstantBracketValue("aBc")),
                    new BracketExpression(new ConstantBracketValue("aBc")),
                    true,
                    $"Two {nameof(BracketExpression)} instances built with inputs that are equals"
                },
                {
                    new BracketExpression(new ConstantBracketValue("aBc")),
                    new BracketExpression(new ConstantBracketValue("aBc")),
                    true,
                    $"Two {nameof(BracketExpression)} instances built with inputs that are equals"
                }
            };

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void Equals_should_behave_as_expected(BracketExpression expression, object obj, bool expected, string reason)
        {
            // Act
            bool actual = expression.Equals(obj);

            // Assert
            actual.Should()
                  .Be(expected, reason);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Two_BracketExpression_instances_built_with_different_inputs_should_not_be_equal(NonEmptyArray<BracketValue> one,
                                                                                                    NonEmptyArray<BracketValue> two)
        {
            // Arrange
            BracketExpression first = new(one.Item);
            BracketExpression second = new(two.Item);

            first.Equals(second).ToProperty().When(one.Item.Equals(two.Item)).VerboseCheck(outputHelper);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Complexity_should_depends_on_input(NonEmptyArray<BracketValue> values)
        {
            // Arrange
            BracketExpression bracketExpression = new(values.Item);
            double expected = values.Item.Select(value => value.Complexity)
                                         .Aggregate((initial, next) => initial * next);

            // Act
            double complexity = bracketExpression.Complexity;

            // Assert
            complexity.Should().Be(expected);
        }

        public static TheoryData<BracketExpression, double> ComplexityCases
            => new()
            {
                {
                    new BracketExpression
                    (
                        new ConstantBracketValue("aa"),
                        new RangeBracketValue('a', 'c')
                    ),
                    new ConstantBracketValue("aa").Complexity * new RangeBracketValue('a', 'c').Complexity
                }
            };

        [Theory]
        [MemberData(nameof(ComplexityCases))]
        public void Complexity_should_behave_as_expected(BracketExpression expression, double expected)
        {
            // Act
            double actual = expression.Complexity;

            // Assert
            actual.Should()
                  .Be(expected);
        }

        [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
        public void Given_BracketRangeValue_IsEquivalentTo_should_be_equivalent_to_many_OrExpression_where_each_expression_contains_one_charater(NonNull<RangeBracketValue> rangeBracketValue)
        {
            // Arrange
            BracketExpression rangeBracketExpression = new(rangeBracketValue.Item);

            OneOfExpression oneOf = new(Enumerable.Range(rangeBracketValue.Item.Start,
                                                          rangeBracketValue.Item.End - rangeBracketValue.Item.Start + 1)
                                                   .Select(ascii => new StringValueExpression(((char)ascii).ToString()))
                                                   .ToArray<FilterExpression>());

            // Act
            bool actual = rangeBracketExpression.IsEquivalentTo(oneOf);

            // Assert
            actual.Should().BeTrue($"Range expression : {rangeBracketValue.Item}");
        }
    }
}
