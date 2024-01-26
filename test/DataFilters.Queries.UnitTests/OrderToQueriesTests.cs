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

    public class OrderToQueriesTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public OrderToQueriesTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        public class Person
        {
            public string Firstname { get; set; }

            public string Lastname { get; set; }
        }

        public static IEnumerable<object[]> OrderToOrderCases
        {
            get
            {
                yield return new object[]
                {
                    new Order<Person>("Name"),
                    (Expression<Func<IEnumerable<IOrder>, bool>>) (sorts => sorts.Once()
                        && sorts.First().Equals(new OrderExpression("Name".Field(), Ascending)))
                };

                {
                    MultiOrder<Person> multiSort = new
                    (
                        new Order<Person>(nameof(Person.Firstname)),
                        new Order<Person>(nameof(Person.Lastname))
                    );
                    yield return new object[]
                    {
                        multiSort,
                        (Expression<Func<IEnumerable<IOrder>, bool>>) (sorts => sorts.Exactly(2)
                            && sorts.First().Equals(new OrderExpression(nameof(Person.Firstname), Ascending))
                            && sorts.Last().Equals(new OrderExpression(nameof(Person.Lastname), Ascending)))
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(OrderToOrderCases))]
        public void FilterToOrder(IOrder<Person> sort, Expression<Func<IEnumerable<IOrder>, bool>> expected)
        {
            _outputHelper.WriteLine($"Sort : {sort.Jsonify()}");

            // Act
            IEnumerable<IOrder> actual = sort.ToOrder();
            _outputHelper.WriteLine($"actual result : {actual.Jsonify()}");

            // Assert
            actual.Should()
                .Match(expected);
        }
    }
}
