#pragma warning disable IDE0065 // Misplaced using directive

using Microsoft.FSharp.Core;

namespace Json5.Internal;

using System;
using System.Text.RegularExpressions;

using Chars = FParsec.CharStream<Unit>;

/// <summary>
/// Provides some helper extension methods on <see cref="FParsec.CharStream{TUserState}"/>.
/// </summary>
public static partial class CharStreamExtensions {
    /// <summary>
    /// Skips the current position of the <see cref="FParsec.CharStream{TUserState}"/> ahead for
    /// <paramref name="utf16Offset"/> characters while counting Unicode escape sequences as
    /// single characters.
    /// </summary>
    /// <param name="chars">The input <see cref="FParsec.CharStream{TUserState}"/>.</param>
    /// <param name="utf16Offset">The nubmer of characters to skip.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="utf16Offset"/> is below <c>0</c>.</exception>
    public static void SkipEscaped(this Chars chars, long utf16Offset) {
        if (utf16Offset < 0) throw new ArgumentOutOfRangeException(nameof(utf16Offset), "Must be positive.");

        for (; utf16Offset > 0; utf16Offset--)
            chars.Skip(chars.MatchUnicodeEscape(out var len) ? len : 1);
    }

    /// <summary>
    /// <para>
    /// Checks if next few characters in the <see cref="FParsec.CharStream{TUserState}"/> match an
    /// <see href="https://tc39.es/ecma262/#prod-UnicodeEscapeSequence">ECMA script Unicode escape sequence</see>.
    /// </para>
    /// <para>
    /// Will not check whether the actual code point is valid (i.e. less than or equal to <c>0x10FFFF</c>).
    /// </para>
    /// </summary>
    /// <param name="chars">The input <see cref="FParsec.CharStream{TUserState}"/>.</param>
    /// <param name="length">The length of the found escape sequence, or <c>0</c> if no escape sequences was found.</param>
    /// <returns><c>true</c> if an escape sequence was found, <c>false</c> otherwise.</returns>
    public static bool MatchUnicodeEscape(this Chars chars, out int length) {
        if (chars.Match(Utf32Escape()) is { Success: true, Length: var len }) {
            length = len;
            return true;
        } else if (chars.Match(Utf16Escape()).Success) {
            length = 6;
            return true;
        } else {
            length = 0;
            return false;
        }
    }

    [GeneratedRegex(@"\A\\u[\da-fA-F]{4}")]
    public static partial Regex Utf16Escape();

    [GeneratedRegex(@"\A\\u\{[\da-fA-F]+\}")]
    public static partial Regex Utf32Escape();
}
