using System;

namespace DataFilters.Grammar.Syntax
{
    /// <summary>
    /// Base class for filter expression
    /// </summary>
    public abstract class FilterExpression
    {
        public override string ToString() => $"{GetType().Name} : {this.Jsonify()}";
    }
}
