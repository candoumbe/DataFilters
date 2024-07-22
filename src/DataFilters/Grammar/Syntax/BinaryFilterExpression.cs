using System;

namespace DataFilters.Grammar.Syntax;

/// <summary>
/// An expression that has <see cref="Left"/> and <see cref="Right"/> operands.
/// </summary>
public abstract class BinaryFilterExpression : FilterExpression, ISimplifiable
{
    /// <summary>
    /// Left operand
    /// </summary>
    public FilterExpression Left { get; }

    /// <summary>
    /// Right operand
    /// </summary>
    public FilterExpression Right { get; }

    /// <summary>
    /// Builds a new <see cref="BinaryFilterExpression"/> instance.
    /// </summary>
    /// <param name="left">the left operand</param>
    /// <param name="right">the right operand</param>
    /// <exception cref="ArgumentNullException">if <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.</exception>
    protected BinaryFilterExpression(FilterExpression left, FilterExpression right)
    {
        (Left, Right) = (left, right) switch
        {
            (null, _) => throw new ArgumentNullException(nameof(left)),
            (_, null) => throw new ArgumentNullException(nameof(right)),
            (BinaryFilterExpression, BinaryFilterExpression) => (new GroupExpression(left), new GroupExpression(right)),
            (BinaryFilterExpression, not GroupExpression) => (new GroupExpression(left), right),
            (not GroupExpression, BinaryFilterExpression) => (left, new GroupExpression(right)),
            (BinaryFilterExpression, _) => (new GroupExpression(left), right),
            (_, BinaryFilterExpression) => (left, new GroupExpression(right)),
            _ => (left, right)
        };
    }

    ///<inheritdoc/>
    public virtual FilterExpression Simplify()
    {
        FilterExpression simplifiedLeft = SimplifyLocal((Left as ISimplifiable)?.Simplify() ?? Left);
        FilterExpression simplifiedRight = SimplifyLocal((Right as ISimplifiable)?.Simplify() ?? Right);

        FilterExpression simplifiedExpression = this;

        if (simplifiedLeft.IsEquivalentTo(simplifiedRight))
        {
            simplifiedExpression = simplifiedLeft.Complexity.CompareTo(simplifiedRight.Complexity) < 0d
                ? simplifiedLeft
                : simplifiedRight;
        }

        return simplifiedExpression;

        static FilterExpression SimplifyLocal(FilterExpression expression)
        {
            FilterExpression current = expression;

            while (current is ISimplifiable simplifiable && !simplifiable.Equals(current))
            {
                current = SimplifyLocal(simplifiable.Simplify());
            }

            return current;
        }
    }

    /// <inheritdoc />
    public override string ToString(string format, IFormatProvider formatProvider)
    {
        FormattableString formattable = format switch
        {
            "d" or "D" => $"@{GetType().Name}({Left:d},{Right:d})",
            null or "" => $"{EscapedParseableString}", 
            _ => throw new ArgumentOutOfRangeException(nameof(format), $"Unsupported '{format}' format")
        };

        return formattable.ToString(formatProvider);
    }
}
