using DataFilters.Grammar.Syntax;
using FluentAssertions;
using Xunit;

namespace DataFilters.UnitTests.Grammar.Syntax;

public class GuidValueExpressionTests
{
    public static TheoryData<GuidValueExpression, FilterExpression, bool, string> EqualsCases
        => new()
        {
            {
                new GuidValueExpression("13a26ddf-33ce-f600-223f-09d5193fe9bf"),
                new NumericValueExpression("1"),
                false,
                $"{nameof(GuidValueExpression)} is not a {nameof(NumericValueExpression)}"
            }
        };

    [Theory]
    [MemberData(nameof(EqualsCases))]
    public void Equals_should_behave_as_expected(GuidValueExpression left, FilterExpression other, bool expected, string reason)
        => left.Equals(other).Should().Be(expected, reason);
}