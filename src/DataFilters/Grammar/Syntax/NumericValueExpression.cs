
namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// Wraps a string that represents a numeric value of some sort
    /// </summary>
    public class NumericValueExpression : ConstantValueExpression
    {
        /// <summary>
        /// Builds a new <see cref="NumericValueExpression"/> instance that can wrap a numeric value of some sort
        /// </summary>
        /// <param name="value"></param>
        public NumericValueExpression(string value) : base(value)
        { }

        ///<inheritdoc/>
        public override string EscapedParseableString => Value;
    }
}