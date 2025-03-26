using System.Linq;
using System.Text;
using DataFilters.Grammar.Parsing;

namespace DataFilters.UnitTests.Grammar.Syntax;

using System;
using DataFilters.Grammar.Syntax;
using DataFilters.UnitTests.Helpers;
using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using Xunit;
using Xunit.Categories;

[UnitTest("StartsWith")]
public class StartsWithExpressionTests
{
    [Fact]
    public void IsFilterExpression() => typeof(StartsWithExpression).Should()
        .NotBeAbstract().And
        .BeAssignableTo<FilterExpression>().And
        .Implement<IEquatable<StartsWithExpression>>().And
        .Implement<IParseableString>().And
        .NotImplement<IBoundaryExpression>();

    [Fact]
    public void Given_string_argument_is_null_Constructor_should_throw_ArgumentNullException()
    {
        // Act
        Action action = () => _ = new StartsWithExpression((string)null);

        // Assert
        action.Should()
            .ThrowExactly<ArgumentNullException>("The parameter of the constructor cannot be null");
    }

    [Fact]
    public void Given_TextExpression_argument_is_null_Constructor_should_throw_ArgumentNullException()
    {
        // Act
        Action action = () => _ = new StartsWithExpression((TextExpression)null);

        // Assert
        action.Should()
            .ThrowExactly<ArgumentNullException>("The parameter of the constructor cannot be null");
    }

    [Fact]
    public void Ctor_Throws_ArgumentOutOfRangeException_When_Argument_Is_Empty()
    {
        // Act
        Action action = () => _ = new StartsWithExpression(string.Empty);

        // Assert
        action.Should()
            .ThrowExactly<ArgumentOutOfRangeException>("The parameter of the constructor cannot be empty");
    }

    [Fact]
    public void Ctor_DoesNot_Throws_ArgumentOutOfRangeException_When_Argument_Is_WhitespaceOnly()
    {
        // Act
        Action action = () => _ = new StartsWithExpression("  ");

        // Assert
        action.Should()
            .NotThrow<ArgumentOutOfRangeException>("The parameter of the constructor can be whitespace only");
        action.Should()
            .NotThrow("The parameter of the constructor can be whitespace only");
    }

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Equals_should_be_commutative(NonNull<StartsWithExpression> first, FilterExpression second)
        => first.Item.Equals(second).Should().Be(second.Equals(first.Item));

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Equals_should_be_reflexive(NonNull<StartsWithExpression> expression)
        => expression.Item.Should().Be(expression.Item);

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Equals_should_be_symmetric(NonNull<StartsWithExpression> expression, NonNull<FilterExpression> otherExpression)
        => (expression.Item.Equals(otherExpression.Item) == otherExpression.Item.Equals(expression.Item)).ToProperty();

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Given_TextExpression_as_input_EscapedParseableString_should_be_correct(NonNull<TextExpression> text)
    {
        // Arrange
        StartsWithExpression expression = new(text.Item);
        string expected = $"{text.Item.EscapedParseableString}*";

        // Act
        string actual = expression.EscapedParseableString;

        // Assert
        actual.Should()
            .Be(expected);
    }

    [Property]
    public void Given_non_whitespace_string_as_input_as_input_EscapedParseableString_should_be_correct(NonWhiteSpaceString textGenerator)
    {
        // Arrange
        string text = textGenerator.Item;
        StartsWithExpression expression = new(text);
        StringValueExpression stringValueExpression = new(text);
        StringBuilder sb = new (text.Length * 2);
        foreach (char chr in text)
        {
            if (FilterTokenizer.SpecialCharacters.Contains(chr))
            {
                sb = sb.Append(FilterTokenizer.EscapedCharacter);
            }
            sb = sb.Append(chr);
        }
        string expected = $"{stringValueExpression.EscapedParseableString}*";

        // Act
        string actual = expression.EscapedParseableString;

        // Assert
        actual.Should()
            .Be(expected);
    }

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Given_StartsWithExpression_When_right_operand_is_EndsWithExpression_Plus_operator_should_return_expected_AndExpression(NonNull<StartsWithExpression> startsWithGen, NonNull<EndsWithExpression> endsWithGen)
    {
        // Arrange
        StartsWithExpression startsWith = startsWithGen.Item;
        EndsWithExpression endsWith = endsWithGen.Item;

        AndExpression expected = new(startsWith, endsWith);

        // Act
        AndExpression actual = startsWith + endsWith;

        // Assert
        actual.IsEquivalentTo(expected).Should().BeTrue();
    }

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Given_StartsWithExpression_When_right_operand_is_StartsWithExpression_Plus_operator_should_return_OneOfExpression(NonNull<StartsWithExpression> leftOperandGen, NonNull<StartsWithExpression> rightOperandGen)
    {
        // Arrange
        StartsWithExpression leftStartsWith = leftOperandGen.Item;
        StartsWithExpression rightStartsWith = rightOperandGen.Item;

        // bat*man*
        OneOfExpression expected = new(new StringValueExpression(leftStartsWith.Value + rightStartsWith.Value),
            new AndExpression(leftStartsWith, new ContainsExpression(rightStartsWith.Value)),
            new StartsWithExpression(leftStartsWith.Value + rightStartsWith.Value));

        // Act
        OneOfExpression actual = leftStartsWith + rightStartsWith;

        // Assert
        actual.IsEquivalentTo(expected).Should().BeTrue();
    }

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Given_StartsWithExpression_When_right_operand_is_Contains_Plus_operator_should_return_OneOfExpression(NonNull<StartsWithExpression> leftOperandGen, NonNull<ContainsExpression> rightOperandGen)
    {
        // Arrange
        StartsWithExpression leftStartsWith = leftOperandGen.Item;
        ContainsExpression rightStartsWith = rightOperandGen.Item;

        OneOfExpression expected = new(new StringValueExpression(leftStartsWith.Value + rightStartsWith.Value),
            new AndExpression(leftStartsWith, new ContainsExpression(rightStartsWith.Value)),
            new StartsWithExpression(leftStartsWith.Value + rightStartsWith.Value));

        // Act
        OneOfExpression actual = leftStartsWith + rightStartsWith;

        // Assert
        actual.IsEquivalentTo(expected).Should().BeTrue();
    }

    [Property(Arbitrary = [typeof(ExpressionsGenerators)])]
    public void Given_StartsWithExpression_When_right_operand_is_StringValueExpression_Plus_operator_should_return_expected_AndExpression(NonNull<StartsWithExpression> leftOperandGen, NonNull<StringValueExpression> rightOperandGen)
    {
        // Arrange
        StartsWithExpression leftStartsWith = leftOperandGen.Item;
        StringValueExpression rightStartsWith = rightOperandGen.Item;

        AndExpression expected = leftStartsWith + new EndsWithExpression(rightStartsWith.Value);

        // Act
        AndExpression actual = leftStartsWith + rightStartsWith;

        // Assert
        actual.IsEquivalentTo(expected).Should().BeTrue();
    }
}