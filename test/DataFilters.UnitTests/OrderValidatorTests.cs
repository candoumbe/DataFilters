namespace DataFilters.UnitTests;

using FluentAssertions;

using FluentValidation.Results;

using Xunit;
using Xunit.Categories;

[UnitTest]
[Feature("Ordering")]
public class OrderValidatorTests
{
    private readonly OrderValidator _sut;

    public OrderValidatorTests()
    {
        _sut = new();
    }

    [Theory]
    [InlineData("", false, "Empty order")]
    [InlineData(" ", false, "Whitespace order")]
    [InlineData(@"prop[""subprop""]", true, "order by a sub-property")]
    [InlineData(@"prop[""subprop1""][""subprop2""]", true, "order by second level sub-property")]
    [InlineData(@"+prop[""subprop1""][""subprop2""], -prop[""subprop1""][""subprop3""]", true, "order by a second level sub-property")]
    public void Validate(string sort, bool expected, string reason)
    {
        // Act
        ValidationResult validationResult = _sut.Validate(sort);

        // Assert
        validationResult.IsValid.Should()
                                .Be(expected, reason);
    }
}