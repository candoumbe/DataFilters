using FluentAssertions;
using FluentAssertions.Extensions;
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
                    (Expression<Func<IEnumerable<Hero>, bool>>)(heroes => heroes.Exactly(2)
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
                    (Expression<Func<IEnumerable<Hero>, bool>>)(heroes => heroes.Exactly(2)
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
                    (Expression<Func<IEnumerable<Hero>, bool>>)(heroes => heroes.Exactly(2)
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

                    (Expression<Func<IEnumerable<Hero>, bool>>)(heroes => heroes.Exactly(2)
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
                    new MultiSort<Hero>
                    (
                        new Sort<Hero>(nameof(Hero.Age), Ascending),
                        new Sort<Hero>(nameof(Hero.Name), Descending)
                    ),
                    (Expression<Func<IEnumerable<Hero>, bool>>)(heroes => heroes.Exactly(3)
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
                    new MultiSort<Hero>
                    (
                        new Sort<Hero>(nameof(Hero.Name), Descending),
                        new Sort<Hero>(nameof(Hero.Age), Ascending)
                    ),
                    (Expression<Func<IEnumerable<Hero>, bool>>)(heroes => heroes.Exactly(3)
                                                                          && heroes.First().Name == "Wonder Woman"
                                                                          && heroes.Last().Name == "Batman"
                    )
                };

                yield return new object[]
                {
                    new[]
                    {
                        new Hero { Name = "Batman", FirstAppearance = 1.May(1939)},
                        new Hero { Name = "Flash" , FirstAppearance = 1.May(1940)}
                    },
                    new Sort<Hero>(nameof(Hero.FirstAppearance), Descending),

                    (Expression<Func<IEnumerable<Hero>, bool>>)(heroes => heroes.Exactly(2)
                                                                          && heroes.First().Name == "Flash"
                                                                          && heroes.Last().Name == "Batman"
                    )
                };

                yield return new object[]
                {
                    new[]
                    {
                        new Hero
                        {
                            Name = "Batman",
                            FirstAppearance = 1.May(1938),
                            Acolyte = new Hero { Name = "Robin", Age = 13 }
                        },
                        new Hero
                        {
                            Name = "Flash",
                            FirstAppearance = 1.May(1940),
                            Acolyte = new Hero { Name = "Kid Flash", Age = 15 }
                        },
                        new Hero
                        {
                            Name = "Green Arrow",
                            FirstAppearance = 1.May(1940),
                            Acolyte = new Hero { Name = "Red Arrow", Age = 14 }
                        }
                    },
                    new MultiSort<Hero>(
                        new Sort<Hero>(nameof(Hero.FirstAppearance), Descending),
                        new Sort<Hero>($@"{nameof(Hero.Acolyte)}.{nameof(Hero.Age)}", Ascending)
                    ),

                    (Expression<Func<IEnumerable<Hero>, bool>>)(heroes => heroes.Select(x => x.Name).SequenceEqual(new []{ "Green Arrow", "Flash", "Batman"}))
                };
            }
        }

        [Theory]
        [MemberData(nameof(SortToOrderClauseCases))]
        public void SortToOrderTests(IEnumerable<Hero> heroes, ISort<Hero> order, Expression<Func<IEnumerable<Hero>, bool>> expectation)
        {
            // Act
            IEnumerable<Hero> actual = heroes.AsQueryable()
                                             .OrderBy(order)
                                             .ToList();

            // Assert
            actual.Should()
                  .Match(expectation);
        }
    }
}
