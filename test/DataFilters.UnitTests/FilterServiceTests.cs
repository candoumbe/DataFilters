#if NET5_0_OR_GREATER
namespace DataFilters.UnitTests;

using System;
using System.Collections.Generic;

using DataFilters.TestObjects;

using FluentAssertions;

using Xunit;
using Xunit.Abstractions;

public class FilterServiceTests
{
    private readonly FilterService _sut = new FilterService(new FilterServiceOptions());

    public static IEnumerable<object[]> ComputeCases
    {
        get
        {
            yield return new object[]
            {
                    "Nickname=Bat",
                    new Filter("Nickname", @operator: FilterOperator.EqualTo, "Bat")
            };
        }
    }

    [Theory]
    [MemberData(nameof(ComputeCases))]
    public void Given_input_Compute_should_build_expected_Filter(string input, IFilter expected)
    {
        // Act
        IFilter actual = _sut.Compute<SuperHero>(input);

        // Assert
        actual.Should()
              .Be(expected);
    }

    [Fact]
    public void Given_options_is_null_Constructor_should_throw_ArgumentNullException()
    {
        // Act
        Action ctorWhereOptionsIsNull = () => new FilterService(null);

        // Assert
        ctorWhereOptionsIsNull.Should()
                              .ThrowExactly<ArgumentNullException>();
    }
}
#endif