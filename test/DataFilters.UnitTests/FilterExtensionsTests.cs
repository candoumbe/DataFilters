namespace DataFilters.UnitTests
{
    using DataFilters.Casing;
    using DataFilters.TestObjects;

    using FluentAssertions;
    using FluentAssertions.Common;
    using FluentAssertions.Extensions;

    using System;
    using System.Collections.Generic;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    using static DataFilters.FilterLogic;
    using static DataFilters.FilterOperator;

    [UnitTest]
    public class FilterExtensionsTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public FilterExtensionsTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        public static IEnumerable<object[]> ToFilterCases
        {
            get
            {
                yield return new object[]
                {
                    $"{nameof(Person.Firstname)}=Hal&{nameof(Person.Lastname)}=Jordan",
                    new MultiFilter{
                        Logic = And,
                        Filters = new IFilter[]
                        {
                            new Filter(nameof(Person.Firstname), EqualTo, "Hal"),
                            new Filter(nameof(Person.Lastname), EqualTo, "Jordan")
                        }
                    }
                };

                yield return new object[]
                {
                        "",
                        new Filter(field: null, @operator: EqualTo)
                };

                yield return new object[]
                {
                        "",
                        Filter.True
                };

                yield return new object[]
                {
                    $"{nameof(Person.Firstname)}=!*wayne",
                    new Filter(field: nameof(Person.Firstname), @operator: NotEndsWith, value: "wayne")
                };

                yield return new object[]
                {
                    $"{nameof(Person.Firstname)}=Vandal|Gengis",
                    new MultiFilter
                    {
                        Logic = Or,
                        Filters = new IFilter[]
                        {
                            new Filter(field: nameof(Person.Firstname), @operator: EqualTo, value: "Vandal"),
                            new Filter(field: nameof(Person.Firstname), @operator: EqualTo, value: "Gengis")
                        }
                    }
                };

                yield return new object[]
                {
                    $"{nameof(Person.Firstname)}=(V*|G*),(*l|*s)",
                    new MultiFilter
                    {
                        Logic = And,
                        Filters = new IFilter[]
                        {
                            new MultiFilter
                            {
                                Logic = Or,
                                Filters = new IFilter[]
                                {
                                    new Filter(field: nameof(Person.Firstname), @operator: StartsWith, value: "V"),
                                    new Filter(field: nameof(Person.Firstname), @operator: StartsWith, value: "G")
                                }
                            },
                            new MultiFilter
                            {
                                Logic = Or,
                                Filters = new IFilter[]
                                {
                                    new Filter(field: nameof(Person.Firstname), @operator: EndsWith, value: "l"),
                                    new Filter(field: nameof(Person.Firstname), @operator: EndsWith, value: "s")
                                }
                            }
                        }
                    }
                };

                yield return new object[]
               {
                    string.Empty,
                    Filter.True
               };

                yield return new object[]
                {
                    "Firstname=Bruce",
                    new Filter("Firstname", EqualTo, "Bruce")
                };

                yield return new object[]
                {
                    "Firstname=!Bruce",
                    new Filter("Firstname", NotEqualTo, "Bruce")
                };

                yield return new object[]
                {
                    "Firstname=!!Bruce",
                    new Filter("Firstname", EqualTo, "Bruce")
                };

                yield return new object[]
                {
                    "Firstname=Bruce|Dick",
                    new MultiFilter
                    {
                        Logic = Or,
                        Filters = new IFilter[]
                        {
                            new Filter("Firstname", EqualTo, "Bruce"),
                            new Filter("Firstname", EqualTo, "Dick")
                        }
                    }
                };

                yield return new object[]
                {
                    "Firstname=Bru*",
                    new Filter("Firstname", StartsWith, "Bru")
                };

                yield return new object[]
                {
                    "Firstname=*Bru",
                    new Filter("Firstname", EndsWith, "Bru")
                };

                yield return new object[]
                {
                    "Height=[100 TO *[",
                    new Filter("Height", GreaterThanOrEqual, 100)
                };

                yield return new object[]
                {
                    "Height=[100 TO 200]",
                    new MultiFilter
                    {
                        Logic = And,
                        Filters = new IFilter[]
                        {
                            new Filter("Height", GreaterThanOrEqual, 100),
                            new Filter("Height", LessThanOrEqualTo, 200)
                        }
                    }
                };

                yield return new object[]
                {
                    "Height=]100 TO 200[",
                    new MultiFilter
                    {
                        Logic = And,
                        Filters = new IFilter[]
                        {
                            new Filter("Height", GreaterThan, 100),
                            new Filter("Height", FilterOperator.LessThan, 200)
                        }
                    }
                };

                yield return new object[]
                {
                    "Height=]* TO 200]",
                     new Filter("Height", LessThanOrEqualTo, 200)
                };

                yield return new object[]
                {
                    "Nickname=Bat*,*man",
                    new MultiFilter
                    {
                        Logic = And,
                        Filters = new IFilter[]
                        {
                            new Filter("Nickname", StartsWith, "Bat"),
                            new Filter("Nickname", EndsWith, "man"),
                        }
                    }
                };

                yield return new object[]
                {
                    "Nickname=Bat*man",
                    new MultiFilter
                    {
                        Logic = And,
                        Filters = new IFilter[]
                        {
                            new Filter("Nickname", StartsWith, "Bat"),
                            new Filter("Nickname", EndsWith, "man"),
                        }
                    }
                };

                yield return new object[]
                {
                    "Nickname=Bat*,*man*",
                     new MultiFilter
                    {
                        Logic = And,
                        Filters = new IFilter[]
                        {
                            new Filter("Nickname", StartsWith, "Bat"),
                            new Filter("Nickname", Contains, "man"),
                        }
                    }
                };

                yield return new object[]
                {
                    "Firstname=!Bru*",
                    new Filter("Firstname", NotStartsWith, "Bru")
                };

                yield return new object[]
                {
                    "Nickname=!(Bat*man)",
                    new MultiFilter
                    {
                        Logic = Or,
                        Filters = new IFilter[]
                        {
                            new Filter("Nickname", NotStartsWith, "Bat"),
                            new Filter("Nickname", NotEndsWith, "man"),
                        }
                    }
                };

                yield return new object[]
                {
                    "Firstname=Bru*&Lastname=Wayne",
                    new MultiFilter
                    {
                        Logic = And,
                        Filters = new IFilter[]
                        {
                            new Filter("Firstname", StartsWith, "Bru"),
                            new Filter("Lastname", EqualTo, "Wayne"),
                        }
                    }
                };

                yield return new object[]
                {
                    "Firstname=Br[uU]ce",
                    new MultiFilter
                    {
                        Logic = Or,
                        Filters = new IFilter[]
                        {
                            new Filter("Firstname", EqualTo, "Bruce"),
                            new Filter("Firstname", EqualTo, "BrUce"),
                        }
                    }
                };

                yield return new object[]
                {
                    "Firstname=*Br[uU]",
                    new MultiFilter
                    {
                        Logic = Or,
                        Filters = new IFilter[]
                        {
                            new Filter("Firstname", EndsWith, "Bru"),
                            new Filter("Firstname", EndsWith, "BrU"),
                        }
                    }
                };

                yield return new object[]
                {
                    "Firstname=!(Bruce|Wayne)",
                    new MultiFilter
                    {
                        Logic = And,
                        Filters = new IFilter[]
                        {
                            new Filter("Firstname", NotEqualTo, "Bruce"),
                            new Filter("Firstname", NotEqualTo, "Wayne")
                        }
                    }
                };

                yield return new object[]
                {
                    "Firstname=(Bruce|Wayne)",
                    new MultiFilter
                    {
                        Logic = Or,
                        Filters = new IFilter[]
                        {
                            new Filter("Firstname", EqualTo, "Bruce"),
                            new Filter("Firstname", EqualTo, "Wayne")
                        }
                    }
                };

                yield return new object[]
                {
                    "Firstname=(Bat*|Sup*)|(*man|*er)",
                    new MultiFilter
                    {
                        Logic = Or,
                        Filters = new IFilter[]
                        {
                            new MultiFilter
                            {
                                Logic = Or,
                                Filters = new IFilter[]
                                {
                                    new Filter("Firstname", StartsWith, "Bat"),
                                    new Filter("Firstname", StartsWith, "Sup"),
                                }
                            },
                            new MultiFilter
                            {
                                Logic = Or,
                                Filters = new IFilter[]
                                {
                                    new Filter("Firstname", EndsWith, "man"),
                                    new Filter("Firstname", EndsWith, "er"),
                                }
                            },
                        }
                    }
                };

                yield return new object[]
                {
                    @"BattleCry=*\!",
                    new Filter(field: "BattleCry", @operator: EndsWith, "!")
                };

                yield return new object[]
                {
                    @"BattleCry=\**",
                    new Filter(field: "BattleCry", @operator: StartsWith, "*")
                };

                yield return new object[]
                {
                    "BirthDate=]2016-10-18 TO 2016-10-25[",
                    new MultiFilter
                    {
                        Logic = And,
                        Filters = new IFilter[]
                        {
#if NET6_0_OR_GREATER
		                    new Filter(nameof(SuperHero.BirthDate), GreaterThan, DateOnly.FromDateTime(18.October(2016))),
                            new Filter(nameof(SuperHero.BirthDate), FilterOperator.LessThan, DateOnly.FromDateTime(25.October(2016)))
#else
                            new Filter(nameof(SuperHero.BirthDate), GreaterThan, 18.October(2016)),
                            new Filter(nameof(SuperHero.BirthDate), FilterOperator.LessThan, 25.October(2016))
#endif
                        }
                    }
                };

                yield return new object[]
                {
                    "BirthDate=]2016-10-18T18:00:00 TO 2016-10-25T19:00:00[",
                    new MultiFilter
                    {
                        Logic = And,
                        Filters = new IFilter[]
                        {
#if NET6_0_OR_GREATER
                            new Filter(nameof(SuperHero.BirthDate), GreaterThan, DateOnly.FromDateTime(18.October(2016))),
                            new Filter(nameof(SuperHero.BirthDate), FilterOperator.LessThan, DateOnly.FromDateTime(25.October(2016)))
#else
                            new Filter(nameof(SuperHero.BirthDate), GreaterThan, 18.October(2016).Add(18.Hours())),
                            new Filter(nameof(SuperHero.BirthDate), FilterOperator.LessThan, 25.October(2016).Add(19.Hours()))
#endif
                        }
                    }
                };

                yield return new object[]
                {
                    "DateTimeWithOffset=]2016-10-18T18:00:00Z TO 2016-10-18T23:00:00-02:00[",
                    new MultiFilter
                    {
                        Logic = And,
                        Filters = new IFilter[]
                        {
                            new Filter("DateTimeWithOffset", GreaterThan, 18.October(2016).Add(18.Hours()).ToDateTimeOffset()),
                            new Filter("DateTimeWithOffset", FilterOperator.LessThan,  18.October(2016).Add(23.Hours()).ToDateTimeOffset(-2.Hours()))
                        }
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(ToFilterCases))]
        public void ToFilter(string filter, IFilter expected)
        {
            _outputHelper.WriteLine($"{nameof(filter)} : '{filter}'");

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

        public static IEnumerable<object[]> ToFilterWithPropertyNameResolutionStrategyCases
        {
            get
            {
                yield return new object[]
                {
                    "SnakeCaseProperty=10",
                    PropertyNameResolutionStrategy.SnakeCase,
                    new Filter("snake_case_property", EqualTo, "10")
                };

                yield return new object[]
                {
                    "pascal_case_property=10",
                    PropertyNameResolutionStrategy.PascalCase,
                    new Filter("PascalCaseProperty", EqualTo, "10")
                };
            }
        }

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
    }
}