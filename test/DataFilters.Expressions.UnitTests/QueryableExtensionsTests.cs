namespace DataFilters.Expressions.UnitTests
{
    using FluentAssertions;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    [UnitTest]
    public class QueryableExtensionsTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public QueryableExtensionsTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        public static IEnumerable<object[]> ThrowsArgumentNullExceptionCases
        {
            get
            {
                yield return new object[]
                {
                    null,
                    new Order<Hero>(nameof(Hero.Name))
                };

                yield return new object[]
                {
                    Enumerable.Empty<Hero>().AsQueryable(),
                    null
                };
            }
        }

        [Theory]
        [MemberData(nameof(ThrowsArgumentNullExceptionCases))]
        public void Should_Throws_ArgumentNullException_When_Parameter_IsNull(IQueryable<Hero> heroes, IOrder<Hero> orderBy)
        {
            _outputHelper.WriteLine($"{nameof(heroes)} is null : {heroes == null}");
            _outputHelper.WriteLine($"{nameof(orderBy)} is null : {orderBy == null}");

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
}
