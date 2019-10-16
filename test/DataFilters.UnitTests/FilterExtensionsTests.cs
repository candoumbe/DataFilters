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

            public int Age { get; set; }
        }

        public static IEnumerable<object[]> ToFilterCases
        {
            get
            {
                yield return new object[]
                {
                    $"{nameof(Person.Firstname)}=Hal&{nameof(Person.Lastname)}=Jordan",
                    new CompositeFilter{
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
                    new CompositeFilter
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
                    $"{nameof(Person.Firstname)}=V*|G*,*l|*s",
                    new CompositeFilter
                    {
                        Logic = And,
                        Filters = new IFilter[]
                        {
                            new CompositeFilter
                            {
                                Logic = Or,
                                Filters = new IFilter[]
                                {
                                    new Filter(field: nameof(Person.Firstname), @operator: StartsWith, value: "V"),
                                    new Filter(field: nameof(Person.Firstname), @operator: StartsWith, value: "G")
                                }
                            },
                            new CompositeFilter
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
                .Be(expected)
                ;
        }
    }
}
