using Datafilters.Expressions;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;
using Xunit.Categories;
using static DataFilters.SortDirection;


namespace DataFilters.Expressions.UnitTests
{
    [UnitTest]
    [Feature("Expressions")]
    public class SortToOrderClauseTests
    {
        
        public static IEnumerable<object[]> SortToOrderClauseCases
        {
            get
            {
                yield return new object[]
                {
                    new[]
                    {
                        new Hero { Name = "Batman"},
                        new Hero { Name = "Flash"}
                    },
                    new Sort<Hero>(nameof(Hero.Name)),
                    (Expression<Func<IEnumerable<Hero>, bool>>)(heroes => heroes.Count() == 2
                        && heroes.First().Name == "Batman"
                        && heroes.Last().Name == "Flash"
                    )
                };

                yield return new object[]
                {
                    new[]
                    {
                        new Hero { Name = "Batman"},
                        new Hero { Name = "Flash"}
                    },
                    new Sort<Hero>("name"),
                    (Expression<Func<IEnumerable<Hero>, bool>>)(heroes => heroes.Count() == 2
                        && heroes.First().Name == "Batman"
                        && heroes.Last().Name == "Flash"
                    )
                };

                yield return new object[]
                {
                    new[]
                    {
                        new Hero { Name = "Batman"},
                        new Hero { Name = "Flash"}
                    },
                    new Sort<Hero>(" name"),
                    (Expression<Func<IEnumerable<Hero>, bool>>)(heroes => heroes.Count() == 2
                        && heroes.First().Name == "Batman"
                        && heroes.Last().Name == "Flash"
                    )
                };


                yield return new object[]
                {
                    new[]
                    {
                        new Hero { Name = "Batman"},
                        new Hero { Name = "Flash"}
                    },
                    new Sort<Hero>(nameof(Hero.Name), Descending),

                    (Expression<Func<IEnumerable<Hero>, bool>>)(heroes => heroes.Count() == (2)
                        && heroes.First().Name == "Flash"
                        && heroes.Last().Name == "Batman"
                    )
                };

                yield return new object[]
                {
                    new[]
                    {
                        new Hero { Name = "Batman", Age = 27},
                        new Hero { Name = "Wonder Woman", Age = 30},
                        new Hero { Name = "Flash", Age = 37}
                    },
                    new MultiSort<Hero>()
                        .Add(new Sort<Hero>(nameof(Hero.Age), Ascending))
                        .Add(new Sort<Hero>(nameof(Hero.Name), Descending))
                        ,

                    (Expression<Func<IEnumerable<Hero>, bool>>)(heroes => heroes.Count() == 3
                        && heroes.First().Name == "Batman"
                        && heroes.Last().Name == "Flash"
                    )
                };

                yield return new object[]
                {
                    new[]
                    {
                        new Hero { Name = "Batman", Age = 27},
                        new Hero { Name = "Wonder Woman", Age = 30},
                        new Hero { Name = "Flash", Age = 37}
                    },
                    new MultiSort<Hero>()
                        .Add(new Sort<Hero>(nameof(Hero.Name), Descending))
                        .Add(new Sort<Hero>(nameof(Hero.Age), Ascending))
                        ,

                    (Expression<Func<IEnumerable<Hero>, bool>>)(heroes => heroes.Count() == 3
                        && heroes.First().Name == "Wonder Woman"
                        && heroes.Last().Name == "Batman"
                    )
                };
            }
        }

        [Theory]
        [MemberData(nameof(SortToOrderClauseCases))]
        public void SortToOrderTests(IEnumerable<Hero> heroes, ISort<Hero> order, Expression<Func<IEnumerable<Hero>, bool>> expectation)
        {
            // Act
            IEnumerable<OrderClause<Hero>> sorts = order.ToOrderClause();

            IEnumerable<Hero> actual = heroes.AsQueryable()
                .OrderBy(sorts)
                .ToList();

            // Assert
            actual.Should()
                .Match(expectation);
        }
    }
}
