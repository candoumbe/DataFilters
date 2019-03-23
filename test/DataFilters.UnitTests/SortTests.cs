using FluentAssertions;
using System;
using Xunit;

namespace DataFilters.UnitTests
{
    public class SortTests
    {
        [Theory]
        [InlineData("", "Expression is empty")]
        [InlineData(" ", "Expression is whitespace only")]
        [InlineData(null, "Expression is null")]
        public void Ctor_Throws_ArgumentException_If_Expression_Is_Null(string expression, string reason)
        {
            // Act
            Action action = () => new Sort<object>(expression);

            // Assert
            action.Should()
                .Throw<ArgumentException>(reason)
                .Where(ex => !string.IsNullOrWhiteSpace(ex.ParamName), "Param name cannot be null")
                .Where(ex => !string.IsNullOrWhiteSpace(ex.Message), "Message cannot be null");
        }
    }
}
