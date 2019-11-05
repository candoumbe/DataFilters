using DataFilters.Grammar.Syntax;
using FluentAssertions;
using System;
using Xunit;

namespace DataFilters.UnitTests.Grammar.Syntax
{
    public class EqualExpressionTests
    {
        [Fact]
        public void IsFilterExpression() => typeof(EqualExpression).Should()
                                                                          .BeAssignableTo<FilterExpression>().And
            .Implement<IEquatable<EqualExpression>>().And
                                                                          .HaveDefaultConstructor();
    }
}
