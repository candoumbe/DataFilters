namespace DataFilters.Expressions.UnitTests;

using DataFilters.Expressions;
using DataFilters.TestObjects;

using FluentAssertions;
using FluentAssertions.Extensions;

using NodaTime;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

using static DataFilters.Expressions.NullableValueBehavior;
using static DataFilters.FilterLogic;
using static DataFilters.FilterOperator;

[Feature("Filters")]
[UnitTest]
public class FilterToExpressionTests
{
    private readonly ITestOutputHelper _output;

    public FilterToExpressionTests(ITestOutputHelper output) => _output = output;

    public static IEnumerable<object[]> EqualToTestCases
    {
        get
        {
            yield return new object[]
            {
                Enumerable.Empty<SuperHero>(),
                new Filter(field : nameof(SuperHero.Firstname), @operator : EqualTo, value : "Bruce"),
                NoAction,
                (Expression<Func<SuperHero, bool>>)(item => item.Firstname == "Bruce")
            };

            yield return new object[]
            {
                new[]
                {
                    new SuperHero(){ Acolytes = null}
                },
                new Filter(field : $"{nameof(SuperHero.Acolytes)}.{nameof(SuperHero.Firstname)}", @operator : EqualTo, value : "Bruce"),
                AddNullCheck,
                (Expression<Func<SuperHero, bool>>)(item => item != null && item.Acolytes != null && item.Acolytes.Any(x => x != null && x.Firstname == "Bruce"))
            };

            yield return new object[]
            {
                new[] {new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" }},
                new Filter(field : nameof(SuperHero.Firstname), @operator : EqualTo, value : null),
                NoAction,
                (Expression<Func<SuperHero, bool>>)(item => item.Firstname == null)
            };

            yield return new object[]
            {
                new[] {new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" }},
                new Filter(field : nameof(SuperHero.Firstname), @operator : EqualTo, value : "Bruce"),
                NoAction,
                (Expression<Func<SuperHero, bool>>)(item => item.Firstname == "Bruce")
            };

            yield return new object[]
            {
                new[] {new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190 }},
                new Filter(field : nameof(SuperHero.Height), @operator : EqualTo, value : 190),
                NoAction,
                (Expression<Func<SuperHero, bool>>)(item => item.Height == 190 )
            };

            yield return new object[]
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
                new MultiFilter {
                    Logic = Or,
                    Filters = new [] {
                        new Filter(field : nameof(SuperHero.Nickname), @operator : EqualTo, value : "Batman"),
                        new Filter(field : nameof(SuperHero.Nickname), @operator : EqualTo, value : "Superman")
                    }
                },
                NoAction,
                (Expression<Func<SuperHero, bool>>)(item => item.Nickname == "Batman" || item.Nickname == "Superman")
        };

        yield return new object[]
        {
                new[] {
                    new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" },
                    new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                    new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash" }
                },
                new MultiFilter {
                    Logic = And,
                    Filters = new IFilter [] {
                        new Filter(field : nameof(SuperHero.Firstname), @operator : Contains, value : "a"),
                        new MultiFilter
                        {
                            Logic = Or,
                            Filters = new [] {
                                new Filter(field : nameof(SuperHero.Nickname), @operator : EqualTo, value : "Batman"),
                                new Filter(field : nameof(SuperHero.Nickname), @operator : EqualTo, value : "Superman")
                            }
                        }
                    }
                },
                NoAction,
                (Expression<Func<SuperHero, bool>>)(item => item.Firstname.Contains("a") && (item.Nickname == "Batman" || item.Nickname == "Superman"))
            };

                yield return new object[]
                {
                    new[] 
                    {
#if NET6_0_OR_GREATER
                        new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman", LastBattleDate = DateOnly.FromDateTime(25.December(2012)) },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman", LastBattleDate = DateOnly.FromDateTime(13.January(2007))},
                        new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash", LastBattleDate = DateOnly.FromDateTime(18.April(2014)) },
                        new SuperHero { Firstname = "Freak", Lastname = "Hazoid",  Height = 190, Nickname = "Zoom", LastBattleDate = DateOnly.FromDateTime(25.April(2014)), PeakShape = TimeOnly.FromTimeSpan(12.Hours()) },

#else
                        new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman", LastBattleDate = 25.December(2012) },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman", LastBattleDate = 13.January(2007)},
                        new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash", LastBattleDate = 18.April(2014) }  
#endif
                    },
                    new MultiFilter {
                        Logic = And,
                        Filters = new IFilter [] {
                            new Filter(field : nameof(SuperHero.Firstname), @operator : Contains, value : "a"),
                            new MultiFilter
                            {
                                Logic = And,
                                Filters = new [] {
#if NET6_0_OR_GREATER
		                            new Filter(field : nameof(SuperHero.LastBattleDate), @operator : GreaterThan, value : DateOnly.FromDateTime(1.January(2007))),
                                    new Filter(field : nameof(SuperHero.LastBattleDate), @operator : FilterOperator.LessThan, value : DateOnly.FromDateTime(31.December(2012))),
                                    //new Filter(field : nameof(SuperHero.PeakShape), @operator : FilterOperator.LessThan, value : TimeOnly.FromTimeSpan(12.Hours()))
#else
		                            new Filter(field : nameof(SuperHero.LastBattleDate), @operator : GreaterThan, value : 1.January(2007)),
                                    new Filter(field : nameof(SuperHero.LastBattleDate), @operator : FilterOperator.LessThan, value : 31.December(2012))
#endif
                                }
                            }
                        }
                    },
                    NoAction,
                    
#if NET6_0_OR_GREATER
                    (Expression<Func<SuperHero, bool>>)(item => item.Firstname.Contains("a")
                                                                && DateOnly.FromDateTime(1.January(2007)) < item.LastBattleDate
                                                                && item.LastBattleDate < DateOnly.FromDateTime(31.December(2012)))
#else
                    (Expression<Func<SuperHero, bool>>)(item => item.Firstname.Contains("a")
                                                                && 1.January(2007) < item.LastBattleDate
                                                                && item.LastBattleDate < 31.December(2012))

#endif
            };

