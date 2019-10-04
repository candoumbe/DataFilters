using FluentAssertions;
using Queries.Core.Parts.Clauses;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace DataFilters.Queries.UnitTests
{
    public class FilterToQueriesTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public FilterToQueriesTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        public static IEnumerable<object[]> FilterToWhereCases
        {
            get
            {
                yield return new object[]
                {
                    new Filter(field: "Name",@operator: FilterOperator.EqualTo, value: "Wayne"),
                    new WhereClause(column:  "Name".Field(), @operator: ClauseOperator.EqualTo, constraint: "Wayne")
                };

                yield return new object[]
                {
                    new Filter(field: "Name",@operator: FilterOperator.NotEqualTo, value: "Wayne"),
                    new WhereClause(column:  "Name".Field(), @operator: ClauseOperator.NotEqualTo, constraint: "Wayne")
                };

                yield return new object[]
                {
                    new Filter(field: "Name",@operator: FilterOperator.Contains, value: "W"),
                    new WhereClause(column:  "Name".Field(), @operator: ClauseOperator.Like, constraint: "%W%")
                };

                yield return new object[]
                {
                    new Filter(field: "Name",@operator: FilterOperator.EndsWith, value: "W"),
                    new WhereClause(column:  "Name".Field(), @operator: ClauseOperator.Like, constraint: "%W")
                };

                yield return new object[]
                {
                    new Filter(field: "Name",@operator: FilterOperator.NotEndsWith, value: "W"),
                    new WhereClause(column:  "Name".Field(), @operator: ClauseOperator.NotLike, constraint: "%W")
                };
                yield return new object[]
                {
                    new CompositeFilter
                    {
                        Logic = FilterLogic.Or,
                        Filters = new []{
                            new Filter(field: "Name",@operator: FilterOperator.EndsWith, value: "man"),
                            new Filter(field: "Name",@operator: FilterOperator.StartsWith, value: "Bat")
                        }
                    },
                    new CompositeWhereClause
                    {
                        Logic = ClauseLogic.Or,
                        Clauses = new []{
                            new WhereClause("Name".Field(),@operator: ClauseOperator.Like, constraint: "%man"),
                            new WhereClause("Name".Field(),@operator: ClauseOperator.Like, constraint: "Bat%")
                        }
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(FilterToWhereCases))]
        public void FilterToWhere(IFilter filter, IWhereClause expected)
        {
            _outputHelper.WriteLine($"Filter : {filter.Jsonify()}");
            _outputHelper.WriteLine($"Expected : {expected.Jsonify()}");

            // Act
            IWhereClause actualWhere= filter.ToWhere();
            _outputHelper.WriteLine($"actualQuery : {actualWhere.Jsonify()}");

            // Assert
            actualWhere.Should()
                .Be(expected);
        }
    }
}
