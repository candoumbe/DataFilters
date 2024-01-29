namespace DataFilters.UnitTests;

using System;
using DataFilters.Casing;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Xunit.Categories;

[UnitTest]
public class FilterServiceOptionsTests
{
    [Property]
    public void Given_no_parameters_Cosntructor_should_set_properties_with_default_values()
    {
        // Act
        FilterServiceOptions _sut = new();

        // Assert
        _sut.MaxCacheSize.Should().Be(FilterServiceOptions.DefaultCacheSize);
        _sut.PropertyNameResolutionStrategy.Should().Be(PropertyNameResolutionStrategy.Default);
    }

    [Property(Arbitrary = [typeof(Generators)])]
    public void Given_positive_integer_value_And_strategy_Constructor_should_set_properties(PositiveInt input, PropertyNameResolutionStrategy propertyNameResolutionStrategy)
    {
        // Act
        FilterServiceOptions _sut = new() { MaxCacheSize = input.Item, PropertyNameResolutionStrategy = propertyNameResolutionStrategy };

        // Assert
        _sut.MaxCacheSize.Should().Be(input.Item);
        _sut.PropertyNameResolutionStrategy.Should().Be(propertyNameResolutionStrategy);
    }

    [Property(Arbitrary = [typeof(Generators)])]
    public void Given_options_Validate_should_check_all_values_are_valid(int maxCacheSize, PropertyNameResolutionStrategy strategy)
    {
        // Arrange
        Lazy<FilterServiceOptions> lazy = new(() =>
        {
            FilterServiceOptions _sut = new()
            {
                PropertyNameResolutionStrategy = strategy,
                MaxCacheSize = maxCacheSize
            };
            _sut.Validate();

            return _sut;
        });

        // Act
        Action action = () =>
        {
            FilterServiceOptions _sut = new()
            {
                PropertyNameResolutionStrategy = strategy,
                MaxCacheSize = maxCacheSize
            };
            _sut.Validate();
        };

        // Assert
        object _ = maxCacheSize switch
        {
            < 1 => action.Should().Throw<FilterServiceOptionsInvalidValueException>(),
            _ => action.Should().NotThrow()
        };
    }
}