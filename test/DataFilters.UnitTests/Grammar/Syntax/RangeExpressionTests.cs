using DataFilters.Grammar.Syntax;
using FluentAssertions;
using System;
using Xunit;

namespace DataFilters.UnitTests.Grammar.Syntax
{
    public class RangeExpressionTests
    {
        [Fact]
        public void IsFilterExpression() => typeof(RangeExpression).Should()
            .BeAssignableTo<FilterExpression>().And
            .Implement<IEquatable<RangeExpression>>().And
            .HaveConstructor(new[] { typeof(ConstantExpression), typeof(ConstantExpression)}).And
            .HaveProperty<ConstantExpression>("Min").And
            .HaveProperty<ConstantExpression>("Max")
            ;

        [Fact]
        public void Ctor_Throws_ArgumentNullException_If_Value_Is_Null()
        {
            // Act
            Action action = () => new RangeExpression(null, null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>($"Either {nameof(RangeExpression.Min)}/{nameof(RangeExpression.Max)} must be set");
        }
    }
}
