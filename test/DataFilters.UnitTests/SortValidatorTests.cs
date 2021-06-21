using FluentAssertions;

using FluentValidation.Results;

using Xunit;
using Xunit.Categories;

namespace DataFilters.UnitTests
{
    [UnitTest]
    [Feature("Sorting")]
    public class SortValidatorTests
    {
        private readonly SortValidator _sut;

        public SortValidatorTests()
        {
            _sut = new();
        }

        [Theory]
        [InlineData("", false, "Empty sort")]
        [InlineData(" ", false, "Whitespace sort")]
        [InlineData(@"prop[""subprop""]", true, "sort on a sub-property")]
        [InlineData(@"prop[""subprop1""][""subprop2""]", true, "sort on second level sub-property")]
        [InlineData(@"+prop[""subprop1""][""subprop2""], -prop[""subprop1""][""subprop3""]", true, "sort on second level sub-property")]
        public void Validate(string sort, bool expected, string reason)
        {
            // Act
            ValidationResult validationResult = _sut.Validate(sort);

            // Assert
            validationResult.IsValid.Should()
                                    .Be(expected, reason);
        }
    }
}