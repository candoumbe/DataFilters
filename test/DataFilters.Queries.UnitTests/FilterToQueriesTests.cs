namespace DataFilters.Queries.UnitTests;

using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Extensions;
using global::Queries.Core.Parts.Clauses;
using Xunit;
using Xunit.Abstractions;
using static DataFilters.FilterOperator;

public class FilterToQueriesTests(ITestOutputHelper outputHelper)
{
    public static IEnumerable<object[]> FilterToWhereCases
    {
        get
        {
            yield return new object[]
            {
                new Filter(field: "Name",@operator: EqualTo, value: "Wayne"),
                new WhereClause(column: "Name".Field(), @operator: ClauseOperator.EqualTo, constraint: "Wayne")
            };

            yield return new object[]
            {
                new Filter(field: "Name",@operator: NotEqualTo, value: "Wayne"),
                new WhereClause(column: "Name".Field(), @operator: ClauseOperator.NotEqualTo, constraint: "Wayne")
            };

            yield return new object[]
            {
                new Filter(field: "Name",@operator: Contains, value: "W"),
                new WhereClause(column: "Name".Field(), @operator: ClauseOperator.Like, constraint: "%W%")
            };

            yield return new object[]
            {
                new Filter(field: "Name",@operator: EndsWith, value: "W"),
                new WhereClause(column: "Name".Field(), @operator: ClauseOperator.Like, constraint: "%W")
            };

            yield return new object[]
            {
                new Filter(field: "Name",@operator: NotEndsWith, value: "W"),
                new WhereClause(column: "Name".Field(), @operator: ClauseOperator.NotLike, constraint: "%W")
            };
            yield return new object[]
            {
                new MultiFilter
                {
                    Logic = FilterLogic.Or,
                    Filters = new []{
                        new Filter(field: "Name",@operator: EndsWith, value: "man"),
                        new Filter(field: "Name",@operator: StartsWith, value: "Bat")
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

            yield return new object[]
            {
                new Filter(field: "Name", @operator: EndsWith, "*"),
                new WhereClause(column: "Name".Field(), @operator: ClauseOperator.Like, "%*"),
            };

            yield return new object[]
            {
                new Filter(field: "Superpower", IsNull),
                new WhereClause(column: "Superpower".Field(), @operator: ClauseOperator.IsNull)
            };

            yield return new object[]
            {
                new Filter(field: "Superpower", IsNotNull),
                new WhereClause(column: "Superpower".Field(), @operator: ClauseOperator.IsNotNull)
            };

            yield return new object[]
            {
                new Filter(field: "Nickname", NotStartsWith, "Bat"),
                new WhereClause(column: "Nickname".Field(), ClauseOperator.NotLike, "Bat%")
            };

            yield return new object[]
            {
                new Filter(field: "Nickname", NotContains, "man"),
                new WhereClause(column: "Nickname".Field(), ClauseOperator.NotLike, "%man%")
            };

            yield return new object[]
            {
                new Filter(field: "XP", FilterOperator.LessThan, 10),
                new WhereClause("XP".Field(), ClauseOperator.LessThan, 10)
            };

            yield return new object[]
            {
                new Filter(field: "XP", LessThanOrEqualTo, 10),
                new WhereClause("XP".Field(), ClauseOperator.LessThanOrEqualTo, 10)
            };

            yield return new object[]
            {
                new Filter(field : "Height", GreaterThan, 6.4),
                new WhereClause("Height".Field(), ClauseOperator.GreaterThan, 6.4.Literal())
            };

            yield return new object[]
            {
                new Filter(field : "Height", GreaterThan, 6m),
                new WhereClause("Height".Field(), ClauseOperator.GreaterThan, 6m)
            };

            yield return new object[]
            {
                new Filter(field : "Height", GreaterThanOrEqual, 6),
                new WhereClause("Height".Field(), ClauseOperator.GreaterThanOrEqualTo, 6)
            };

            yield return new object[]
            {
                new Filter("BirthDate", GreaterThan, 18.January(1983)),
                new WhereClause("BirthDate".Field(), ClauseOperator.GreaterThan, 18.January(1983))
            };

#if NET6_0_OR_GREATER
            yield return new object[]
            {
                new Filter("BirthDate", GreaterThan, DateOnly.FromDateTime(18.January(1983))),
                new WhereClause("BirthDate".Field(), ClauseOperator.GreaterThan, DateOnly.FromDateTime(18.January(1983)))
            };

            yield return new object[]
            {
                new Filter("Time", GreaterThan, TimeOnly.FromDateTime(18.January(1983).Add(15.Hours().And(47.Minutes())))),
                new WhereClause("Time".Field(), ClauseOperator.GreaterThan, TimeOnly.FromDateTime(18.January(1983).Add(15.Hours().And(47.Minutes()))))
            };
#endif
        }
    }

    [Theory]
    [MemberData(nameof(FilterToWhereCases))]
    public void FilterToWhere(IFilter filter, IWhereClause expected)
    {
        outputHelper.WriteLine($"Filter : {filter.Jsonify()}");
        outputHelper.WriteLine($"Expected : {expected.Jsonify()}");

        // Act
        IWhereClause actualWhere = filter.ToWhere();
        outputHelper.WriteLine($"actualQuery : {actualWhere.Jsonify()}");

        // Assert
        actualWhere.Should()
            .Be(expected);
    }
}
