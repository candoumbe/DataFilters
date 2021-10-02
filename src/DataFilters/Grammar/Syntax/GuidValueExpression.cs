
namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// Wraps a string that represents a <see cref="System.Guid"/>
    /// </summary>
    public class GuidValueExpression : ConstantValueExpression
    {
        /// <summary>
        /// Builds a new <see cref="GuidValueExpression"/> instance that can wrap a <see cref="System.Guid"/>
        /// <param name="value"></param>
        public GuidValueExpression(string value) : base(value)
        { }

        ///<inheritdoc/>
        public override string EscapedParseableString => Value;
    }
}