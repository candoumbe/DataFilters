using System;
using DataFilters.ValueObjects;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Xunit;
using Xunit.Categories;

namespace DataFilters.UnitTests.ValueObjects;

[UnitTest]
public class EscapedStringTests
{
    [Fact]
    public void Given_input_is_null_Then_Create_should_throw_ArgumentNullException()
    {
        // Arrange
        const string input = null;

        // Act
        Action createEscapedStringWhenInputIsNull = () => EscapedString.From(input);

        // Assert
        createEscapedStringWhenInputIsNull.Should().Throw<ArgumentNullException>();
    }

    [Property]
    public void Given_input_is_not_null_Then_Value_should_be_set_correctly(NonEmptyString inputGenerator)
    {
        // Arrange
        string input = inputGenerator.Get;

        // Act
        EscapedString actual = EscapedString.From(input);

        // Assert
        actual.Value.Should().Be(input);
    }

    [Property]
    public void Given_two_instances_were_built_from_same_value_Then_they_should_be_equal(NonEmptyString inputGenerator)
    {
        // Arrange
        EscapedString left = EscapedString.From(inputGenerator.Get);
        EscapedString right = EscapedString.From(inputGenerator.Get);

        // Act
        bool actual = left.Equals(right);

        // Assert
        actual.Should().BeTrue("both instances were built from the same input value");
    }
}