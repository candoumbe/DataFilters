
namespace DataFilters.UnitTests.Grammar.Syntax
{
    using DataFilters.Grammar.Syntax;
    using DataFilters.UnitTests.Helpers;

    using FluentAssertions;

    using FsCheck;
    using FsCheck.Fluent;
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
                    "operands are the same and in the same order"
                };

                yield return new object[]
                {
                    new AndExpression(new StartsWithExpression("prop1"), new StartsWithExpression("prop2")),
                    new AndExpression(new StartsWithExpression("prop1"), new StartsWithExpression("prop3")),
                    false,
                    "one operand is different"
                };

                yield return new object[]
                {
                    new AndExpression(new StartsWithExpression("prop1"), new StartsWithExpression("prop2")),
                    new AndExpression(new StartsWithExpression("prop2"), new StartsWithExpression("prop1")),
                    true,
                    "operands are the same but their order differs"
                };
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void Equals_should_work_as_expected(AndExpression first, object other, bool expected, string reason)
        {
            _outputHelper.WriteLine($"First instance : {first}");
            _outputHelper.WriteLine($"Second instance : {other}");

            // Act
            bool actual = first.Equals(other);

            // Assert
            actual.Should()
                  .Be(expected, reason);
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
                    new AndExpression(new StringValueExpression("val"), new StringValueExpression("val")),
                    new StringValueExpression("val")
                };

                yield return new object[]
                {
                    new AndExpression(new StringValueExpression("val"), new OrExpression(new StringValueExpression("val"), new StringValueExpression("val"))),
                    new StringValueExpression("val")
                };

                yield return new object[]
                {
                    new AndExpression(new OrExpression(new StringValueExpression("val"), new StringValueExpression("val")), new StringValueExpression("val")),
                    new StringValueExpression("val")
                };

                yield return new object[]
                {
                    new AndExpression(new IntervalExpression(new BoundaryExpression(new NumericValueExpression("-1"), true),
                                                                new BoundaryExpression(new NumericValueExpression("-1"), true)),
                                        new IntervalExpression(new BoundaryExpression(new NumericValueExpression("-1"), true),
                                                        new BoundaryExpression(new NumericValueExpression("-1"), true))),
                    new NumericValueExpression("-1"),
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

        [Property(Arbitrary = new[] {typeof(ExpressionsGenerators)})]
        public void Given_two_AndExpression_instances_one_and_two_where_oneU002Eleft_eq_twoU002Eright_and_oneU002Eright_eq_twoU002Eleft_IsEquivalentTo_should_return_true(FilterExpression first, FilterExpression second)
        {
            // Arrange
            AndExpression one = new(first, second);
            AndExpression two = new(second, first);

            // Act
            bool actual = one.IsEquivalentTo(two);

            // Assert
            actual.Should()
                  .BeTrue();
        }

        [Property(Arbitrary = new[] {typeof(ExpressionsGenerators)})]
        public void IsEquivalent_should_be_reflexive(AndExpression and)
            => and.IsEquivalentTo(and).Should().BeTrue();

        
        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_two_AndExpression_instances_first_and_second_and_firstU002ERight_eq_secondU002Eright_and_firstU002ELeft_eq_secondU002ELeft_Equals_should_returns_true(FilterExpression left, FilterExpression right)
        {
            // Arrange
            AndExpression first = new(left, right);
            AndExpression second = new(left, right);

            // Assert
            return first.Equals(second).ToProperty();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void An_AndExpression_Equals_should_neq_false(FilterExpression left, FilterExpression right)
        {
            // Arrange
            AndExpression first = new(left, right);

            // Act
            bool actual = first.Equals(null);

            // Assert
            actual.Should()
                  .BeFalse();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_AndExpression_instance_where_instanceU002ELeft_is_equivalent_to_instanceU002ERight_Simplify_should_return_the_expression_with_the_lowest_Complexity(FilterExpression expression, PositiveInt count)
        {
            // Arrange
            OneOfExpression oneOfExpression = new(Enumerable.Repeat(expression, count.Item + 1) // if count == 1
                                                            .ToArray());

            _outputHelper.WriteLine($"{nameof(oneOfExpression)} : '{oneOfExpression.EscapedParseableString}'");
            _outputHelper.WriteLine($"{nameof(oneOfExpression.Complexity)} : {oneOfExpression.Complexity}");

            AndExpression and = new(oneOfExpression, expression);

            // Act
            FilterExpression actual = and.Simplify();
            bool isEquivalent = actual.IsEquivalentTo(oneOfExpression);

            // Assert
            _outputHelper.WriteLine($"actual : {actual.EscapedParseableString} (Complexity : {actual.Complexity})");
            _outputHelper.WriteLine($"actual is equivalent to expression : {isEquivalent})");

            return (actual.Complexity < oneOfExpression.Complexity)
                .And(actual.IsEquivalentTo(expression));
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Given_left_operand_is_a_AndFilterExpression_instance_When_right_is_not_null_Constructor_should_wrap_left_inside_a_GroupExpression_instance(NonNull<AndExpression> left, NonNull<FilterExpression> right)
        {
            // Act
            AndExpression and = new (left.Item, right.Item);

            // Assert
            and.Left.Should()
                    .BeOfType<GroupExpression>($"Left instance is a '{nameof(AndExpression)}'");
        }
    }
}
