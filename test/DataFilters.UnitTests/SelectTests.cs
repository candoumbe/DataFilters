using DataFilters.TestObjects;

using FluentAssertions;

using System;
using System.Collections.Generic;

using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace DataFilters.UnitTests
{
    [UnitTest]
    public class SelectTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public SelectTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Theory]
        [InlineData("", "Expression is empty")]
        [InlineData(" ", "Expression is whitespace only")]
        [InlineData(null, "Expression is null")]
        [InlineData("prop prop2", "Expression does not conform to a valid property name")]
        [InlineData(@"prop[""""]", "Expression does not conform to a valid property name")]
        public void Ctor_Throws_ArgumentException_If_Expression_Is_Null(string expression, string reason)
        {
            // Act
            Action action = () => new Select(expression);

            // Assert
            action.Should()
                .Throw<ArgumentException>(reason)
                .Where(ex => !string.IsNullOrWhiteSpace(ex.ParamName), "Param name cannot be null")
                .Where(ex => !string.IsNullOrWhiteSpace(ex.Message), "Message cannot be null");
        }

        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                yield return new object[]
                {
                    new Select("Name"),
                    new Select("Name"),
                    true,
                    $"Two distinct {nameof(Select)} instances with same properties must be equal"
                };

                {
                    Select selector = new("Name");
                    yield return new object[]
                    {
                        selector,
                        selector,
                        true,
                        $"A {nameof(Select)} instance is equal to itself"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void EqualsTests(Select first, object second, bool expected, string reason)
        {
            _outputHelper.WriteLine($"{nameof(first)} : '{first}'");
            _outputHelper.WriteLine($"{nameof(second)} : '{second}'");

            // Act
            bool actual = first.Equals(second);
            int actualHashCode = first.GetHashCode();

            // Assert
            actual.Should()
                .Be(expected, reason);

            if (expected)
            {
                actualHashCode.Should()
                    .Be(second?.GetHashCode(), reason);
            }
            else
            {
                actualHashCode.Should()
                    .NotBe(second?.GetHashCode(), reason);
            }
        }

        [Theory]
        [InlineData("prop")]
        [InlineData("prop.subProp")]
        [InlineData(@"prop[""subProp""]")]
        public void Given_valid_inputs_Ctor_should_fills_Expression_property_accordingly(string input)
        {
            // Act
            Select select = new(input);

            // Assert
            select.Expression.Should()
                             .Be(input);
        }

        public void Given_non_null_object_and_selector_ToExpression()
        {
            // Arrange
            Select select = new("prop");
        }
    }
}
