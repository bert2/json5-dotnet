#pragma warning disable IDE0065 // Misplaced using directive

using Microsoft.FSharp.Core;

using System.Text.Json.Nodes;

namespace Json5.Internal;

using FParsec.CSharp;

using System.Globalization;

using static ArrayParser;
using static FParsec.CSharp.CharParsersCS;
using static FParsec.CSharp.PrimitivesCS;
using static NumberParser;
using static ObjectParser;
using static PrimitiveParsers;
using static StringParser;
using static System.Buffers.Binary.BinaryPrimitives;

using JsonNodeP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<JsonNode?>>;
using StringP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<string>>;
using UnitP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<Unit>>;

public static class CommonParsers {
    public const string NonBreakingWhitespaceChars = " \t\v\f\u00A0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A\u202F\u205F\u3000\uFEFF";

    public const string BreakingWhitespaceChars = "\n\u0085\u2028\u2029"; // \r and \r\n are normalized to \n by FParsec

    public const string WhitespaceChars = NonBreakingWhitespaceChars + BreakingWhitespaceChars;

    public static UnitP NonBreakSpaces { get; set; } = Purify(SkipMany(AnyOf(NonBreakingWhitespaceChars)));

    /// <summary>Skips all whitespace and comments until the next JSON5 token.</summary>
    public static UnitP WSC { get; set; } = Purify(SkipMany(Choice(
        SkipMany1(AnyOf(WhitespaceChars)),
        Skip("//").AndR(SkipRestOfLine(skipNewline: true)),
        Skip("/*").AndR(SkipCharsTillString("*/", maxCount: int.MaxValue, skipString: true)))));

    public static StringP HexEncodedAscii { get; set; } = Array(2, Hex).Map(ConvertUtf8);

    public static StringP HexEncodedUnicode { get; set; } =
        Choice(
            Between('{', Many1Chars(Hex), '}').And(ParseUtf32),
            Array(4, Hex).Map(ConvertUtf16));

    public static JsonNodeP Json5Value { get; set; } =
        Choice(
            Json5Array,
            Json5Object,
            Json5Null,
            Json5Bool,
            Json5String,
            Json5Number)
        .And(WSC);

    private static string ConvertUtf8(char[] twoHexDigits)
        => new((char)Convert.FromHexString(twoHexDigits)[0], 1);

    private static string ConvertUtf16(char[] fourHexDigits)
        => new((char)ReadUInt16BigEndian(Convert.FromHexString(fourHexDigits)), 1);

    private static StringP ParseUtf32(string hexDigits) {
        const NumberStyles sty = NumberStyles.AllowHexSpecifier;
        var cult = CultureInfo.InvariantCulture;
        const int maxCodePoint = 0x10FFFF;
        return int.TryParse(hexDigits, sty, cult, out var utf32) && utf32 <= maxCodePoint
            ? Return(char.ConvertFromUtf32(utf32))
            : Fail<string>($@"Invalid Unicode escape sequence: \u{{{hexDigits}}}");
    }
}
