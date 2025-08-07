
using System;
using System.Linq;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace DataFilters.Expressions.UnitTests;
[UnitTest]
public class QueryableExtensionsTests(ITestOutputHelper outputHelper)
{
    public static TheoryData<IQueryable<Hero>, IOrder<Hero>> ThrowsArgumentNullExceptionCases
        => new()
        {
            {
                null,
                new Order<Hero>(nameof(Hero.Name))
            },
            {
                Enumerable.Empty<Hero>().AsQueryable(),
                null
            }
        };

    [Theory]
    [MemberData(nameof(ThrowsArgumentNullExceptionCases))]
    public void Should_Throws_ArgumentNullException_When_Parameter_IsNull(IQueryable<Hero> heroes, IOrder<Hero> orderBy)
    {
        outputHelper.WriteLine($"{nameof(heroes)} is null : {heroes == null}");
        outputHelper.WriteLine($"{nameof(orderBy)} is null : {orderBy == null}");

        // Act
        Action action = () => heroes.OrderBy(orderBy);

        // Assert
        action.Should()
            .ThrowExactly<ArgumentNullException>()
            .Where(ex => !string.IsNullOrWhiteSpace(ex.ParamName));
    }

    [Fact]
    public void Should_Throws_ArgumentNullException_WhenOrderBy_Null()
    {
        // Act
        Action action = () => Enumerable.Empty<Hero>().AsQueryable().OrderBy(null);

        // Assert
        action.Should()
            .ThrowExactly<ArgumentNullException>();
    }
}