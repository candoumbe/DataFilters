namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// Building block for <see cref="RegularExpression"/>.
    /// </summary>
#if NET5_0_OR_GREATER
    public abstract record RegexValue;
#else
    public abstract class RegexValue
    {}
#endif
}