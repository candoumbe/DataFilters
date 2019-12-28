using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using static DataFilters.FilterLogic;
using static DataFilters.FilterOperator;

namespace DataFilters.UnitTests
{
    public class FilterExtensionsTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public FilterExtensionsTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        public class Person
        {
            public string Firstname { get; set; }

            public string Lastname { get; set; }

            public DateTime BirthDate { get; set; }

            public string Nickname { get; set; }

            public int Height { get; set; }
        }

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
                    "Height=[100 TO *]",
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
                    "Height=[* TO 200]",
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

                //yield return new object[]
                //{
                //    "Nickname=Sup*er*man",
                //    new MultiFilter
                //    {
                //        Logic = And,
                //        Filters = new IFilter[]
                //        {
                //            new Filter("Nickname", StartsWith, "Sup"),
                //            new Filter("Nickname", Contains, "er"),
                //            new Filter("Nickname", EndsWith, "man"),
                //        }
                //    }
                //};

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
            }
        }

        [Theory]
        [MemberData(nameof(ToFilterCases))]
        public void ToFilter(string filter, IFilter expected)
        {
            _outputHelper.WriteLine($"{nameof(filter)} : '{filter}'");

            // Act
            IFilter actual = filter.ToFilter<Person>();

            // Assert
            actual.Should()
                .NotBeSameAs(expected).And
                .Be(expected);
        }
    }
}