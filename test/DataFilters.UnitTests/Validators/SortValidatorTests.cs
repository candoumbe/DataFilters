using FluentAssertions;
using FluentValidation.Results;
using System;
using Xunit;

namespace DataFilters.UnitTests.Validators
{
    public class SortValidatorTests
    {
        private readonly SortValidator _sut;

        public SortValidatorTests() => _sut = new SortValidator();

        [Theory]
        [InlineData("prop", true, "'prop' is a valid sort expression")]
        [InlineData("+prop", true, "'+prop' is a valid sort expression")]
        [InlineData("-prop", true, "'-prop' is a valid sort expression")]
        [InlineData("--prop", false, "'--prop' is not valid because it starts with two consecutive hyphens")]
        [InlineData("++prop", false, "'++prop' is not valid because it starts with two consecutive '+'")]
        [InlineData("+ + prop", false, "'+ + prop' is not valid because it starts with two consecutive '+'")]
        [InlineData(" ", false, "' ' is not valid because it contains only whitespace")]
        [InlineData("+ ", false, "'+ ' is not valid because it contains only a '+' followed by whitespaces")]
        [InlineData("- ", false, "'- ' is not valid because it contains only a '-' followed by whitespaces")]
        [InlineData("prop1,prop2", true, "'prop1,prop2' is not valid because it contains only a '-' followed by whitespaces")]
        [InlineData("prop1, prop2", true, "'prop1, prop2' is valid because it contains two properties separated by a comma")]
        [InlineData("prop1 prop2", false, "'prop1 prop2' is not valid because it contains two properties separated by a whitespace")]
        [InlineData(",prop1,prop2", false, "',prop1,prop2' is not valid because it starts with a ','")]
        [InlineData("prop1,prop2,", false, "'prop1,prop2,' is not valid because it ends with a ','")]
        public void IsValid(string sort, bool expected, string reason)
        {
            // Act
            ValidationResult actual = _sut.Validate(sort);

            // Assert
            actual.IsValid.Should()
                .Be(expected, reason);
        }
    }
}
