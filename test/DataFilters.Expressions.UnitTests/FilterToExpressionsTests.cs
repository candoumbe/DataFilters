
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DataFilters.TestObjects;
using FluentAssertions;
using FluentAssertions.Extensions;
using NodaTime;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static DataFilters.Expressions.NullableValueBehavior;
using static DataFilters.FilterLogic;
using static DataFilters.FilterOperator;

namespace DataFilters.Expressions.UnitTests;

[Feature("Filters")]
[UnitTest]
public class FilterToExpressionTests(ITestOutputHelper output)
{
    public static TheoryData<IReadOnlyList<SuperHero>, IFilter, NullableValueBehavior, Expression<Func<SuperHero, bool>>> EqualToTestCases
        => new()
            {
                {
                    [],
                    new Filter(field: nameof(SuperHero.Firstname), @operator: EqualTo, value: "Bruce"),
                    NoAction,
                    item => item.Firstname == "Bruce"
                },

                {
                    [
                        new SuperHero { Acolytes = null }
                    ],
                    new Filter(field: $"{nameof(SuperHero.Acolytes)}.{nameof(SuperHero.Firstname)}", @operator: EqualTo, value: "Bruce"),
                    AddNullCheck,
                    item => item != null && item.Acolytes != null && item.Acolytes.Any(x => x != null && x.Firstname == "Bruce")
                },
                {
                    new[] { new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" } },
                    new Filter(field: nameof(SuperHero.Firstname), @operator: EqualTo, value: null),
                    NoAction,
                    item => item.Firstname == null
                },
                {
                    new[] { new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" } },
                    new Filter(field: nameof(SuperHero.Firstname), @operator: EqualTo, value: "Bruce"),
                    NoAction,
                    item => item.Firstname == "Bruce"
                },
                {
                    new[] { new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190 } },
                    new Filter(field: nameof(SuperHero.Height), @operator: EqualTo, value: 190),
                    NoAction,
                    item => item.Height == 190
                },
                {
                    new[] {
#if NET6_0_OR_GREATER
                        new SuperHero
                        {
                            Firstname = "Bruce",
                            Lastname = "Wayne",
                            Height = 190,
                            Nickname = "Batman",
                            BirthDate = DateOnly.FromDateTime(1.April(1938)),
                            PeakShape = TimeOnly.FromTimeSpan(13.Hours())
                        },
#else
                        new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman", BirthDate = 1.April(1938) },
#endif
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" }
                    },
                    new MultiFilter
                    {
                        Logic = Or,
                        Filters = new[] {
                            new Filter(field: nameof(SuperHero.Nickname), @operator: EqualTo, value: "Batman"),
                            new Filter(field: nameof(SuperHero.Nickname), @operator: EqualTo, value: "Superman")
                        }
                    },
                    NoAction,
                    item => item.Nickname == "Batman" || item.Nickname == "Superman"
                },
                {
                    [
                        new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                        new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash" }
                    ],
                    new MultiFilter
                    {
                        Logic = And,
                        Filters =
                        [
                            new Filter(field: nameof(SuperHero.Firstname), @operator: Contains, value: "a"),
                            new MultiFilter
                            {
                                Logic = Or,
                                Filters =
                                [
                                    new Filter(field: nameof(SuperHero.Nickname), @operator: EqualTo, value: "Batman"),
                                    new Filter(field: nameof(SuperHero.Nickname), @operator: EqualTo, value: "Superman")
                                ]
                            }
                        ]
                    },
                    NoAction,
                    item => item.Firstname.Contains('a') && (item.Nickname == "Batman" || item.Nickname == "Superman")
                },
                {
                    new[]
                    {
#if NET6_0_OR_GREATER
                        new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman", LastBattleDate = DateOnly.FromDateTime(25.December(2012)) },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman", LastBattleDate = DateOnly.FromDateTime(13.January(2007)) },
                        new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash", LastBattleDate = DateOnly.FromDateTime(18.April(2014)) },
                        new SuperHero { Firstname = "Freak", Lastname = "Hazoid", Height = 190, Nickname = "Zoom", LastBattleDate = DateOnly.FromDateTime(25.April(2014)), PeakShape = TimeOnly.FromTimeSpan(12.Hours()) },

#else
                        new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman", LastBattleDate = 25.December(2012) },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman", LastBattleDate = 13.January(2007)},
                        new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash", LastBattleDate = 18.April(2014) }
#endif
                    },
                    new MultiFilter
                    {
                        Logic = And,
                        Filters =
                        [
                            new Filter(field: nameof(SuperHero.Firstname), @operator: Contains, value: "a"),
                            new MultiFilter
                            {
                                Logic = And,
                                Filters =
                                [
#if NET6_0_OR_GREATER
                                    new Filter(field: nameof(SuperHero.LastBattleDate), @operator: GreaterThan, value: DateOnly.FromDateTime(1.January(2007))),
                                    new Filter(field: nameof(SuperHero.LastBattleDate), @operator: FilterOperator.LessThan, value: DateOnly.FromDateTime(31.December(2012))),
#else
		                            new Filter(field : nameof(SuperHero.LastBattleDate), @operator : GreaterThan, value : 1.January(2007)),
                                    new Filter(field : nameof(SuperHero.LastBattleDate), @operator : FilterOperator.LessThan, value : 31.December(2012))
#endif
                                ]
                            }
                        ]
                    },
                    NoAction,

#if NET6_0_OR_GREATER
                    item => item.Firstname.Contains('a')
                                      && DateOnly.FromDateTime(1.January(2007)) < item.LastBattleDate
                                      && item.LastBattleDate < DateOnly.FromDateTime(31.December(2012))
#else
                    (Expression<Func<SuperHero, bool>>)(item => item.Firstname.Contains("a")
                                                                && 1.January(2007) < item.LastBattleDate
                                                                && item.LastBattleDate < 31.December(2012))

#endif
                },
                {
                    [
#if NET6_0_OR_GREATER
                        new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman", LastBattleDate = DateOnly.FromDateTime(25.December(2012)) },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman", LastBattleDate = DateOnly.FromDateTime(13.January(2007)) },
                        new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash", LastBattleDate = DateOnly.FromDateTime(18.April(2014)) }
#else
                        new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman", LastBattleDate = 25.December(2012) },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman", LastBattleDate = 13.January(2007)},
                        new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash", LastBattleDate = 18.April(2014) }
#endif
                    ],
                    new MultiFilter
                    {
                        Logic = And,
                        Filters =
                        [
                            new Filter(field: nameof(SuperHero.Firstname), @operator: Contains, value: "a"),
                            new MultiFilter
                            {
                                Logic = And,
                                Filters =
                                [
#if NET6_0_OR_GREATER
                                    new Filter(field: nameof(SuperHero.LastBattleDate), @operator: GreaterThan, value: DateOnly.FromDateTime(1.January(2007))),
                                    new Filter(field: nameof(SuperHero.LastBattleDate), @operator: FilterOperator.LessThan, value: DateOnly.FromDateTime(31.December(2012)))
#else
		                            new Filter(field : nameof(SuperHero.LastBattleDate), @operator : GreaterThan, value : 1.January(2007)),
                                    new Filter(field : nameof(SuperHero.LastBattleDate), @operator : FilterOperator.LessThan, value : 31.December(2012))
#endif
                                ]
                            }
                        ]
                    },
                    NoAction,
