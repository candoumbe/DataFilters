using DataFilters.Casing;

using FluentAssertions;

using Xunit;
using Xunit.Categories;

namespace DataFilters.UnitTests;

[UnitTest]
public class FilterOptionsTests
{
    [Fact]
    public void Ctor_should_create_instance_with_default_values()
    {
        // Act
        FilterOptions options = new ();

        // Assert
        options.DefaultPropertyNameResolutionStrategy.Should()
            .Be(PropertyNameResolutionStrategy.Default);
        options.Logic.Should()
            .Be(FilterLogic.And);
    }

    [Fact]
    public void Given_null_value_DefaultPropertyNameResolutionStrategy_should_be_Default()
    {
        // Arrange
        FilterOptions options = new();

        // Act
#if NET6_0_OR_GREATER
        options = options with { DefaultPropertyNameResolutionStrategy = null }; 
#else
        options = new () { DefaultPropertyNameResolutionStrategy = null };
#endif

        // Assert
        options.DefaultPropertyNameResolutionStrategy.Should()
            .Be(PropertyNameResolutionStrategy.Default);
    }
}
