using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFilters.Grammar.Syntax
{
    public class OneOfExpression : FilterExpression, IEquatable<OneOfExpression>
    {
        public IEnumerable<FilterExpression> Values { get; }

        public OneOfExpression(params FilterExpression[] values) => Values = values ?? throw new ArgumentNullException(nameof(values));

        public bool Equals(OneOfExpression other) => other != null
            && Values.Exactly(other.Values.Count())
            && Values.All(value => other.Values.Contains(value))
            && other.Values.All(value => Values.Contains(value));

        public override bool Equals(object obj) => Equals(obj as OneOfExpression);

        public override int GetHashCode() => Values.GetHashCode();

        public override string ToString() => $"{GetType().Name} : Values -> {string.Join(Environment.NewLine, Values)}";
    }
}
