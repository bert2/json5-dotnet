#pragma warning disable IDE0065 // Misplaced using directive

using Microsoft.FSharp.Core;

namespace Json5.Internal;

using System;
using System.Text.RegularExpressions;

using Chars = FParsec.CharStream<Unit>;

public static partial class CharStreamExtensions {
    public static void SkipEscaped(this Chars chars, long utf16Offset) {
        if (utf16Offset < 0) throw new ArgumentOutOfRangeException(nameof(utf16Offset), "Must be positive.");

        for (; utf16Offset > 0; utf16Offset--)
            chars.Skip(chars.MatchUnicodeEscape(out var len) ? len : 1);
    }

    public static bool MatchUnicodeEscape(this Chars chars, out int length) {
        if (chars.Match(Utf16Escape()).Success) {
            length = 6;
            return true;
        } else if (chars.Match(Utf32Escape()) is { Success: true, Length: var len }) {
            length = len;
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
