namespace DataFilters.Grammar.Syntax;

/// <summary>
/// Building block for <see cref="BracketExpression"/>.
/// </summary>
public abstract class BracketValue : IParseableString, IHaveComplexity
{
    ///<inheritdoc/>
    public abstract string EscapedParseableString { get; }

    ///<inheritdoc/>
    public abstract string OriginalString { get; }

    ///<inheritdoc/>
    public abstract double Complexity { get; }

    ///<inheritdoc/>
    public override string ToString() => EscapedParseableString;
}