namespace DataFilters.UnitTests
{
    using System;
    using DataFilters;
    using DataFilters.Casing;
    using DataFilters.TestObjects;
    using FluentAssertions;
    using FluentAssertions.Common;
    using FluentAssertions.Extensions;
    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;
    using static DataFilters.FilterLogic;
    using static DataFilters.FilterOperator;

    [UnitTest]
    public class FilterExtensionsTests(ITestOutputHelper outputHelper)
    {
        public static TheoryData<string, IFilter> ToFilterCases
        => new()
                {
                    {
                        $"{nameof(Person.Firstname)}=Hal&{nameof(Person.Lastname)}=Jordan",
                        new MultiFilter
                        {
                            Logic = And,
                            Filters =
                            [
                                new Filter(nameof(Person.Firstname), EqualTo, "Hal"),
                            new Filter(nameof(Person.Lastname), EqualTo, "Jordan")
                            ]
                        }
                    },

                    {
                        string.Empty,
                        new Filter(field: null, @operator: EqualTo)
                    },
                    {
                        string.Empty,
                        Filter.True
                    },
                    {
                        $"{nameof(Person.Firstname)}=!*wayne",
                        new Filter(field: nameof(Person.Firstname), @operator: NotEndsWith, value: "wayne")
                    },
                    {
                        $"{nameof(Person.Firstname)}=Vandal|Genghis",
                        new MultiFilter
                        {
                            Logic = Or,
                            Filters =
                            [
                                new Filter(field: nameof(Person.Firstname), @operator: EqualTo, value: "Vandal"),
                            new Filter(field: nameof(Person.Firstname), @operator: EqualTo, value: "Genghis")
                            ]
                        }
                    },
                    {
                        $"{nameof(Person.Firstname)}=(V*|G*),(*l|*s)",
                        new MultiFilter
                        {
                            Logic = And,
                            Filters =
                            [
                                new MultiFilter
                            {
                                Logic = Or,
                                Filters =
                                [
                                    new Filter(field: nameof(Person.Firstname), @operator: StartsWith, value: "V"),
                                    new Filter(field: nameof(Person.Firstname), @operator: StartsWith, value: "G")
                                ]
                            },
                            new MultiFilter
                            {
                                Logic = Or,
                                Filters =
                                [
                                    new Filter(field: nameof(Person.Firstname), @operator: EndsWith, value: "l"),
                                    new Filter(field: nameof(Person.Firstname), @operator: EndsWith, value: "s")
                                ]
                            }
                            ]
                        }
                    },

                    {
                        string.Empty,
                        Filter.True
                    },

                    {
                        "Firstname=Bruce",
                        new Filter("Firstname", EqualTo, "Bruce")
                    },

                    {
                        "Firstname=!Bruce",
                        new Filter("Firstname", NotEqualTo, "Bruce")
                    },

                    {
                        "Firstname=!!Bruce",
                        new Filter("Firstname", EqualTo, "Bruce")
                    },

                    {
                        "Firstname=Bruce|Dick",
                        new MultiFilter
                        {
                            Logic = Or,
                            Filters =
                            [
                                new Filter("Firstname", EqualTo, "Bruce"),
                            new Filter("Firstname", EqualTo, "Dick")
                            ]
                        }
                    },

                    {
                        "Firstname=Bru*",
                        new Filter("Firstname", StartsWith, "Bru")
                    },

                    {
                        "Firstname=*Bru",
                        new Filter("Firstname", EndsWith, "Bru")
                    },

                    {
                        "Height=[100 TO *[",
                        new Filter("Height", GreaterThanOrEqual, 100)
                    },

                    {
                        "Height=[100 TO 200]",
                        new MultiFilter
                        {
                            Logic = And,
                            Filters =
                            [
                                new Filter("Height", GreaterThanOrEqual, 100),
                            new Filter("Height", LessThanOrEqualTo, 200)
                            ]
                        }
                    },

                    {
                        "Height=]100 TO 200[",
                        new MultiFilter
                        {
                            Logic = And,
                            Filters =
                            [
                                new Filter("Height", GreaterThan, 100),
                            new Filter("Height", FilterOperator.LessThan, 200)
                            ]
                        }
                    },

                    {
                        "Height=]* TO 200]",
                        new Filter("Height", LessThanOrEqualTo, 200)
                    },

                    {
                        "Nickname=Bat*,*man",
                        new MultiFilter
                        {
                            Logic = And,
                            Filters =
                            [
                                new Filter("Nickname", StartsWith, "Bat"),
                            new Filter("Nickname", EndsWith, "man")
                            ]
                        }
                    },

                    {
                        "Nickname=Bat*man",
                        new MultiFilter
                        {
                            Logic = And,
                            Filters =
                            [
                                new Filter("Nickname", StartsWith, "Bat"),
                            new Filter("Nickname", EndsWith, "man")
                            ]
                        }
                    },

                    {
                        "Nickname=Bat*,*man*",
                        new MultiFilter
                        {
                            Logic = And,
                            Filters =
                            [
                                new Filter("Nickname", StartsWith, "Bat"),
                            new Filter("Nickname", Contains, "man")
                            ]
                        }
                    },

                    {
                        "Firstname=!Bru*",
                        new Filter("Firstname", NotStartsWith, "Bru")
                    },

                    {
                        "Nickname=!(Bat*man)",
                        new MultiFilter
                        {
                            Logic = Or,
                            Filters =
                            [
                                new Filter("Nickname", NotStartsWith, "Bat"),
                            new Filter("Nickname", NotEndsWith, "man")
                            ]
                        }
                    },

                    {
                        "Firstname=Bru*&Lastname=Wayne",
                        new MultiFilter
                        {
                            Logic = And,
                            Filters =
                            [
                                new Filter("Firstname", StartsWith, "Bru"),
                            new Filter("Lastname", EqualTo, "Wayne")
                            ]
                        }
                    },

                    {
                        "Firstname=Br[uU]ce",
                        new MultiFilter
                        {
                            Logic = Or,
                            Filters =
                            [
                                new Filter("Firstname", EqualTo, "Bruce"),
                            new Filter("Firstname", EqualTo, "BrUce")
                            ]
                        }
                    },

                    {
                        "Firstname=*Br[uU]",
                        new MultiFilter
                        {
                            Logic = Or,
                            Filters =
                            [
                                new Filter("Firstname", EndsWith, "Bru"),
                                new Filter("Firstname", EndsWith, "BrU"),
                            ]
                        }
                    },

                    {
                        "Firstname=!(Bruce|Wayne)",
                        new MultiFilter
                        {
                            Logic = And,
                            Filters =
                            [
                                new Filter("Firstname", NotEqualTo, "Bruce"),
                                new Filter("Firstname", NotEqualTo, "Wayne")
                            ]
                        }
                    },

                    {
                        "Firstname=(Bruce|Wayne)",
                        new MultiFilter
                        {
                            Logic = Or,
                            Filters =
                            [
                                new Filter("Firstname", EqualTo, "Bruce"),
                                new Filter("Firstname", EqualTo, "Wayne")
                            ]
                        }
                    },

                    {
                        "Firstname=(Bat*|Sup*)|(*man|*er)",
                        new MultiFilter
                        {
                            Logic = Or,
                            Filters =
                            [
                                new MultiFilter
                            {
                                Logic = Or,
                                Filters =
                                [
                                    new Filter("Firstname", StartsWith, "Bat"),
                                    new Filter("Firstname", StartsWith, "Sup")
                                ]
                            },
                            new MultiFilter
                            {
                                Logic = Or,
                                Filters =
                                [
                                    new Filter("Firstname", EndsWith, "man"),
                                    new Filter("Firstname", EndsWith, "er")
                                ]
                            }
                            ]
                        }
                    },

                    {
                        @"BattleCry=*\!",
                        new Filter(field: "BattleCry", @operator: EndsWith, "!")
                    },

                    {
                        @"BattleCry=\**",
                        new Filter(field: "BattleCry", @operator: StartsWith, "*")
                    },

                    {
                        "BirthDate=]2016-10-18 TO 2016-10-25[",
                        new MultiFilter
                        {
                            Logic = And,
                            Filters =
                            [
                                new Filter(nameof(SuperHero.BirthDate), GreaterThan, DateOnly.FromDateTime(18.October(2016))),
                                new Filter(nameof(SuperHero.BirthDate), FilterOperator.LessThan, DateOnly.FromDateTime(25.October(2016)))
                            ]
                        }
                    },

                    {
                        "BirthDate=]2016-10-18T18:00:00 TO 2016-10-25T19:00:00[",
                        new MultiFilter
                        {
                            Logic = And,
                            Filters =
                            [
                                new Filter(nameof(SuperHero.BirthDate), GreaterThan, DateOnly.FromDateTime(18.October(2016))), 
                                new Filter(nameof(SuperHero.BirthDate), FilterOperator.LessThan, DateOnly.FromDateTime(25.October(2016)))
                            ]
                        }
                    },

                    {
                        "DateTimeWithOffset=]2016-10-18T18:00:00Z TO 2016-10-18T23:00:00-02:00[",
                        new MultiFilter
                        {
                            Logic = And,
                            Filters =
                            [
                                new Filter("DateTimeWithOffset", GreaterThan, 18.October(2016).Add(18.Hours()).ToDateTimeOffset()),
                                new Filter("DateTimeWithOffset", FilterOperator.LessThan, 18.October(2016).Add(23.Hours()).ToDateTimeOffset(-2.Hours()))
                            ]
                        }
                    },

                    {
                        "Nickname={Bat|Sup|Wonder}*,*m[ae]n",
                        new MultiFilter
                        {
                            Logic = And,
                            Filters =
                            [
                                new MultiFilter
                                {
                                    Logic = Or,
                                    Filters =
                                    [
                                        new Filter(nameof(SuperHero.Nickname), StartsWith, "Bat"),
                                        new Filter(nameof(SuperHero.Nickname), StartsWith, "Sup"),
                                        new Filter(nameof(SuperHero.Nickname), StartsWith, "Wonder")
                                    ]
                                },
                                new MultiFilter
                                {
                                    Logic = Or,
                                    Filters =
                                    [
                                        new Filter(nameof(SuperHero.Nickname), EndsWith, "man"),
                                        new Filter(nameof(SuperHero.Nickname), EndsWith, "men")
                                    ]
                                }
                            ]
                        }
                    },
                };

