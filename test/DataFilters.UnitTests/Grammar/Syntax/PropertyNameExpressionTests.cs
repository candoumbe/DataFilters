namespace DataFilters.UnitTests.Grammar.Syntax
{
    using System;
    using DataFilters.Grammar.Syntax;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    public class PropertyNameTests(ITestOutputHelper outputHelper)
    {
        [Fact]
        public void IsFilterExpression() => typeof(PropertyName).Should()
                                                                .HaveConstructor(new[] { typeof(string) }).And
                                                                .HaveProperty<string>("Name");

        [Fact]
        public void Ctor_Throws_ArgumentNullException_When_Argument_Is_Null()
        {
            // Act
            Action action = () => _ = new PropertyName(null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>("The parameter of the constructor cannot be null");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Ctor_Throws_ArgumentOutyException_When_Argument_Is_Null(string name)
        {
            // Act
            Action action = () => _ = new PropertyName(name);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentOutOfRangeException>("The parameter of the constructor cannot be empty or whitespace only");
        }

        public static TheoryData<PropertyName, object, bool, string> EqualsCases
            => new()
            {
                {
                    new PropertyName("prop1"),
                    new PropertyName("prop1"),
                    true,
                    "comparing two different instances with same property name"
                },
                {
                    new PropertyName("prop1"),
                    new PropertyName("prop2"),
                    false,
                    "comparing two different instances with different property name"
                }
            };

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void ImplementsEqualsCorrectly(PropertyName first, object other, bool expected, string reason)
        {
            outputHelper.WriteLine($"First instance : {first}");
            outputHelper.WriteLine($"Second instance : {other}");

            // Act
            bool actual = first.Equals(other);
            int actualHashCode = first.GetHashCode();

            // Assert
            actual.Should()
                .Be(expected, reason);

            object _ = expected switch
            {
                true => actualHashCode.Should()
                    .Be(other?.GetHashCode(), reason),
                _ => true
            };
        }
    }
}
