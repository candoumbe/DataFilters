using DataFilters.Grammar.Syntax;
using FluentAssertions;
using System;
using Xunit;

namespace DataFilters.UnitTests.Grammar.Syntax
{
    public class RegularExpressionTests
    {
        [Fact]
        public void IsFilterExpression() => typeof(RegularExpression).Should()
            .BeAssignableTo<FilterExpression>().And
            .Implement<IEquatable<RegularExpression>>().And
            .HaveConstructor(new[] { typeof(string) }).And
            .HaveProperty<string>("Value")
            ;

        [Fact]
        public void Ctor_Throws_ArgumentNullException_If_Value_Is_Null()
        {
            // Act
            Action action = () => new RegularExpression(null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>($"{nameof(RegularExpression)}.{nameof(RegularExpression.Value)} cannot be null");
        }
    }
}
