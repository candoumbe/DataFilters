namespace DataFilters.UnitTests.Grammar.Syntax
{
    using DataFilters.Grammar.Syntax;
    using FluentAssertions;
    using FsCheck.Xunit;
    using FsCheck;

    using System;
    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;
    using DataFilters.UnitTests.Helpers;
    using FsCheck.Fluent;
    using System.Collections.Generic;

    [Feature(nameof(DataFilters.Grammar.Syntax))]
    public class DateTimeExpressionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public DateTimeExpressionTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        [Fact]
        public void IsFilterExpression() => typeof(DateTimeExpression).Should()
                                            .BeAssignableTo<FilterExpression>().And
                                            .Implement<IEquatable<DateTimeExpression>>().And
                                            .HaveConstructor(new[] { typeof(DateExpression), typeof(TimeExpression), typeof(OffsetExpression) }).And
                                            .HaveConstructor(new[] { typeof(DateExpression), typeof(TimeExpression)}).And
                                            .HaveConstructor(new[] { typeof(TimeExpression) }).And
                                            .HaveConstructor(new[] { typeof(DateExpression) }).And
                                            .HaveProperty<DateExpression>("Date").And
                                            .HaveProperty<TimeExpression>("Time").And
                                            .HaveProperty<OffsetExpression>("Offset");

        [Fact]
        public void Given_date_and_time_parameters_are_null_Constructor_should_throws_ArgumentException()
        {
            // Act
            Action ctor = () => new DateTimeExpression(null, null);

            // Assert
            ctor.Should()
                .ThrowExactly<ArgumentException>("both date and time are null");
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void A_non_null_DateTimeExpression_instance_should_never_be_equal_to_null(DateTimeExpression instance)
        {
            // Act
            bool actual = instance.Equals(null);

            // Assert
            actual.Should()
                  .BeFalse();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Two_DateTimeExpression_instances_which_have_same_data_should_be_equal(DateExpression date, TimeExpression time, OffsetExpression offset)
        {
            // Arrange
            DateTimeExpression first = new(date, time, offset);
            DateTimeExpression other = new(date, time, offset);

            // Act
            bool actual = first.Equals(other);

            // Assert
            actual.Should()
                  .BeTrue();
            first.GetHashCode().Should()
                               .Be(other.GetHashCode(), "Two instances that are equal should have same hashcode");
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void A_DateTimeExpression_built_from_variables_obtained_from_deconstructing_a_DateTimeExpression_should_equal_the_original_DateTimeExpression_value(DateTimeExpression source)
        {
            _outputHelper.WriteLine(message: $"DateTimeExpression is {source}");
            (DateExpression date, TimeExpression time, OffsetExpression offset, _) = source;

            // Act
            DateTimeExpression clone = new(date, time, offset);

            // Assert
            clone.Should()
                 .Be(source);
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_DateTimeExpression_GetComplexity_should_return_1(NonNull<DateTimeExpression> dateTime) => (dateTime.Item.Complexity == 1).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_DateTimeExpression_where_only_date_part_is_set_IsEquivalent_should_be_true_when_comparing_to_a_non_null_DateExpression(NonNull<DateExpression> dateExpression)
        {
            // Arrange
            DateTimeExpression dateTimeExpression = new (dateExpression.Item);

            // Act
            bool actual = dateTimeExpression.Equals(dateExpression.Item) == dateExpression.Item.Equals(dateTimeExpression);

            // Assert
            return actual.ToProperty();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_DateTimeExpression_where_only_time_part_is_set_Equals_should_be_true_when_comparing_to_a_non_null_TimeExpression(NonNull<TimeExpression> timeExpression)
        {
            // Arrange
            DateTimeExpression dateTimeExpression = new(timeExpression.Item);

            // Act
            bool actual = dateTimeExpression.Equals(timeExpression.Item) == timeExpression.Item.Equals(dateTimeExpression);

            // Assert
            return actual.ToProperty();
        }

        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                yield return new object[]
                {
                    new DateTimeExpression(new TimeExpression()),
                    new TimeExpression(),
                    true,
                    $"{nameof(DateTimeExpression.Date)} and {nameof(DateTimeExpression.Date)} are null and TimeExpression are equal"
                };

                yield return new object[]
                {
                    new DateTimeExpression(new (), new (), new()),
                    new DateTimeExpression(new (), new (), new()),
                    true,
                    $"Two instances with {nameof(DateTimeExpression)} that are equal"
                };
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void Equals_should_work_as_expected(DateTimeExpression dateTime, object obj, bool expected, string reason)
        {
            // Act
            bool actual = dateTime.Equals(obj);

            // Assert
            actual.Should()
                  .Be(expected, reason);
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_commutative(NonNull<DateTimeExpression> first, FilterExpression second)
            => (first.Item.Equals(second) == second.Equals(first.Item)).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_reflexive(NonNull<DateTimeExpression> expression)
            => expression.Item.Equals(expression.Item).ToProperty();

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Equals_should_be_symetric(NonNull<DateTimeExpression> expression, NonNull<FilterExpression> otherExpression)
            => (expression.Item.Equals(otherExpression.Item) == otherExpression.Item.Equals(expression.Item)).ToProperty();

    }
}
