
namespace DataFilters.Grammar.Syntax;

/// <summary>
/// Wraps a string that represents a <see cref="System.Guid"/>
/// </summary>
/// <remarks>
/// Builds a new <see cref="GuidValueExpression"/> instance that can wrap a <see cref="System.Guid"/>
/// </remarks>
/// <param name="value"></param>
public class GuidValueExpression(string value) : ConstantValueExpression(value)
{
    ///<inheritdoc/>
    public override string EscapedParseableString => Value;
}