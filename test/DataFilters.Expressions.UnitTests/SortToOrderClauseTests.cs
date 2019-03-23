using Datafilters.Expressions;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;
using static DataFilters.SortDirection;


namespace DataFilters.Expressions.UnitTests
{
    public class SortToOrderClauseTests
    {
        public class Hero : IEquatable<Hero>
        {
            public string Name { get; set; }

            public int Age { get; set; }

            public override bool Equals(object obj) => Equals(obj as Hero);
            public bool Equals(Hero other) => other != null && Name == other.Name && Age == other.Age;
            public override int GetHashCode() => HashCode.Combine(Name, Age);

            public static bool operator ==(Hero hero1, Hero hero2)
            {
                return EqualityComparer<Hero>.Default.Equals(hero1, hero2);
            }

            public static bool operator !=(Hero hero1, Hero hero2)
            {
                return !(hero1 == hero2);
            }
        }


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
