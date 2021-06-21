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
            .Implement<IHaveComplexity>().And
            .HaveConstructor(new[] { typeof(RegexValue[]) }).And
            .HaveProperty<IReadOnlyList<RegexValue>>("Values")
            ;

        [Fact]
        public void Ctor_Throws_ArgumentNullException_If_Value_Is_Null()
        {
            // Act
            Action action = () => new RegularExpression(null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>($"{nameof(RegularExpression)}.{nameof(RegularExpression.Values)} cannot be null");
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Two_RegularExpression_instances_built_with_same_input_should_be_equals(NonEmptyArray<RegexValue> inputs)
        {
            // Arrange
            RegularExpression first = new(inputs.Item);
            RegularExpression second = new(inputs.Item);

            // Act
            bool actual = first.Equals(second);

            // Assert
            return actual.ToProperty();
        }

        [Property(Arbitrary = new[] { typeof(ExpressionsGenerators) })]
        public Property Two_RegularExpression_instances_built_with_different_inputs_should_not_be_equal(NonEmptyArray<RegexValue> one, NonEmptyArray<RegexValue> two)
        {
            // Arrange
            RegularExpression first = new(one.Item);
            RegularExpression second = new(two.Item);

            // Act
            bool actual = !first.Equals(second);

            // Assert
            return actual.ToProperty();
        }
    }
}
