namespace DataFilters.Grammar.Syntax;

using System;

/// <summary>
/// Combines two <see cref="FilterExpression"/>s using the logical <c>OR</c> operator
/// </summary>
public sealed class OrExpression : BinaryFilterExpression, IEquatable<OrExpression>
{
    private readonly Lazy<string> _lazyToString;
    private readonly Lazy<string> _lazyEscapedParseableString;

    /// <summary>
    /// Builds a new <see cref="OrExpression"/> that combines <paramref name="left"/> and <paramref name="right"/> using the logical
    /// <c>OR</c> operator
    /// </summary>
    /// <remarks>
    /// The constructor will wrap <paramref name="left"/> (respectively  <paramref name="right"/>) inside a <see cref="GroupExpression"/> when <paramref name="left"/> is either
    /// a <see cref="AndExpression"/> or a <see cref="OrExpression"/>.
    /// </remarks>
    /// <param name="left">Left member of the expression</param>
    /// <param name="right">Right member of the expression</param>
    /// <exception cref="ArgumentNullException">if either <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.</exception>
    public OrExpression(FilterExpression left, FilterExpression right) : base(left, right)
    {
        _lazyToString = new Lazy<string>(() => $@"[""{nameof(Left)} ({Left.GetType().Name})"": {Left.EscapedParseableString}; ""{nameof(Right)} ({Right.GetType().Name})"": {Right.EscapedParseableString}]");
        _lazyEscapedParseableString = new Lazy<string>(() => $"{Left.EscapedParseableString}|{Right.EscapedParseableString}");
    }

    ///<inheritdoc/>
    public bool Equals(OrExpression other) => other is not null && ( ( Left.Equals(other.Left) && Right.Equals(other.Right) ) || ( Left.Equals(other.Right) && Right.Equals(other.Left) ) );

    ///<inheritdoc/>
    public override bool Equals(object obj) => Equals(obj as OrExpression);

    ///<inheritdoc/>
#if NETSTANDARD1_3 || NETSTANDARD2_0
        public override int GetHashCode() => (Left, Right).GetHashCode();
#else
    public override int GetHashCode() => HashCode.Combine(Left, Right);
#endif

    ///<inheritdoc/>
    public override string ToString() => _lazyToString.Value;

        
    ///<inheritdoc/>
    public override string EscapedParseableString => _lazyEscapedParseableString.Value;

    /// <inheritdoc/>
    public override bool IsEquivalentTo(FilterExpression other)
    {
        bool isEquivalent = false;

        if (other is not null)
        {
            switch (other)
            {
                case GroupExpression { Expression : OrExpression expression }:
                    isEquivalent = IsEquivalentTo(expression);
                    break;
                case OrExpression or:
                    isEquivalent = ( or.Right.IsEquivalentTo(Right) && or.Left.IsEquivalentTo(Left) ) || ( or.Left.IsEquivalentTo(Right) && or.Right.IsEquivalentTo(Left) );
                    break;
                case OneOfExpression oneOf:
                {
                    FilterExpression simplifiedOneOf = oneOf.Simplify();
                    isEquivalent = simplifiedOneOf switch
                    {
                        OrExpression orExpression => IsEquivalentTo(orExpression),
                        OneOfExpression _ => false,
                        _ => Left.IsEquivalentTo(Right) && ( Left.IsEquivalentTo(simplifiedOneOf) || Right.IsEquivalentTo(simplifiedOneOf) )
                    };

                    break;
                }
                default:
                    isEquivalent = Left.IsEquivalentTo(Right) && ( Left.IsEquivalentTo(other) || Right.IsEquivalentTo(other) );
                    break;
            }
        }

        return isEquivalent;
    }

    ///<inheritdoc/>
    public override double Complexity => Left.Complexity + Right.Complexity;

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
            simplified = ( Left, Right ) switch
            {
                (GroupExpression { Expression: OrExpression left }, GroupExpression { Expression: OrExpression right }) => new OneOfExpression(left.Left, left.Right, right.Left, right.Right),
                (GroupExpression { Expression: OrExpression or }, _) => new OneOfExpression(or.Left, or.Right, Right),
                (_, GroupExpression { Expression: OrExpression or }) => new OneOfExpression(or.Left, or.Right, Right),
                _ => this
            };
        }

        return simplified;
    }
}