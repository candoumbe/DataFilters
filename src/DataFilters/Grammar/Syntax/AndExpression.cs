using System;

namespace DataFilters.Grammar.Syntax;

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
    /// <exception cref="ArgumentNullException">if either <paramref name="left"/> or <paramref name="right"/> is <c>null</c>.</exception>
    public AndExpression(FilterExpression left, FilterExpression right) : base(left, right)
    {
        _lazyToString = new Lazy<string>(() => $@"[""{nameof(Left)} ({Left.GetType().Name})"": '{Left}'; ""{nameof(Right)} ({Right.GetType().Name})"": '{Right}']");
        _lazyEscapedParseableString = new Lazy<string>(() => $"{Left.EscapedParseableString},{Right.EscapedParseableString}");
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
               ConstantValueExpression constant => (Left.Equals(Right) || Left.IsEquivalentTo(Right)) && (Left.IsEquivalentTo(constant) || Right.IsEquivalentTo(constant)),
               ISimplifiable simplifiable => simplifiable.Simplify().IsEquivalentTo(this),
               _ => false
           };

    ///<inheritdoc/>
    public override string EscapedParseableString => _lazyEscapedParseableString.Value;

    ///<inheritdoc/>
    public override string ToString() => _lazyEscapedParseableString.Value;

    /// <inheritdoc />
    public override FilterExpression Simplify()
    {
        FilterExpression simplified;
        if (ReferenceEquals(Left, Right) || Left.Equals(Right) || Left.IsEquivalentTo(Right))
        {
            simplified = Left.Complexity < Right.Complexity
                ? Left.As<ISimplifiable>()?.Simplify() ?? Left
                : Right.As<ISimplifiable>()?.Simplify() ?? Right;
        }
        else
        {
            simplified = (Left, Right) switch
            {
                (ISimplifiable left, ISimplifiable right) => new AndExpression(left.Simplify(), right.Simplify()),
                (ISimplifiable left, not ISimplifiable) => new AndExpression(left.Simplify(), Right),
                (not ISimplifiable, ISimplifiable right) => new AndExpression(Left, right.Simplify()),
                (not ISimplifiable, not ISimplifiable) => this
            };
        }

        return simplified;
    }
}