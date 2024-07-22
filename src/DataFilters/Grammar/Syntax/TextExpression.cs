namespace DataFilters.Grammar.Syntax
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// An expression that holds a string value "as is".
    /// </summary>
    /// <remarks>
    /// Builds a new <see cref="TextExpression"/> instance.
    /// </remarks>
    /// <param name="value">Value of the expression</param>
    public class TextExpression(string value) : StringValueExpression(value), IEquatable<TextExpression>
    {
        private readonly Lazy<string> _lazyEscapedParseableString = new(() =>
            {
                StringBuilder escapedParseableString;
                if (value.AtLeastOnce(chr => chr == '"' || chr == '\\'))
                {
                    escapedParseableString = new StringBuilder((value.Length * 2) + 2);

                    foreach (char chr in value)
                    {
                        if (chr is '"' or '\\')
                        {
                            escapedParseableString.Append('\\');
                        }
                        escapedParseableString.Append(chr);
                    }
                }
                else
                {
                    escapedParseableString = new StringBuilder(value);
                }

                return escapedParseableString.Insert(0, '"').Append('"').ToString();
            });

        ///<inheritdoc/>
        public override string EscapedParseableString => _lazyEscapedParseableString.Value;

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