            yield return new object[]
            {
                    new[]
                    {
#if NET6_0_OR_GREATER
                        new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman", LastBattleDate = DateOnly.FromDateTime(25.December(2012)) },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman", LastBattleDate = DateOnly.FromDateTime(13.January(2007))},
                        new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash", LastBattleDate = DateOnly.FromDateTime(18.April(2014)) }
#else
                        new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman", LastBattleDate = 25.December(2012) },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman", LastBattleDate = 13.January(2007)},
                        new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash", LastBattleDate = 18.April(2014) }  
#endif
                    },
                    new MultiFilter {
                        Logic = And,
                        Filters = new IFilter [] {
                            new Filter(field : nameof(SuperHero.Firstname), @operator : Contains, value : "a"),
                            new MultiFilter
                            {
                                Logic = And,
                                Filters = new [] {
#if NET6_0_OR_GREATER
		                            new Filter(field : nameof(SuperHero.LastBattleDate), @operator : GreaterThan, value : DateOnly.FromDateTime(1.January(2007))),
                                    new Filter(field : nameof(SuperHero.LastBattleDate), @operator : FilterOperator.LessThan, value : DateOnly.FromDateTime(31.December(2012)))
#else
		                            new Filter(field : nameof(SuperHero.LastBattleDate), @operator : GreaterThan, value : 1.January(2007)),
                                    new Filter(field : nameof(SuperHero.LastBattleDate), @operator : FilterOperator.LessThan, value : 31.December(2012))
#endif           
                                }
                            }
                        }
                    },
                    NoAction,
#if NET6_0_OR_GREATER
                    (Expression<Func<SuperHero, bool>>)(item => item.Firstname.Contains("a")
                                                                && DateOnly.FromDateTime(1.January(2007)) < item.LastBattleDate
                                                                && item.LastBattleDate < DateOnly.FromDateTime(31.December(2012)))
