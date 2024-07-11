namespace DataFilters.Queries.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using FluentAssertions;
    using global::Queries.Core.Parts.Sorting;
    using Xunit;
    using Xunit.Abstractions;
    using static global::Queries.Core.Parts.Sorting.OrderDirection;

    public class OrderToQueriesTests(ITestOutputHelper outputHelper)
    {
        public class Person
        {
            public string Firstname { get; set; }

            public string Lastname { get; set; }
        }

        public static TheoryData<IOrder<Person>, Expression<Func<IEnumerable<IOrder>, bool>>> OrderToOrderCases
            => new()
            {
                {
                    new Order<Person>("Name"),
                     sorts => sorts.Once()
                        && sorts.First().Equals(new OrderExpression("Name".Field(), Ascending))
                },

                {
                    new MultiOrder<Person>
                    (
                        new Order<Person>(nameof(Person.Firstname)),
                        new Order<Person>(nameof(Person.Lastname))
                    ),
                         sorts => sorts.Exactly(2)
                            && sorts.First().Equals(new OrderExpression(nameof(Person.Firstname), Ascending))
                            && sorts.Last().Equals(new OrderExpression(nameof(Person.Lastname), Ascending))
                }
            };

        [Theory]
        [MemberData(nameof(OrderToOrderCases))]
        public void FilterToOrder(IOrder<Person> sort, Expression<Func<IEnumerable<IOrder>, bool>> expected)
        {
            outputHelper.WriteLine($"Sort : {sort.Jsonify()}");

            // Act
            IEnumerable<IOrder> actual = sort.ToOrder();
            outputHelper.WriteLine($"actual result : {actual.Jsonify()}");

            // Assert
            actual.Should()
                .Match(expected);
        }
    }
}
