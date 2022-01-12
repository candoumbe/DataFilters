﻿#if NET5_0_OR_GREATER
namespace DataFilters.UnitTests;

using DataFilters.TestObjects;

using FluentAssertions;

using FsCheck.Xunit;
using FsCheck;

using System;
using System.Collections.Generic;

using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

public class FilterServiceTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly FilterService _sut;

    public FilterServiceTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _sut = new FilterService(new FilterServiceOptions());
    }

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