#else
                    (Expression<Func<SuperHero, bool>>)(item => item.Firstname.Contains("a")
                                                                && 1.January(2007) < item.LastBattleDate
                                                                && item.LastBattleDate < 31.December(2012))
#endif
            };

            yield return new object[]
            {
                    new[] {
                        new SuperHero {
                            Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman",
                        },
                        new SuperHero
                        {
                            Firstname = "Clark", Lastname = "Kent", Nickname = "Superman",
                            Powers = new [] { "super strength", "heat vision" }
                        }
                    },
                    new Filter(field : nameof(SuperHero.Powers), @operator : EqualTo, value: "heat"),
                    NoAction,
                    (Expression<Func<SuperHero, bool>>)(item => item.Powers.Any(power => power == "heat"))
            };

            yield return new object[]
            {
                    new[] {
                        new SuperHero {
                            Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman",
                            Henchman = new Henchman
                            {
                                Nickname = "Robin",
                                Weapons = new []
                                {
                                    new Weapon{ Name = "stick", Level = 100 }
                                }
                            }
                        }
                    },
                    new Filter(field : @$"{nameof(SuperHero.Henchman)}[""{nameof(Henchman.Weapons)}""][""{nameof(Weapon.Name)}""]",
                               @operator : EqualTo,
                               value: "stick"),
                    NoAction,
                    (Expression<Func<SuperHero, bool>>)(item => item.Henchman.Weapons.Any(weapon => weapon.Name == "stick"))
            };
        }
    }

    [Theory]
    [MemberData(nameof(EqualToTestCases))]
    public void BuildEqual(IEnumerable<SuperHero> superheroes, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<SuperHero, bool>> expression)
        => Build(superheroes, filter, nullableValueBehavior, expression);

    public static IEnumerable<object[]> IsNullTestCases
    {
        get
        {
            yield return new object[]
            {
                    Enumerable.Empty<SuperHero>(),
                    new Filter(field : nameof(SuperHero.Firstname), @operator : IsNull),
                    NoAction,
                    (Expression<Func<SuperHero, bool>>)(item => item.Firstname == null)
            };

            yield return new object[]
            {
                    new[] {
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                        new SuperHero { Firstname = null, Lastname = "", Height = 178, Nickname = "Sinestro" }
                    },
                    new Filter(field : nameof(SuperHero.Firstname), @operator : IsNull),
                    NoAction,
                    (Expression<Func<SuperHero, bool>>)(item => item.Firstname == null),
            };
        }
    }

    [Theory]
    [MemberData(nameof(IsNullTestCases))]
    public void BuildIsNull(IEnumerable<SuperHero> superheroes, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<SuperHero, bool>> expression)
        => Build(superheroes, filter, nullableValueBehavior, expression);

    public static IEnumerable<object[]> IsEmptyTestCases
    {
        get
        {
            yield return new object[]
            {
                    Enumerable.Empty<SuperHero>(),
                    new Filter(field : nameof(SuperHero.Lastname), @operator : IsEmpty),
                    NoAction,
                    (Expression<Func<SuperHero, bool>>)(item => item.Lastname == string.Empty)
            };

            yield return new object[]
            {
                    new[] {
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                        new SuperHero { Firstname = "", Lastname = "", Height = 178, Nickname = "Sinestro" }
                    },
                    new Filter(field : nameof(SuperHero.Lastname), @operator : IsEmpty),
                    NoAction,
                    (Expression<Func<SuperHero, bool>>)(item => item.Lastname == string.Empty),
            };

            yield return new object[]
            {
                    new[] {
                        new SuperHero {
                            Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman",
                            Henchman = new Henchman
                            {
                                Firstname = "Dick", Lastname = "Grayson", Nickname = "Robin"
                            }
                        },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman",
                           Henchman = new Henchman { Nickname = "Krypto" } }
                    },
                    new Filter(field : $@"{nameof(SuperHero.Henchman)}[""{nameof(Henchman.Lastname)}""]", @operator : IsEmpty),
                    NoAction,
                    (Expression<Func<SuperHero, bool>>)(item => item.Henchman.Lastname == string.Empty)
            };

            yield return new object[]
            {
                    new[] {
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
                    },
                    new Filter(field : $@"{nameof(SuperHero.Henchman)}[""{nameof(Henchman.Firstname)}""]", @operator : NotEqualTo, value: "Dick"),
                    NoAction,
                    (Expression<Func<SuperHero, bool>>)(item => item.Henchman.Firstname != "Dick")
            };

            yield return new object[]
            {
                    new[] {
                        new SuperHero {
                            Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman",
                        },
                        new SuperHero
                        {
                            Firstname = "Clark", Lastname = "Kent", Nickname = "Superman",
                            Powers = new [] { "super strength", "heat vision" }
                        }
                    },
                    new Filter(field : nameof(SuperHero.Powers), @operator : IsEmpty),
                    NoAction,
                    (Expression<Func<SuperHero, bool>>)(item => !item.Powers.Any())
            };

            yield return new object[]
            {
                    new[] {
                        new SuperHero {
                            Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman",
                        },
                        new SuperHero
                        {
                            Firstname = "Clark", Lastname = "Kent", Nickname = "Superman",
                            Powers = new [] { "super strength", "heat vision" }
                        }
                    },
                    new Filter(field : $@"{nameof(SuperHero.Acolytes)}[""{nameof(SuperHero.Weapons)}""]", @operator : IsEmpty),
                    NoAction,
                    (Expression<Func<SuperHero, bool>>)(item => item.Acolytes.Any(acolyte => !acolyte.Weapons.Any()))
            };
        }
    }

    [Theory]
    [MemberData(nameof(IsEmptyTestCases))]
    public void BuildIsEmpty(IEnumerable<SuperHero> superheroes, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<SuperHero, bool>> expression)
        => Build(superheroes, filter, nullableValueBehavior, expression);

    public static IEnumerable<object[]> IsNotEmptyTestCases
    {
        get
        {
            yield return new object[]
            {
                    Enumerable.Empty<SuperHero>(),
                    new Filter(field : nameof(SuperHero.Lastname), @operator : IsNotEmpty),
                    NoAction,
                    (Expression<Func<SuperHero, bool>>)(item => item.Lastname != string.Empty)
            };

            yield return new object[]
            {
                    new[] {
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                        new SuperHero { Firstname = "", Lastname = "", Height = 178, Nickname = "Sinestro" }
                    },
                    new Filter(field: nameof(SuperHero.Lastname), @operator: IsNotEmpty),
                    NoAction,
                    (Expression<Func<SuperHero, bool>>)(item => item.Lastname != string.Empty),
            };

            yield return new object[]
            {
                    new[] {
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
                    },
                    new Filter(field: $@"{nameof(SuperHero.Acolytes)}[""{nameof(Henchman.Lastname)}""]", @operator: IsNotEmpty),
                    NoAction,
                    (Expression<Func<SuperHero, bool>>)(item => item.Acolytes.Any(acolyte => acolyte.Lastname != string.Empty))
            };

            yield return new object[]
            {
                    new[] {
                        new SuperHero {
                            Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman",
                        },
                        new SuperHero
                        {
                            Firstname = "Clark", Lastname = "Kent", Nickname = "Superman",
                            Powers = new [] { "super strength", "heat vision" }
                        }
                    },
                    new Filter(field : nameof(SuperHero.Powers), @operator : IsNotEmpty),
                    NoAction,
                    (Expression<Func<SuperHero, bool>>)(item => item.Powers.Any())
            };
        }
    }

    [Theory]
    [MemberData(nameof(IsNotEmptyTestCases))]
    public void BuildIsNotEmpty(IEnumerable<SuperHero> superheroes, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<SuperHero, bool>> expression)
        => Build(superheroes, filter, nullableValueBehavior, expression);

    public static IEnumerable<object[]> StartsWithCases
    {
        get
        {
            yield return new object[]
            {
                    new[] {
                        new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                        new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190 }
                    },
                    new Filter(field : nameof(SuperHero.Nickname), @operator : StartsWith, value: "B"),
                    AddNullCheck,
                    (Expression<Func<SuperHero, bool>>)(item => item != null && item.Nickname != null && item.Nickname.StartsWith("B"))
            };
        }
    }

    [Theory]
    [MemberData(nameof(StartsWithCases))]
    public void BuildStartsWith(IEnumerable<SuperHero> superheroes, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<SuperHero, bool>> expression)
        => Build(superheroes, filter, nullableValueBehavior, expression);

    public static IEnumerable<object[]> NotStartsWithCases
    {
        get
        {
            yield return new object[]
            {
                    new[] {
                        new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                        new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash" }
                    },
                    new Filter(field : nameof(SuperHero.Nickname), @operator : NotStartsWith, value: "B"),
                    NoAction,
                    (Expression<Func<SuperHero, bool>>)(item => item != null && !item.Nickname.StartsWith("B"))
            };
        }
    }

    [Theory]
    [MemberData(nameof(NotStartsWithCases))]
    public void BuildNotStartsWith(IEnumerable<SuperHero> superheroes, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<SuperHero, bool>> expression)
        => Build(superheroes, filter, nullableValueBehavior, expression);

    public static IEnumerable<object[]> EndsWithCases
    {
        get
        {
            yield return new object[]
            {
                    new[]
                    {
                        new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                        new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash" }
                    },
                    new Filter(field: nameof(SuperHero.Nickname), @operator: EndsWith, value:"n"),
                    NoAction,
                    (Expression<Func<SuperHero, bool>>)(item => item.Nickname.EndsWith("n"))
            };
        }
    }

    [Theory]
    [MemberData(nameof(EndsWithCases))]
    public void BuildEndsWith(IEnumerable<SuperHero> superheroes, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<SuperHero, bool>> expression)
        => Build(superheroes, filter, nullableValueBehavior, expression);

    public static IEnumerable<object[]> NotEndsWithCases
    {
        get
        {
            yield return new object[]
            {
                    new[]
                    {
                        new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                        new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash" }
                    },
                    new Filter(field: nameof(SuperHero.Nickname), @operator: NotEndsWith, value:"n"),
                    NoAction,
                    (Expression<Func<SuperHero, bool>>)(item => !item.Nickname.EndsWith("n"))
            };
        }
    }

    [Theory]
    [MemberData(nameof(NotEndsWithCases))]
    public void BuildNotEndsWith(IEnumerable<SuperHero> superheroes, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<SuperHero, bool>> expression)
        => Build(superheroes, filter, nullableValueBehavior, expression);

    public static IEnumerable<object[]> ContainsCases
    {
        get
        {
            yield return new object[]
            {
                    new[]
                    {
                        new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                        new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash" }
                    },
                    new Filter(field:nameof(SuperHero.Nickname), @operator: Contains, value: "an"),
                    NoAction,
                    (Expression<Func<SuperHero, bool>>)(item => item.Nickname.Contains("an")),
            };

            yield return new object[]
            {
                    new[] {
                        new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman", Powers = new[]{ "super strength", "heat vision"} },
                        new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash", Powers = new[]{ "super speed"} }
                    },
                    new Filter(field:nameof(SuperHero.Powers), @operator: Contains, value: "strength"),
                    NoAction,
                    (Expression<Func<SuperHero, bool>>)(item => item.Powers.Any( power => power.Contains("strength")))
            };

            yield return new object[]
            {
                    new[] {
                        new SuperHero {
                            Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman",
                        },
                        new SuperHero
                        {
                            Firstname = "Clark", Lastname = "Kent", Nickname = "Superman",
                            Powers = new [] { "super strength", "heat vision" }
                        },
                        new SuperHero
                        {
                            Firstname = "Barry", Lastname = "Allen", Nickname = "Flash",
                            Powers = new [] { "super speed" }
                        }
                    },
                    new MultiFilter
                    {
                        Logic = Or,
                        Filters = new IFilter[]
                        {
                            new Filter(field : nameof(SuperHero.Powers), @operator : Contains, value: "heat"),
                            new Filter(field : nameof(SuperHero.Powers), @operator : Contains, value: "speed"),
                        }
                    },
                    NoAction,
                    (Expression<Func<SuperHero, bool>>)(item => item.Powers.Any(power => power.Contains("heat") || power.Contains("speed")))
            };
        }
    }

    [Theory]
    [MemberData(nameof(ContainsCases))]
    public void BuildContains(IEnumerable<SuperHero> superheroes, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<SuperHero, bool>> expression)
        => Build(superheroes, filter, nullableValueBehavior, expression);

    public static IEnumerable<object[]> NotContainsCases
    {
        get
        {
            yield return new object[]
            {
                    new[] {
                        new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                        new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash" }
                    },
                    new Filter(field:nameof(SuperHero.Nickname), @operator: NotContains, value: "an"),
                    NoAction,
                    (Expression<Func<SuperHero, bool>>)(item => !item.Nickname.Contains("an"))
            };
        }
    }

    [Theory]
    [MemberData(nameof(NotContainsCases))]
    public void BuildNotContains(IEnumerable<SuperHero> superheroes, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<SuperHero, bool>> expression)
        => Build(superheroes, filter, nullableValueBehavior, expression);

    public static IEnumerable<object[]> LessThanTestCases
    {
        get
        {
            yield return new object[]
            {
                    new[] {
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                        new SuperHero { Firstname = null, Lastname = "", Height = 178, Nickname = "Sinestro" }
                    },
                    new Filter(field : nameof(SuperHero.Height), @operator : FilterOperator.LessThan, value: 150),
                    NoAction,
                    (Expression<Func<SuperHero, bool>>)(item => item.Height < 150),
            };
        }
    }

    [Theory]
    [MemberData(nameof(LessThanTestCases))]
    public void BuildLessThan(IEnumerable<SuperHero> superheroes, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<SuperHero, bool>> expression)
        => Build(superheroes, filter, nullableValueBehavior, expression);

    public static IEnumerable<object[]> NotEqualToTestCases
    {
        get
        {
            yield return new object[]
            {
                    Enumerable.Empty<SuperHero>(),
                    new Filter(field : nameof(SuperHero.Lastname), @operator : NotEqualTo, value : "Kent"),
                    NoAction,
                    (Expression<Func<SuperHero, bool>>)(item => item.Lastname != "Kent")
            };
            yield return new object[]
            {
                    new[] {
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" }
                    },
                    new Filter(field : nameof(SuperHero.Lastname), @operator : NotEqualTo, value : "Kent"),
                    NoAction,
                    (Expression<Func<SuperHero, bool>>)(item => item.Lastname != "Kent")
            };

            yield return new object[]
            {
                    new[] {
                        new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" }
                    },
                    new Filter(field : nameof(SuperHero.Lastname), @operator : NotEqualTo, value : "Kent"),
                    NoAction,
                    (Expression<Func<SuperHero, bool>>)(item => item.Lastname != "Kent")
            };

            yield return new object[]
            {
                    new[] {
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
                    },
                    new Filter(field : $@"{nameof(SuperHero.Henchman)}[""{nameof(Henchman.Firstname)}""]", @operator : NotEqualTo, value : "Dick"),
                    NoAction,
                    (Expression<Func<SuperHero, bool>>)(item => item.Henchman.Firstname != "Dick")
            };
        }
    }

    [Theory]
    [MemberData(nameof(NotEqualToTestCases))]
    public void BuildNotEqual(IEnumerable<SuperHero> superheroes, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<SuperHero, bool>> expression)
        => Build(superheroes, filter, nullableValueBehavior, expression);

    public static IEnumerable<object[]> LessThanOrEqualToCases
    {
        get
        {
            yield return new object[]
            {
#if NET6_0_OR_GREATER
                    new[]
                    {
                        new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman", LastBattleDate = DateOnly.FromDateTime(25.December(2012)) },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman", LastBattleDate = DateOnly.FromDateTime(13.January(2007))},
                        new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash", LastBattleDate = DateOnly.FromDateTime(18.April(2014)) }
                    },
                    new Filter(field : nameof(SuperHero.LastBattleDate), @operator : LessThanOrEqualTo, value : DateOnly.FromDateTime(1.January(2007))),
                    NullableValueBehavior.NoAction,
                    (Expression<Func<SuperHero, bool>>)(item => item.Firstname.Contains("a")
                                                                && item.LastBattleDate <= DateOnly.FromDateTime(1.January(2007)))
#else
                    new[]
                    {
                        new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman", LastBattleDate = 25.December(2012) },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman", LastBattleDate = 13.January(2007)},
                        new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash", LastBattleDate = 18.April(2014) }
                    },
                    new Filter(field : nameof(SuperHero.LastBattleDate), @operator : LessThanOrEqualTo, value : 1.January(2007)),
                    NoAction,
                    (Expression<Func<SuperHero, bool>>)(item => item.Firstname.Contains("a")
                                                                && item.LastBattleDate <= 1.January(2007))
#endif
            };
        }
    }

    [Theory]
    [MemberData(nameof(LessThanOrEqualToCases))]
    public void BuildLessThanOrEqual(IEnumerable<SuperHero> superHeroes, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<SuperHero, bool>> expression)
        => Build(superHeroes, filter, nullableValueBehavior, expression);

    /// <summary>
    /// Tests various filters
    /// </summary>
    /// <param name="elements">Collections of </param>
    /// <param name="filter">filter under test</param>
    /// <param name="expression">Expression the filter should match once built</param>
    private void Build<T>(IEnumerable<T> elements, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<T, bool>> expression)
    {
        _output.WriteLine($"Filtering {elements.Jsonify()}");
        _output.WriteLine($"Filter under test : {filter}");
        _output.WriteLine($"Behavior : {nullableValueBehavior}");
        _output.WriteLine($"Reference expression : {expression.Body}");

        // Act
        Expression<Func<T, bool>> filterExpression = filter.ToExpression<T>(nullableValueBehavior);
        IEnumerable<T> filteredResult = elements
            .Where(filterExpression.Compile())
            .ToList();
        _output.WriteLine($"Current expression : {filterExpression.Body}");

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
        IEnumerable<SuperHero> superHeroes = new[]
        {
                new SuperHero { Firstname = "Bruce", Lastname = "Wayne" },
                new SuperHero { Firstname = "Dick", Lastname = "Grayson" },
                new SuperHero { Firstname = "Diana", Lastname = "Price" },
            };
        IFilter filter = new Filter(field: null, @operator: @operator, value: value);

        // Act
        Expression<Func<SuperHero, bool>> actualExpression = filter.ToExpression<SuperHero>();
        IEnumerable<SuperHero> superHeroesFiltered = superHeroes.Where(actualExpression.Compile());

        // Assert
        superHeroesFiltered.Should()
            .HaveSameCount(superHeroes).And
            .OnlyContain(x => superHeroes.Once(sh => x.Firstname == sh.Firstname && x.Lastname == sh.Lastname));
    }

    public static IEnumerable<object[]> DateTimeOffsetFilterCases
    {
        get
        {
            yield return new object[]
            {
                    new[]
                    {
                        new Appointment{ Name = "Medical appointment", Date = 23.July(2012) },
                        new Appointment{ Name = "Medical appointment", Date = 10.September(2012) },
                    },
                    new Filter(field: nameof(Appointment.Date), GreaterThan, 23.July(2012)),
                    (Expression<Func<Appointment, bool>>)(app => app.Date > 23.July(2012))
            };

            yield return new object[]
            {
                    new[]
                    {
                        new Appointment{ Name = "Medical appointment", Date = 23.July(2012) },
                        new Appointment{ Name = "Medical appointment", Date = 10.September(2012) },
                    },
                    new Filter(field: nameof(Appointment.Date), GreaterThanOrEqual, 23.July(2012)),
                    (Expression<Func<Appointment, bool>>)(app => app.Date >= 23.July(2012))
            };

            yield return new object[]
            {
                    new []
                    {
                        new Appointment { Name = "Medical appointment", Date = 12.April(2013).Add(14.Hours(1.Hours()) )},
                        new Appointment { Name = "Medical appointment", Date = 12.April(2013).Add(14.Hours()) }
                    },
                    new Filter(field : nameof(Appointment.Date), EqualTo, 12.April(2013).Add(14.Hours(1.Hours()))),
                    (Expression<Func<Appointment, bool>>)(app => app.Date == 12.April(2013).Add(14.Hours(1.Hours())))
            };
        }
    }

    [Theory]
    [MemberData(nameof(DateTimeOffsetFilterCases))]
    public void Filter_DateTimeOffset(IEnumerable<Appointment> appointments, IFilter filter, Expression<Func<Appointment, bool>> expression)
    {
        _output.WriteLine($"Filtering {appointments.Jsonify()}");
        _output.WriteLine($"Filter under test : {filter}");
        _output.WriteLine($"Reference expression : {expression.Body}");

        // Act
        Expression<Func<Appointment, bool>> buildResult = filter.ToExpression<Appointment>();
        IEnumerable<Appointment> filteredResult = appointments.Where(buildResult.Compile())
                                                              .ToArray();
        _output.WriteLine($"Current expression : {buildResult.Body}");

        // Assert
        _output.WriteLine($"Filtered result : {filteredResult.Jsonify()}");
        filteredResult.Should()
                      .NotBeNull().And
                      .BeEquivalentTo(appointments?.Where(expression.Compile()));
    }

    public static IEnumerable<object[]> LocalDateFilterCases
    {
        get
        {
            yield return new object[]
            {
                    new[]
                    {
                        new NodaTimeClass{ LocalDate = LocalDate.FromDateTime(19.January(2019)) },
                        new NodaTimeClass{ LocalDate = LocalDate.FromDateTime(21.January(2019)) }
                    },
                    new Filter(nameof(NodaTimeClass.LocalDate), EqualTo, LocalDate.FromDateTime(19.January(2019))),
                    NoAction,
                    (Expression<Func<NodaTimeClass, bool>>)(x => x.LocalDate == LocalDate.FromDateTime(19.January(2019)))
            };
        }
    }

    [Theory]
    [MemberData(nameof(LocalDateFilterCases))]
    public void Filter_LocalDate(IEnumerable<NodaTimeClass> elements, IFilter filter, NullableValueBehavior nullableValueBehavior, Expression<Func<NodaTimeClass, bool>> expression)
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
        Hero[] heroes = new Hero[]
        {
                new()
        };

        Filter filter = new(nameof(Hero.Name), op, value);

        // Act
        Expression<Func<Hero, bool>> expression = filter.ToExpression<Hero>(NoAction);
        Action act = () => heroes.Where(expression.Compile()).ToArray();

        // Assert
        act.Should()
           .Throw<NullReferenceException>();
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
        SuperHero[] heroes = new SuperHero[]
        {
                new() { Acolytes = null}
        };

        Filter filter = new($"{nameof(SuperHero.Acolytes)}.{nameof(SuperHero.Nickname)}", op, value);

        // Act
        Expression<Func<SuperHero, bool>> expression = filter.ToExpression<SuperHero>(AddNullCheck);
        _output.WriteLine($"Computed expression : {expression.Body}");
        Action act = () => heroes.Where(expression.Compile()).ToArray();

        // Assert
        act.Should()
           .NotThrow();
    }
}
