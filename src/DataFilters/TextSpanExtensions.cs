using System;
using Microsoft.Extensions.Primitives;

// ReSharper disable once CheckNamespace
namespace Superpower.Model;

internal static class TextSpanExtensions
{
    /// <summary>
    /// Converts a <see cref="TextSpan"/> to the equivalent <see cref="StringSegment"/>
    /// </summary>
    /// <param name="textSpan"></param>
    /// <returns>The equivalent <see cref="StringSegment"/></returns>
    /// <exception cref="ArgumentException">if <paramref name="textSpan"/> underlying <see langword="string"/> is <see langword="null"/>.</exception>
    internal static StringSegment ToStringSegment(this TextSpan textSpan)
    {
        if (textSpan is { Source: null })
        {
            throw new ArgumentException("The text span inner text cannot be null.", nameof(textSpan));
        }

        return new StringSegment(textSpan.Source, textSpan.Position.Absolute, textSpan.Length);
    }
}
