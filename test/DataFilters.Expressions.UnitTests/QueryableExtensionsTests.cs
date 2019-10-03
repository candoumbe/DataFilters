using DataFilters.Expressions;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace DataFilters.Expressions.UnitTests
{
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
                    new[]
                    {
                        OrderClause<Hero>.Create(x => x.Name)
                    }
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
        public void Should_Throws_ArgumentNullException_When_Parameter_IsNull(IQueryable<Hero> heroes, IEnumerable<OrderClause<Hero>> orderBy)
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
        public void Should_Throws_EmptyOrderByException_WhenOrderBy_IsEmpty()
        {
            // Act
            Action action = () => Enumerable.Empty<Hero>().AsQueryable().OrderBy(Enumerable.Empty<OrderClause<Hero>>());

            // Asser
            action.Should()
                .ThrowExactly<EmptyOrderByException>();
        }

        [Fact]
        public void Should_Throws_ArgumentNullException_WhenOrderBy_Null()
        {
            // Act
            Action action = () => Enumerable.Empty<Hero>().AsQueryable().OrderBy((ISort<Hero>)null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>();
        }
    }
}
