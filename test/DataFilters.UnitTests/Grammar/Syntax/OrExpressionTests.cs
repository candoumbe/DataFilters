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
            .Implement<ISimplifiable>().And
            .HaveConstructor(new[] { typeof(FilterExpression), typeof(FilterExpression) }).And
            .HaveProperty<FilterExpression>("Left").And
            .HaveProperty<FilterExpression>("Right");

        public static IEnumerable<object[]> ArgumentNullExceptionCases
        {
            get
            {
                FilterExpression[] expression = { new StartsWithExpression("ce"), null };

                return expression.CrossJoin(expression, (left, right) => (left, right))
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
        public Property Given_two_OrExpression_instances_that_left_and_right_expressions_are_both_equals_IsEquivalentTo_should_return_true(FilterExpression left, FilterExpression right)
        {
            // Arrange
            OrExpression one = new(left, right);
            OrExpression two = new(left: right, right: left);

            return one.IsEquivalentTo(two).ToProperty();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators)}, Replay = "13802116455375353263,6490926112197589173")]
        public Property Given_OrExpression_and_OneOfExpression_that_contains_the_same_two_expression_then_calling_IsEquivalentTo_with_that_OneOfExpression_should_return_true(FilterExpression left, FilterExpression right)
        {
            // Arrange
            OneOfExpression oneOf = new(left, right);
            OrExpression or = new(left: right, right: left);

            // Act
            bool actual = or.IsEquivalentTo(oneOf);

            // Assert
            return actual.ToProperty();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Given_ConstantExpression_equals_left_and_left_equals_right_expression_IsEquivalentTo_should_be_true(ConstantValueExpression filterExpression)
            => Given_FilterExpression_equals_left_and_right_IsEquivalentTo_should_return_true(filterExpression);

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Given_DateExpression_equals_left_and_left_equals_right_expression_IsEquivalentTo_should_be_true(DateExpression filterExpression)
            => Given_FilterExpression_equals_left_and_right_IsEquivalentTo_should_return_true(filterExpression);

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Given_DateTimeExpression_equals_left_and_left_equals_right_expression_IsEquivalentTo_should_be_true(DateTimeExpression filterExpression)
            => Given_FilterExpression_equals_left_and_right_IsEquivalentTo_should_return_true(filterExpression);

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Given_AsteriskExpression_equals_left_and_left_equals_right_expression_IsEquivalentTo_should_be_true(AsteriskExpression filterExpression)
            => Given_FilterExpression_equals_left_and_right_IsEquivalentTo_should_return_true(filterExpression);

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Given_left_eq_right_eq_OrExpression_Simplify_should_returns_the_inner_OrExpression(OrExpression filterExpression)
        {
            // Arrange
            OrExpression orExpression = new(filterExpression, filterExpression);

            // Act
            FilterExpression simplified = orExpression.Simplify();
            bool isEquivalent = simplified.IsEquivalentTo(filterExpression);
            // Assert
            isEquivalent.Should()
                        .BeTrue();
        }

        private static void Given_FilterExpression_equals_left_and_right_IsEquivalentTo_should_return_true(FilterExpression expression)
        {
            // Arrange
            OrExpression filterExpression = new(expression, expression);

            // Act
            bool actual = filterExpression.IsEquivalentTo(expression);

            // Assert
            actual.Should().Be(true);
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_OrExpression_GetComplexity_should_return_sum_of_left_and_right_complexity(OrExpression orExpression)
            => (orExpression.Complexity == orExpression.Left.Complexity + orExpression.Right.Complexity).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Given_left_eq_right_Simplify_should_return_left(FilterExpression expected)
        {
            // Arrange
            OrExpression orExpression = new(expected, expected);

            // Act
            FilterExpression actual = orExpression.Simplify();

            _outputHelper.WriteLine($"Simplifiied expression : {actual}");

            bool isEquivalent = actual.IsEquivalentTo(expected);

            // Assert
            isEquivalent.Should()
                        .BeTrue("the meaning of the expression should remain the same even after being simplified");
        }

        public static IEnumerable<object[]> SimplifyCases
        {
            get
            {
                yield return new object[]
                {
                    new OrExpression(new NumericValueExpression("1"), new OrExpression(new NumericValueExpression("1"), new NumericValueExpression("1"))),
                    new NumericValueExpression("1")
                };
            }
        }

        [Theory]
        [MemberData(nameof(SimplifyCases))]
        public void Given_OrExpression_Simplify_should_return_expected_result(OrExpression orExpression, FilterExpression expected)
        {
            // Act
            FilterExpression actual = orExpression.Simplify();

            // Assert
            actual.Should()
                  .Be(expected);
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Given_two_OrExpression_instances_one_and_two_where_oneU002Eleft_eq_twoU002Eright_and_oneU002Eright_eq_twoU002Eleft_IsEquivalentTo_should_return_true(FilterExpression first, FilterExpression second)
        {
            // Arrange
            OrExpression one = new(first, second);
            OrExpression two = new(second, first);

            // Act
            bool actual = one.IsEquivalentTo(two);

            // Assert
            actual.Should()
                  .BeTrue();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_OrExpression_instance_one_oneU002EIsEquivalentTo_one_should_return_true(OrExpression or)
            => or.IsEquivalentTo(or).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Given_left_is_AndExpression_instance_and_right_is_not_null_Constructor_should_wrap_left_inside_a_GroupExpression_instance(NonNull<AndExpression> left, NonNull<FilterExpression> right)
        {
            // Act
            OrExpression or = new(left.Item, right.Item);

            // Assert
            or.Left.Should()
                    .BeOfType<GroupExpression>($"Left instance is a '{nameof(AndExpression)}'")
                    .Which.IsEquivalentTo(left.Item).Should()
                    .BeTrue("Wrapping should not change the meaning of resulting expression");
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Given_left_is_OrExpression_instance_and_right_is_not_null_Constructor_should_wrap_left_inside_a_GroupExpression_instance(NonNull<OrExpression> left, NonNull<FilterExpression> right)
        {
            // Act
            OrExpression or = new(left.Item, right.Item);

            // Assert
            or.Left.Should()
                    .BeOfType<GroupExpression>($"Left instance is a '{nameof(AndExpression)}'")
                    .Which.IsEquivalentTo(left.Item).Should()
                    .BeTrue("Wrapping should not change the meaning of resulting expression");
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Given_left_is_not_null_and_right_is_AndExpression_Constructor_should_wrap_left_inside_a_GroupExpression_instance(NonNull<AndExpression> right, NonNull<FilterExpression> left)
        {
            // Act
            OrExpression or = new(left.Item, right.Item);

            // Assert
            or.Right.Should()
                    .BeOfType<GroupExpression>($"Right instance is a '{nameof(AndExpression)}'")
                    .Which.IsEquivalentTo(right.Item).Should().BeTrue("Wrapping should not change the meaning of resulting expression");
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Given_right_is_not_null_and_right_is_OrExpression_Constructor_should_wrap_left_inside_a_GroupExpression_instance(NonNull<OrExpression> right, NonNull<FilterExpression> left)
        {
            // Act
            OrExpression or = new(left.Item, right.Item);

            // Assert
            or.Right.Should()
                    .BeOfType<GroupExpression>($"Right instance is a '{nameof(AndExpression)}'")
                    .Which.IsEquivalentTo(right.Item).Should()
                    .BeTrue("Wrapping should not change the meaning of resulting expression");
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_commutative(NonNull<OrExpression> first, FilterExpression second)
            => (first.Item.Equals(second) == second.Equals(first.Item)).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_reflexive(NonNull<OrExpression> expression)
            => expression.Item.Equals(expression.Item).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_symetric(NonNull<OrExpression> expression, NonNull<FilterExpression> otherExpression)
            => (expression.Item.Equals(otherExpression.Item) == otherExpression.Item.Equals(expression.Item)).ToProperty();

    }
}
