namespace DataFilters.Expressions.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using FluentAssertions;
    using FluentAssertions.Extensions;
    using Xunit;
    using Xunit.Categories;
    using static DataFilters.OrderDirection;

    [UnitTest]
    [Feature("Expressions")]
    public class ToOrderClauseTests
    {
        public static IEnumerable<object[]> ToOrderClauseCases
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
                    new Order<Hero>(nameof(Hero.Name)),
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
                    new Order<Hero>("name"),
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
                    new Order<Hero>(" name"),
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
                    new Order<Hero>(nameof(Hero.Name), Descending),

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
                    new MultiOrder<Hero>
                    (
                        new Order<Hero>(nameof(Hero.Age), Ascending),
                        new Order<Hero>(nameof(Hero.Name), Descending)
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
                    new MultiOrder<Hero>
                    (
                        new Order<Hero>(nameof(Hero.Name), Descending),
                        new Order<Hero>(nameof(Hero.Age), Ascending)
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
                    new Order<Hero>(nameof(Hero.FirstAppearance), Descending),

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
                    new MultiOrder<Hero>(
                        new Order<Hero>(nameof(Hero.FirstAppearance), Descending),
                        new Order<Hero>($"{nameof(Hero.Acolyte)}.{nameof(Hero.Age)}", Ascending)
                    ),

                    (Expression<Func<IEnumerable<Hero>, bool>>)(heroes => heroes.Select(x => x.Name).SequenceEqual(new []{ "Green Arrow", "Flash", "Batman"}))
                };
            }
        }

        [Theory]
        [MemberData(nameof(ToOrderClauseCases))]
        public void ToOrderTests(IEnumerable<Hero> heroes, IOrder<Hero> order, Expression<Func<IEnumerable<Hero>, bool>> expectation)
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
