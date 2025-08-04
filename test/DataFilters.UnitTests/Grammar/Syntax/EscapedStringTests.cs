using System;
using DataFilters.Grammar.Syntax;
using FluentAssertions;
using FsCheck;
using Xunit;
using Xunit.Categories;

namespace DataFilters.UnitTests.Grammar.Syntax;

[UnitTest]
public class EscapedStringShould
{
    [Fact]
    public void Throws_ArgumentNullException_when_calling_From_with_null_parameter()
    {
        // Act
        Action callingFromWithNullValue = () => _ = EscapedString.From( null );

        // Assert
        callingFromWithNullValue.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Returns_Empty_when_calling_From_with_empty()
    {
        // Arrange
        string emptyString = string.Empty;

        // Act
        EscapedString escaped = EscapedString.From(emptyString);

        // Assert
        escaped.Should().Be(EscapedString.Empty);
    }
}