        [Theory]
        [MemberData(nameof(ToFilterCases))]
        public void ToFilter(string filter, IFilter expected)
        {
            outputHelper.WriteLine($"{nameof(filter)} : '{filter}'");

            // Act
            IFilter actual = filter.ToFilter<SuperHero>();

            // Assert
            actual.Should()
                  .NotBeSameAs(expected).And
                  .Be(expected);
        }

        [Fact]
        public void Given_null_ToFilter_should_throw_ArgumentNullException()
        {
            // Act
            Action action = () => ((string)null).ToFilter<Person>();

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        public static TheoryData<string, PropertyNameResolutionStrategy, Filter> ToFilterWithPropertyNameResolutionStrategyCases
            => new()
                {
                    {
                        "SnakeCaseProperty=10",
                        PropertyNameResolutionStrategy.SnakeCase,
                        new Filter("snake_case_property", EqualTo, "10")
                    },
                    {
                        "pascal_case_property=10",
                        PropertyNameResolutionStrategy.PascalCase,
                        new Filter("PascalCaseProperty", EqualTo, "10")
                    }
                };

        [Theory]
        [MemberData(nameof(ToFilterWithPropertyNameResolutionStrategyCases))]
        public void Given_input_and_casing_resolution_strategy_ToFilter_should_create_corresponding_filter(string filter, PropertyNameResolutionStrategy propertyNameResolutionStrategy, IFilter expected)
        {
            // Act
            IFilter actual = filter.ToFilter<Model>(propertyNameResolutionStrategy);

            // Assert
            actual.Should()
                  .Be(expected);
        }

        public static TheoryData<string, FilterOptions, MultiFilter> ToFilterWithFilterOptionsCases
        {
            get
            {
                FilterLogic[] logics = [And, Or];
                TheoryData<string, FilterOptions, MultiFilter> cases = [];
                foreach (FilterLogic logic in logics)
                {
                    cases.Add
                    (
                        "snake_case_property=10&PascalCaseProperty=value",
                        new FilterOptions { Logic = logic },
                        new MultiFilter
                        {
                            Logic = logic,
                            Filters =
                            [
                                new Filter("snake_case_property", EqualTo, "10"),
                                new Filter("PascalCaseProperty", EqualTo, "value")
                            ]
                        }
                    );

                    cases.Add
                    (
                        "snake_case_property=10&PascalCaseProperty=value",
                        new FilterOptions { Logic = logic },
                        new MultiFilter
                        {
                            Logic = logic,
                            Filters =
                            [
                                new Filter("snake_case_property", EqualTo, "10"),
                                new Filter("PascalCaseProperty", EqualTo, "value")
                            ]
                        }
                    );
                }

                return cases;
            }
        }

        [Theory]
        [MemberData(nameof(ToFilterWithFilterOptionsCases))]
        public void Given_input_and_FilterOptions_ToFilter_should_create_corresponding_filter(string filter, FilterOptions options, IFilter expected)
        {
            // Act
            IFilter actual = filter.ToFilter<Model>(options);

            // Assert
            actual.Should()
                  .Be(expected);
        }
    }
}