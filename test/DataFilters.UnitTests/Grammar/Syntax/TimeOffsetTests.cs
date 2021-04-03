using DataFilters.Grammar.Syntax;

using FluentAssertions;

using Xunit;
using Xunit.Categories;

namespace DataFilters.UnitTests.Grammar.Syntax
{
    [UnitTest]
    public class TimeOffsetTests
    {
        [Theory]
        [InlineData(0, 0, "Z")]
        [InlineData(-1, 0, "-01:00")]
        [InlineData(2, 0, "+02:00")]
        [InlineData(0, -30, "-00:30")]
        public void Given_input_ToString_should_be_correct(int hours, int minutes, string expected)
        {
            // Arrange
            TimeOffset offset = new(hours, minutes);

            // Act
            string actual = offset.ToString();

            // Assert
            actual.Should()
                  .Be(expected);
        }
    }
}
