namespace DataFilters.Grammar.Syntax
{
    using System;
    using System.Text;
    using System.Collections.Generic;

    /// <summary>
    /// An expression that holds a string value "as is".
    /// </summary>
    public class TextExpression : StringValueExpression, IEquatable<TextExpression>
    {
        private readonly Lazy<string> _lazyParseableString;

        /// <summary>
        /// Builds a new <see cref="TextExpression"/> instance.
        /// </summary>
        /// <param name="value">Value of the expression</param>
        public TextExpression(string value) : base(value)
        {
            _lazyParseableString = new(() =>
            {
                StringBuilder escapedParseableString;
                if (value.AtLeastOnce(chr => chr == '"' || chr == '\\'))
                {
                    escapedParseableString = new StringBuilder((value.Length * 2) + 2);

                    foreach (char chr in value)
                    {
                        if (chr == '"' || chr == '\\')
                        {
                            escapedParseableString.Append('\\');
                        }
                        escapedParseableString.Append(chr);
                    }
                }
                else
                {
                    escapedParseableString = new(value);
                }

                return escapedParseableString.Insert(0, '"').Append('"').ToString();
            });
        }

        ///<inheritdoc/>
        public override string EscapedParseableString => _lazyParseableString.Value;

        ///<inheritdoc/>
        public override string OriginalString => Value;

        ///<inheritdoc/>
        public virtual bool Equals(TextExpression other) => Value.Equals(other?.Value);

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as TextExpression);

        ///<inheritdoc/>
        public override int GetHashCode() => Value.GetHashCode();
    }
}