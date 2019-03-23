using FluentAssertions;
using Queries.Core.Parts.Clauses;
using Sorting = Queries.Core.Parts.Sorting;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using Queries.Core.Parts.Sorting;

namespace DataFilters.Queries.UnitTests
{
    public class SortToQueriesTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public SortToQueriesTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        public class Person
        {
            public string Firstname { get; set; }

            public string Lastname { get; set; }
        }

        public static IEnumerable<object[]> SortToSortCases
        {
            get
            {
                yield return new object[]
                {
                    new Sort<Person>("Name"),
                    new OrderExpression(column : "Name".Field())
                };
            }
        }

        [Theory]
        [MemberData(nameof(SortToSortCases))]
        public void FilterToSort(ISort<Person> sort, IOrder expected)
        {
            _outputHelper.WriteLine($"Sort : {sort.Stringify()}");
            _outputHelper.WriteLine($"Expected : {expected.Stringify()}");

            // Act
            IOrder actual = sort.ToOrder();
            _outputHelper.WriteLine($"actual : {actual.Stringify()}");

            // Assert
            actual.Should()
                .Be(expected);

        }
    }
}