#if NET6_0_OR_GREATER
                    item => item.Firstname.Contains('a')
                                      && DateOnly.FromDateTime(1.January(2007)) < item.LastBattleDate
                                      && item.LastBattleDate < DateOnly.FromDateTime(31.December(2012))
#else
                    (Expression<Func<SuperHero, bool>>)(item => item.Firstname.Contains("a")
                                                                && 1.January(2007) < item.LastBattleDate
                                                                && item.LastBattleDate < 31.December(2012))
#endif
                },
                {
                    [
                        new SuperHero
                        {
                            Firstname = "Bruce",
                            Lastname = "Wayne",
                            Height = 190,
                            Nickname = "Batman",
                        },
                        new SuperHero
                        {
                            Firstname = "Clark",
                            Lastname = "Kent",
                            Nickname = "Superman",
                            Powers = [ "super strength", "heat vision" ]
                        }
                    ],
                    new Filter(field: nameof(SuperHero.Powers), @operator: EqualTo, value: "heat"),
                    NoAction,
                    item => item.Powers.Any(power => power == "heat")
                },
                {
                    [
                        new SuperHero
                        {
                            Firstname = "Bruce",
                            Lastname = "Wayne",
                            Height = 190,
                            Nickname = "Batman",
                            Henchman = new Henchman
                            {
                                Nickname = "Robin",
                                Weapons =
                                [
                                    new Weapon { Name = "stick", Level = 100 }
                                ]
                            }
                        }
                    ],
                    new Filter(field: $"""{nameof(SuperHero.Henchman)}["{nameof(Henchman.Weapons)}"]["{nameof(Weapon.Name)}"]""",
                               @operator: EqualTo,
                               value: "stick"),
                    NoAction,
                    item => item.Henchman.Weapons.Any(weapon => weapon.Name == "stick")
                }
            };

    [Theory]
    [MemberData(nameof(EqualToTestCases))]
    public void BuildEqual(IReadOnlyList<SuperHero> superheroes, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<SuperHero, bool>> expression)
        => Build(superheroes, filter, nullableValueBehavior, expression);

    public static TheoryData<IReadOnlyList<SuperHero>, IFilter, NullableValueBehavior, Expression<Func<SuperHero, bool>>> IsNullTestCases
        => new()
        {
            {
                    [],
                    new Filter(field : nameof(SuperHero.Firstname), @operator : IsNull),
                    NoAction,
                    item => item.Firstname == null
            },
            {
                    [
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                        new SuperHero { Firstname = null, Lastname = "", Height = 178, Nickname = "Sinestro" }
                    ],
                    new Filter(field : nameof(SuperHero.Firstname), @operator : IsNull),
                    NoAction,
                    item => item.Firstname == null
            }
        };

    [Theory]
    [MemberData(nameof(IsNullTestCases))]
    public void BuildIsNull(IReadOnlyList<SuperHero> superheroes, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<SuperHero, bool>> expression)
        => Build(superheroes, filter, nullableValueBehavior, expression);

    public static TheoryData<IReadOnlyList<SuperHero>, IFilter, NullableValueBehavior, Expression<Func<SuperHero, bool>>> IsEmptyTestCases
        => new()
        {
            {
                [],
                new Filter(field : nameof(SuperHero.Lastname), @operator : IsEmpty),
                NoAction,
                item => item.Lastname == string.Empty
            },
            {
                [
                    new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                    new SuperHero { Firstname = "", Lastname = "", Height = 178, Nickname = "Sinestro" }
                ],
                new Filter(field : nameof(SuperHero.Lastname), @operator : IsEmpty),
                NoAction,
                item => item.Lastname == string.Empty
            },
            {
                [
                    new SuperHero {
                        Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman",
                        Henchman = new Henchman
                        {
                            Firstname = "Dick", Lastname = "Grayson", Nickname = "Robin"
                        }
                    },
                    new SuperHero
                    {
                        Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman",
                        Henchman = new Henchman
                        {
                            Nickname = "Krypto"
                        }
                    }
                ],
                new Filter(field : $@"{nameof(SuperHero.Henchman)}[""{nameof(Henchman.Lastname)}""]", @operator : IsEmpty),
                NoAction,
                item => item.Henchman.Lastname == string.Empty
            },
            {
                [
                    new SuperHero
                    {
                        Firstname = "Bruce",
                        Lastname = "Wayne",
                        Height = 190,
                        Nickname = "Batman",
                        Henchman = new Henchman
                        {
                            Firstname = "Dick",
                            Lastname = "Grayson"
                        }
                    }
                ],
                new Filter(field : $@"{nameof(SuperHero.Henchman)}[""{nameof(Henchman.Firstname)}""]", @operator : NotEqualTo, value: "Dick"),
                NoAction,
                item => item.Henchman.Firstname != "Dick"
            },
            {
                [
                    new SuperHero {
                        Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman",
                    },
                    new SuperHero
                    {
                        Firstname = "Clark", Lastname = "Kent", Nickname = "Superman",
                        Powers = new [] { "super strength", "heat vision" }
                    }
                ],
                new Filter(field : nameof(SuperHero.Powers), @operator : IsEmpty),
                NoAction,
                item => !item.Powers.Any()
            },
            {
                [
                    new SuperHero {
                        Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman",
                    },
                    new SuperHero
                    {
                        Firstname = "Clark", Lastname = "Kent", Nickname = "Superman",
                        Powers = new [] { "super strength", "heat vision" }
                    }
                ],
                new Filter(field : $@"{nameof(SuperHero.Acolytes)}[""{nameof(SuperHero.Weapons)}""]", @operator : IsEmpty),
                NoAction,
                item => item.Acolytes.Any(acolyte => !acolyte.Weapons.Any())
            }
        };

    [Theory]
    [MemberData(nameof(IsEmptyTestCases))]
    public void BuildIsEmpty(IReadOnlyList<SuperHero> superheroes, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<SuperHero, bool>> expression)
        => Build(superheroes, filter, nullableValueBehavior, expression);

    public static TheoryData<IReadOnlyList<SuperHero>, IFilter, NullableValueBehavior, Expression<Func<SuperHero, bool>>> IsNotEmptyTestCases
        => new()
        {
            {
                    [],
                    new Filter(field : nameof(SuperHero.Lastname), @operator : IsNotEmpty),
                    NoAction,
                    item => item.Lastname != string.Empty
            },
            {
                    new[] {
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                        new SuperHero { Firstname = "", Lastname = "", Height = 178, Nickname = "Sinestro" }
                    },
                    new Filter(field: nameof(SuperHero.Lastname), @operator: IsNotEmpty),
                    NoAction,
                    item => item.Lastname != string.Empty
            },
            {
                [
                    new SuperHero
                    {
                        Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman",
                        Henchman = new Henchman
                        {
                            Firstname = "Dick", Lastname = "Grayson", Nickname = "Robin"
                        }
                    },
                    new SuperHero
                    {
                        Firstname = "Clark",
                        Lastname = "Kent",
                        Height = 190,
                        Nickname = "Superman",
                        Acolytes = new []
                        {
                            new SuperHero { Nickname = "Krypto" }
                        }
                    }
                ],
                new Filter(field: $@"{nameof(SuperHero.Acolytes)}[""{nameof(Henchman.Lastname)}""]", @operator: IsNotEmpty),
                NoAction,
                item => item.Acolytes.Any(acolyte => acolyte.Lastname != string.Empty)
            },
            {
                [
                    new SuperHero {
                        Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman",
                    },
                    new SuperHero
                    {
                        Firstname = "Clark", Lastname = "Kent", Nickname = "Superman",
                        Powers = [ "super strength", "heat vision" ]
                    }
                ],
                new Filter(field : nameof(SuperHero.Powers), @operator : IsNotEmpty),
                NoAction,
                item => item.Powers.Any()
            }
        };

    [Theory]
    [MemberData(nameof(IsNotEmptyTestCases))]
    public void BuildIsNotEmpty(IReadOnlyList<SuperHero> superheroes, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<SuperHero, bool>> expression)
        => Build(superheroes, filter, nullableValueBehavior, expression);

    public static TheoryData<IReadOnlyList<SuperHero>, IFilter, NullableValueBehavior, Expression<Func<SuperHero, bool>>> StartsWithCases
        => new()
        {
            {
                [
                    new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" },
                    new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                    new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190 }
                ],
                new Filter(field : nameof(SuperHero.Nickname), @operator : StartsWith, value: "B"),
                AddNullCheck,
                item => item != null && item.Nickname != null && item.Nickname.StartsWith('B')
            }
        };

    [Theory]
    [MemberData(nameof(StartsWithCases))]
    public void BuildStartsWith(IReadOnlyList<SuperHero> superheroes, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<SuperHero, bool>> expression)
        => Build(superheroes, filter, nullableValueBehavior, expression);

    public static TheoryData<IReadOnlyList<SuperHero>, IFilter, NullableValueBehavior, Expression<Func<SuperHero, bool>>> NotStartsWithCases
        => new()
        {
            {
                [
                    new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" },
                    new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                    new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash" }
                ],
                new Filter(field : nameof(SuperHero.Nickname), @operator : NotStartsWith, value: "B"),
                NoAction,
                item => item != null && !item.Nickname.StartsWith("B")
            },
            {
                [
                    new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" },
                    new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                    new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash" }
                ],
                new Filter(field : nameof(SuperHero.Nickname), @operator : NotStartsWith, value: 'B'),
                NoAction,
                item => item != null && !item.Nickname.StartsWith('B')
            }
        };

    [Theory]
    [MemberData(nameof(NotStartsWithCases))]
    public void BuildNotStartsWith(IReadOnlyList<SuperHero> superheroes, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<SuperHero, bool>> expression)
        => Build(superheroes, filter, nullableValueBehavior, expression);

    public static TheoryData<IReadOnlyList<SuperHero>, IFilter, NullableValueBehavior, Expression<Func<SuperHero, bool>>> EndsWithCases
        => new()
        {
            {
                [
                    new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" },
                    new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                    new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash" }
                ],
                new Filter(field: nameof(SuperHero.Nickname), @operator: EndsWith, value:"n"),
                NoAction,
                item => item.Nickname.EndsWith("n")
            },
            {
                [
                    new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" },
                    new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                    new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash" }
                ],
                new Filter(field: nameof(SuperHero.Nickname), @operator: EndsWith, value:'n'),
                NoAction,
                item => item.Nickname.EndsWith('n')
            }
        };

    [Theory]
    [MemberData(nameof(EndsWithCases))]
    public void BuildEndsWith(IReadOnlyList<SuperHero> superheroes, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<SuperHero, bool>> expression)
        => Build(superheroes, filter, nullableValueBehavior, expression);

    public static TheoryData<IReadOnlyList<SuperHero>, IFilter, NullableValueBehavior, Expression<Func<SuperHero, bool>>> NotEndsWithCases
        => new()
        {
            {
                [
                    new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" },
                    new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                    new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash" }
                ],
                new Filter(field: nameof(SuperHero.Nickname), @operator: NotEndsWith, value:"n"),
                NoAction,
                item => !item.Nickname.EndsWith('n')
            }
        };

    [Theory]
    [MemberData(nameof(NotEndsWithCases))]
    public void BuildNotEndsWith(IReadOnlyList<SuperHero> superheroes, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<SuperHero, bool>> expression)
        => Build(superheroes, filter, nullableValueBehavior, expression);

    public static TheoryData<IReadOnlyList<SuperHero>, IFilter, NullableValueBehavior, Expression<Func<SuperHero, bool>>> ContainsCases
        => new()
        {
            {
                [
                    new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" },
                    new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                    new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash" }
                ],
                new Filter(field:nameof(SuperHero.Nickname), @operator: Contains, value: "an"),
                NoAction,
                item => item.Nickname.Contains("an")
            },
            {

                [
                    new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" },
                    new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman", Powers = [ "super strength", "heat vision"] },
                    new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash", Powers = ["super speed"] }
                ],
                new Filter(field:nameof(SuperHero.Powers), @operator: Contains, value: "strength"),
                NoAction,
                item => item.Powers.Any( power => power.Contains("strength"))
            },
            {
                [
                    new SuperHero {
                        Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman",
                    },
                    new SuperHero
                    {
                        Firstname = "Clark", Lastname = "Kent", Nickname = "Superman",
                        Powers = ["super strength", "heat vision"]
                    },
                    new SuperHero
                    {
                        Firstname = "Barry", Lastname = "Allen", Nickname = "Flash",
                        Powers = [ "super speed" ]
                    }
                ],
                new MultiFilter
                {
                    Logic = Or,
                    Filters =
                    [
                        new Filter(field : nameof(SuperHero.Powers), @operator : Contains, value: "heat"),
                        new Filter(field : nameof(SuperHero.Powers), @operator : Contains, value: "speed")
                    ]
                },
                NoAction,
                item => item.Powers.Any(power => power.Contains("heat") || power.Contains("speed"))
            }
        };

    [Theory]
    [MemberData(nameof(ContainsCases))]
    public void BuildContains(IReadOnlyList<SuperHero> superheroes, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<SuperHero, bool>> expression)
        => Build(superheroes, filter, nullableValueBehavior, expression);

    public static TheoryData<IReadOnlyList<SuperHero>, IFilter, NullableValueBehavior, Expression<Func<SuperHero, bool>>> NotContainsCases
        => new()
        {
            {
                [
                    new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" },
                    new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                    new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash" }
                ],
                new Filter(field:nameof(SuperHero.Nickname), @operator: NotContains, value: "an"),
                NoAction,
                item => !item.Nickname.Contains("an")
            }
        };

    [Theory]
    [MemberData(nameof(NotContainsCases))]
    public void BuildNotContains(IReadOnlyList<SuperHero> superheroes, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<SuperHero, bool>> expression)
        => Build(superheroes, filter, nullableValueBehavior, expression);

    public static TheoryData<IReadOnlyList<SuperHero>, IFilter, NullableValueBehavior, Expression<Func<SuperHero, bool>>> LessThanTestCases
        => new()
        {
            {
                [
                    new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                    new SuperHero { Firstname = null, Lastname = "", Height = 178, Nickname = "Sinestro" }
                ],
                new Filter(field : nameof(SuperHero.Height), @operator : FilterOperator.LessThan, value: 150),
                NoAction,
                item => item.Height < 150
            }
        };

    [Theory]
    [MemberData(nameof(LessThanTestCases))]
    public void BuildLessThan(IReadOnlyList<SuperHero> superheroes, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<SuperHero, bool>> expression)
        => Build(superheroes, filter, nullableValueBehavior, expression);

    public static TheoryData<IReadOnlyList<SuperHero>, IFilter, NullableValueBehavior, Expression<Func<SuperHero, bool>>> NotEqualToTestCases
        => new()
        {
            {
                [],
                new Filter(field : nameof(SuperHero.Lastname), @operator : NotEqualTo, value : "Kent"),
                NoAction,
                item => item.Lastname != "Kent"
            },
            {
                [
                    new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" }
                ],
                new Filter(field : nameof(SuperHero.Lastname), @operator : NotEqualTo, value : "Kent"),
                NoAction,
                item => item.Lastname != "Kent"
            },
            {
                new[] {
                    new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" },
                    new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" }
                },
                new Filter(field : nameof(SuperHero.Lastname), @operator : NotEqualTo, value : "Kent"),
                NoAction,
                item => item.Lastname != "Kent"
            },
            {
                [
                    new SuperHero {
                        Firstname = "Bruce",
                        Lastname = "Wayne",
                        Height = 190,
                        Nickname = "Batman",
                        Henchman = new Henchman
                        {
                            Firstname = "Dick",
                            Lastname = "Grayson"
                        }
                    }
                ],
                new Filter(field : $@"{nameof(SuperHero.Henchman)}[""{nameof(Henchman.Firstname)}""]", @operator : NotEqualTo, value : "Dick"),
                NoAction,
                item => item.Henchman.Firstname != "Dick"
            }
        };

    [Theory]
    [MemberData(nameof(NotEqualToTestCases))]
    public void BuildNotEqual(IReadOnlyList<SuperHero> superheroes, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<SuperHero, bool>> expression)
        => Build(superheroes, filter, nullableValueBehavior, expression);

    public static TheoryData<IReadOnlyList<SuperHero>, IFilter, NullableValueBehavior, Expression<Func<SuperHero, bool>>> LessThanOrEqualToCases
        => new()
        {
            {
#if NET8_0_OR_GREATER
                [
                    new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman", LastBattleDate = DateOnly.FromDateTime(25.December(2012)) },
                    new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman", LastBattleDate = DateOnly.FromDateTime(13.January(2007))},
                    new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash", LastBattleDate = DateOnly.FromDateTime(18.April(2014)) }
                ],
                new Filter(field : nameof(SuperHero.LastBattleDate), @operator : LessThanOrEqualTo, value : DateOnly.FromDateTime(1.January(2007))),
                NoAction,
                item => item.Firstname.Contains('a')
                                                            && item.LastBattleDate <= DateOnly.FromDateTime(1.January(2007))
#else
                [
                    new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman", LastBattleDate = 25.December(2012) },
                    new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman", LastBattleDate = 13.January(2007)},
                    new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash", LastBattleDate = 18.April(2014) }
                ],
                new Filter(field : nameof(SuperHero.LastBattleDate), @operator : LessThanOrEqualTo, value : 1.January(2007)),
                NoAction,
                (Expression<Func<SuperHero, bool>>)(item => item.Firstname.Contains("a")
                                                                && item.LastBattleDate <= 1.January(2007))
#endif
            }
        };

    [Theory]
    [MemberData(nameof(LessThanOrEqualToCases))]
    public void BuildLessThanOrEqual(IReadOnlyList<SuperHero> superHeroes, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<SuperHero, bool>> expression)
        => Build(superHeroes, filter, nullableValueBehavior, expression);

    /// <summary>
    /// Tests various filters
    /// </summary>
    /// <param name="elements">Collections of </param>
    /// <param name="filter">filter under test</param>
    /// <param name="expression">Expression the filter should match once built</param>
    private void Build<T>(IReadOnlyList<T> elements, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<T, bool>> expression)
    {
        output.WriteLine($"Filtering {elements.Jsonify()}");
        output.WriteLine($"Filter under test : {filter}");
        output.WriteLine($"Behavior : {nullableValueBehavior}");
        output.WriteLine($"Reference expression : {expression.Body}");

        // Act
        Expression<Func<T, bool>> filterExpression = filter.ToExpression<T>(nullableValueBehavior);
        IEnumerable<T> filteredResult = elements
            .Where(filterExpression.Compile())
            .ToList();
        output.WriteLine($"Current expression : {filterExpression.Body}");

        // Assert
        filteredResult.Should()
                      .NotBeNull().And
                      .BeEquivalentTo(elements?.Where(expression.Compile()));
    }

    [Fact]
    public void ToExpressionThrowsArgumentNullExceptionWhenParameterIsNull()
    {
        // Act
        Action action = () => ((IFilter)null).ToExpression<Person>();

        // Assert
        action.Should().Throw<ArgumentNullException>().Which
            .ParamName.Should()
            .NotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData(Contains, " ")]
    [InlineData(EndsWith, "")]
    [InlineData(EqualTo, "")]
    [InlineData(GreaterThan, "")]
    [InlineData(GreaterThanOrEqual, "")]
    [InlineData(IsEmpty, "")]
    [InlineData(IsNotEmpty, "")]
    [InlineData(IsNull, "")]
    [InlineData(IsNotNull, "")]
    [InlineData(FilterOperator.LessThan, " ")]
    [InlineData(LessThanOrEqualTo, " ")]
    [InlineData(NotEqualTo, " ")]
    [InlineData(StartsWith, " ")]
    public void FilterIsConvertedToAlwaysTrueExpressionWhenFieldIsNull(FilterOperator @operator, object value)
    {
        // Arrange
        IReadOnlyList<SuperHero> superHeroes = [
            new SuperHero { Firstname = "Bruce", Lastname = "Wayne" },
            new SuperHero { Firstname = "Dick", Lastname = "Grayson" },
            new SuperHero { Firstname = "Diana", Lastname = "Price" },
        ];
        IFilter filter = new Filter(field: null, @operator: @operator, value: value);

        // Act
        Expression<Func<SuperHero, bool>> actualExpression = filter.ToExpression<SuperHero>();
        IEnumerable<SuperHero> superHeroesFiltered = superHeroes.Where(actualExpression.Compile());

        // Assert
        superHeroesFiltered.Should()
            .HaveSameCount(superHeroes).And
            .OnlyContain(x => superHeroes.Once(sh => x.Firstname == sh.Firstname && x.Lastname == sh.Lastname));
    }

    public static TheoryData<IReadOnlyList<Appointment>, IFilter, Expression<Func<Appointment, bool>>> DateTimeOffsetFilterCases
        => new()
        {
            {
                [
                    new Appointment{ Name = "Medical appointment", Date = 23.July(2012) },
                    new Appointment{ Name = "Medical appointment", Date = 10.September(2012) },
                ],
                new Filter(field: nameof(Appointment.Date), GreaterThan, 23.July(2012)),
                app => app.Date > 23.July(2012)
            },
            {
                [
                    new Appointment{ Name = "Medical appointment", Date = 23.July(2012) },
                    new Appointment{ Name = "Medical appointment", Date = 10.September(2012) },
                ],
                new Filter(field: nameof(Appointment.Date), GreaterThanOrEqual, 23.July(2012)),
                app => app.Date >= 23.July(2012)
            },
            {
                [
                    new Appointment { Name = "Medical appointment", Date = 12.April(2013).Add(14.Hours(1.Hours()) )},
                    new Appointment { Name = "Medical appointment", Date = 12.April(2013).Add(14.Hours()) }
                ],
                new Filter(field : nameof(Appointment.Date), EqualTo, 12.April(2013).Add(14.Hours(1.Hours()))),
                app => app.Date == 12.April(2013).Add(14.Hours(1.Hours()))
            }
        };

    [Theory]
    [MemberData(nameof(DateTimeOffsetFilterCases))]
    public void Filter_DateTimeOffset(IEnumerable<Appointment> appointments, IFilter filter, Expression<Func<Appointment, bool>> expression)
    {
        output.WriteLine($"Filtering {appointments.Jsonify()}");
        output.WriteLine($"Filter under test : {filter}");
        output.WriteLine($"Reference expression : {expression.Body}");

        // Act
        Expression<Func<Appointment, bool>> buildResult = filter.ToExpression<Appointment>();
        IEnumerable<Appointment> filteredResult = appointments.Where(buildResult.Compile())
                                                              .ToArray();
        output.WriteLine($"Current expression : {buildResult.Body}");

        // Assert
        output.WriteLine($"Filtered result : {filteredResult.Jsonify()}");
        filteredResult.Should()
                      .NotBeNull().And
                      .BeEquivalentTo(appointments?.Where(expression.Compile()));
    }

    public static TheoryData<IReadOnlyList<NodaTimeClass>, IFilter, NullableValueBehavior, Expression<Func<NodaTimeClass, bool>>> LocalDateFilterCases
        => new()
        {
            {
                [
                    new NodaTimeClass{ LocalDate = LocalDate.FromDateTime(19.January(2019)) },
                    new NodaTimeClass{ LocalDate = LocalDate.FromDateTime(21.January(2019)) }
                ],
                new Filter(nameof(NodaTimeClass.LocalDate), EqualTo, LocalDate.FromDateTime(19.January(2019))),
                NoAction,
                x => x.LocalDate == LocalDate.FromDateTime(19.January(2019))
            }
        };

    [Theory]
    [MemberData(nameof(LocalDateFilterCases))]
    public void Filter_LocalDate(IReadOnlyList<NodaTimeClass> elements, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<NodaTimeClass, bool>> expression)
        => Build(elements, filter, nullableValueBehavior, expression);

    [Theory]
    [InlineData(StartsWith, " ")]
    [InlineData(NotStartsWith, " ")]
    [InlineData(EndsWith, " ")]
    [InlineData(NotEndsWith, " ")]
    [InlineData(Contains, " ")]
    [InlineData(NotContains, " ")]
    public void Given_property_contains_null_value_and_behaviour_is_set_operator_will_result_in_instance_method_call_Expression_should_throws_NullReferenceException(FilterOperator op, object value)
    {
        // Arrange
        Hero[] heroes = [new()];

        Filter filter = new(nameof(Hero.Name), op, value);

        // Act
        Expression<Func<Hero, bool>> expression = filter.ToExpression<Hero>();
        Action act = () => _ = heroes.Where(expression.Compile()).ToArray();

        // Assert
        act.Should()
           .Throw<NullReferenceException>("the resulting expression would apply to a property without checking if it's null");
    }

    [Theory]
    [InlineData(StartsWith, " ")]
    [InlineData(NotStartsWith, " ")]
    [InlineData(EndsWith, " ")]
    [InlineData(NotEndsWith, " ")]
    [InlineData(Contains, " ")]
    [InlineData(NotContains, " ")]
    public void Given_property_is_enumerable_and_behaviour_is_set_operator_will_result_in_instance_method_call_Expression_should_not_throws_NullReferenceException(FilterOperator op, object value)
    {
        // Arrange
        SuperHero[] heroes = [new() { Acolytes = null }];

        Filter filter = new($"{nameof(SuperHero.Acolytes)}.{nameof(SuperHero.Nickname)}", op, value);

        // Act
        Expression<Func<SuperHero, bool>> expression = filter.ToExpression<SuperHero>(AddNullCheck);
        output.WriteLine($"Computed expression : {expression.Body}");
        Action act = () => _ = heroes.Where(expression.Compile()).ToArray();

        // Assert
        act.Should()
           .NotThrow();
    }
}