namespace DataFilters.Grammar.Syntax;

using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// An expression that holds a string value "as is".
/// </summary>
public class TextExpression : StringValueExpression, IEquatable<TextExpression>
{
    private readonly Lazy<string> _lazyEscapedParseableString;

    /// <summary>
    /// Builds a new <see cref="TextExpression"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <paramref name="value"/> should neither start nor end with &quot; unless these quotes are part of the raw text expression.
    /// More specifically, all quotes inside <paramref name="value"/> will be escaped.
    /// </para>
    /// <list type="table">
    /// <listheader>
    ///     <term>Input</term>
    ///     <term>Outputs</term>
    /// </listheader>
    /// <item>
    ///     <term><c>foo</c></term>
    ///     <description>
    ///         <list type="bullet">
    ///             <item>
    ///                 <term><see cref="ConstantValueExpression.Value"/></term>
    ///                 <description><c>foo</c></description>
    ///             </item>
    ///             <item>
    ///                 <term><see cref="OriginalString"/></term>
    ///                 <description><c>foo</c></description>
    ///             </item>
    ///             <item>
    ///                 <term><see cref="EscapedParseableString"/></term>
    ///                 <description><c>"foo"</c></description>
    ///             </item>
    ///         </list>
    ///     </description>
    /// </item>
    /// <item>
    ///     <term><c>"bar"</c></term>
    ///     <description>
    ///         <list type="bullet">
    ///             <item>
    ///                 <term><see cref="ConstantValueExpression.Value"/></term>
    ///                 <description><c>"bar"</c></description>
    ///             </item>
    ///             <item>
    ///                 <term><see cref="OriginalString"/></term>
    ///                 <description><c>"bar"</c></description>
    ///             </item>
    ///             <item>
    ///                 <term><see cref="EscapedParseableString"/></term>
    ///                 <description><c>"\"bar\""</c></description>
    ///             </item>
    ///         </list>
    ///     </description>
    /// </item>
    /// <item>
    ///     <term><c>foo"bar</c></term>
    ///     <description>
    ///         <list type="bullet">
    ///             <item>
    ///                 <term><see cref="ConstantValueExpression.Value"/></term>
    ///                 <description><c>foo"bar</c></description>
    ///             </item>
    ///             <item>
    ///                 <term><see cref="OriginalString"/></term>
    ///                 <description><c>foo"bar</c></description>
    ///             </item>
    ///             <item>
    ///                 <term><see cref="EscapedParseableString"/></term>
    ///                 <description><c>"foo\"bar"</c></description>
    ///             </item>
    ///         </list>
    ///     </description> 
    /// </item>
    /// </list>
    /// </remarks>
    /// <param name="value">Value of the expression.</param>
    public TextExpression(string value) : base(value)
    {
        _lazyEscapedParseableString = new Lazy<string>(() =>
        {
            StringBuilder escapedParseableString;
            if (value.AtLeastOnce(chr => chr == '"' || chr == '\\'))
            {
                escapedParseableString = new StringBuilder(( value.Length * 2 ) + 2);

                foreach (char chr in value)
                {
                    if (chr is '"' or '\\')
                    {
                        escapedParseableString = escapedParseableString.Append('\\');
                    }

                    escapedParseableString = escapedParseableString.Append(chr);
                }
            }
            else
            {
                escapedParseableString = new StringBuilder(value);
            }

            return escapedParseableString.Insert(0, '"').Append('"').ToString();
        });
    }

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