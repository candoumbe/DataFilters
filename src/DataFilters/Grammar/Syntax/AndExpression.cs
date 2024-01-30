namespace DataFilters.Grammar.Syntax;

using System;

/// <summary>
/// A <see cref="FilterExpression"/> that combine two <see cref="FilterExpression"/> expressions using the logical <c>AND</c> operator
/// </summary>
public sealed class AndExpression : BinaryFilterExpression, IEquatable<AndExpression>
{
    private readonly Lazy<string> _lazyToString;
    private readonly Lazy<string> _lazyEscapedParseableString;

    /// <inheritdoc/>
    public override double Complexity => Right.Complexity * Left.Complexity;

    /// <summary>
    /// Builds a new <see cref="AndExpression"/> that combines <paramref name="left"/> and <paramref name="right"/> using the logical
    /// <c>AND</c> operator
    /// </summary>
    /// <remarks>
    /// <paramref name="left"/> and/or <paramref name="right"/> will be wrapped inside a <see cref="GroupExpression"/> when either is a
    /// <see cref="AndExpression"/> or a <see cref="OrExpression"/> instance.
    /// </remarks>
    /// <param name="left">Left member</param>
    /// <param name="right">Right member</param>
    /// <exception cref="ArgumentNullException">if either <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.</exception>
    public AndExpression(FilterExpression left, FilterExpression right) : base(left, right)
    {
        _lazyToString = new(() => $@"[""{nameof(Left)} ({Left.GetType().Name})"": {Left.EscapedParseableString}; ""{nameof(Right)} ({Right.GetType().Name})"": {Right.EscapedParseableString}]");
        _lazyEscapedParseableString = new(() => $"{Left.EscapedParseableString},{Right.EscapedParseableString}");
    }

    ///<inheritdoc/>
    public bool Equals(AndExpression other) => (Left.Equals(other?.Left) && Right.Equals(other?.Right)) || (Left.Equals(other?.Right) && Right.Equals(other?.Left));

    ///<inheritdoc/>
    public override bool Equals(object obj) => Equals(obj as AndExpression);

    ///<inheritdoc/>
    public override int GetHashCode() => (Left, Right).GetHashCode();

    ///<inheritdoc/>
    public override bool IsEquivalentTo(FilterExpression other)
        => ReferenceEquals(this, other)
            || other switch
            {
                AndExpression and => Equals(and) || (Left.IsEquivalentTo(and.Left) && Right.IsEquivalentTo(and.Right)) || (Left.IsEquivalentTo(and.Right) && Right.IsEquivalentTo(and.Left)),
                ConstantValueExpression constant => Simplify().IsEquivalentTo(constant),
                ISimplifiable simplifiable => IsEquivalentTo(simplifiable.Simplify()),
                _ => false
            };

    ///<inheritdoc/>
    public override string EscapedParseableString => _lazyEscapedParseableString.Value;

    ///<inheritdoc/>
    public override string ToString() => _lazyToString.Value;
}