namespace DataFilters.UnitTests.Grammar.Syntax
{
    using DataFilters.Grammar.Syntax;
    using FluentAssertions;
    using FsCheck.Xunit;
    using FsCheck;

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;
    using DataFilters.UnitTests.Helpers;

    [Feature(nameof(DataFilters.Grammar.Syntax))]
    public class DateTimeExpressionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public DateTimeExpressionTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        [Fact]
        public void IsFilterExpression() => typeof(DateTimeExpression).Should()
                                            .BeAssignableTo<FilterExpression>().And
                                            .Implement<IEquatable<DateTimeExpression>>().And
                                            .HaveConstructor(new[] { typeof(DateExpression), typeof(TimeExpression)}).And
                                            .HaveConstructor(new[] { typeof(TimeExpression) }).And
                                            .HaveConstructor(new[] { typeof(DateExpression) }).And
                                            .HaveProperty<DateExpression>("Date").And
                                            .HaveProperty<TimeExpression>("Time");

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
        public void A_non_null_DateTimeExpression_instance_should_be_equal_to_itself(DateTimeExpression instance)
        {
            // Act
            bool actual = instance.Equals(instance);

            // Assert
            actual.Should()
                  .BeTrue();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public void Two_DateTimeExpression_instances_which_have_same_data_should_be_equal(DateExpression date, TimeExpression time)
        {
            // Arrange
            DateTimeExpression first = new(date, time);
            DateTimeExpression other = new(date, time);

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
            (DateExpression date, TimeExpression time) = source;

            // Act
            DateTimeExpression clone = new(date, time);

            // Assert
            clone.Should()
                 .Be(source);
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Given_DateTimeExpression_GetComplexity_should_return_1(DateTimeExpression dateTime) => (dateTime.Complexity == 1).ToProperty();
    }
}
