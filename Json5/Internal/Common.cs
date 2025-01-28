#pragma warning disable IDE0065 // Misplaced using directive

using Microsoft.FSharp.Core;

namespace Json5.Internal;

using FParsec.CSharp;

using System.Globalization;
using System.Text.Encodings.Web;

using static ArrayParser;
using static FParsec.CSharp.CharParsersCS;
using static FParsec.CSharp.PrimitivesCS;
using static NumberParser;
using static ObjectParser;
using static StringParser;
using static System.Buffers.Binary.BinaryPrimitives;

using StringP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<string>>;
using UnitP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<Unit>>;

/// <summary>Implements some JSON5 parsers and helpers used by other parsers.</summary>
public static class Common {
    /// <summary>All whitespace characters that don't break lines.</summary>
    public const string NonBreakingWhitespaceChars = " \t\v\f\u00A0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A\u202F\u205F\u3000\uFEFF";

    /// <summary>
    /// <para>All whitespace characters that are considered line breaks.</para>
    /// <para>Does not contain \r because FParsec normalizes are line endings to \n.</para>
    /// </summary>
    public const string BreakingWhitespaceChars = "\n\u0085\u2028\u2029";

    /// <summary>All possible whitespace characters (breaking and non-breaking).</summary>
    public const string WhitespaceChars = NonBreakingWhitespaceChars + BreakingWhitespaceChars;

    /// <summary>
    /// The encoder used to encode JSON strings and object keys.
    /// Defaults to <see cref="JavaScriptEncoder.UnsafeRelaxedJsonEscaping"/>.
    /// <para>
    /// Set to <see cref="JavaScriptEncoder.Default"/> for safer encodings.
    /// </para>
    /// </summary>
    public static JavaScriptEncoder Encoder { get; set; } = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

    /// <summary>
    /// <para>Skips zero or more optional non-breaking whitespace characters.</para>
    /// <para>Will not produce any parser errors in case no whitespace was found.</para>
    /// </summary>
    public static UnitP NonBreakSpaces { get; set; } = Purify(SkipMany(AnyOf(NonBreakingWhitespaceChars)));

    /// <summary>
    /// <para>Skips all whitespace and comments until the next JSON5 token.</para>
    /// <para>Will not produce any parser errors in case no whitespace was found.</para>
    /// </summary>
    public static UnitP WSC { get; set; } = Purify(SkipMany(Choice(
        SkipMany1(AnyOf(WhitespaceChars)),
        Skip("//").AndR(SkipManyTill(NoneOf(BreakingWhitespaceChars), SkipAnyOf(BreakingWhitespaceChars).Or(EOF))),
        Skip("/*").AndR(SkipCharsTillString("*/", maxCount: int.MaxValue, skipString: true)))));

    /// <summary>Parses two hex digits and converts them to a single-character string.</summary>
    public static StringP HexEncodedAscii { get; set; } = Array(2, Hex).Map(ConvertUtf8);

    /// <summary>
    /// Parses either:
    /// <list type="bullet">
    /// <item>
    /// one or more hex digits enclosed with <c>{}</c> and converts the resulting UTF32 character to a UTF16 string.
    /// </item>
    /// <item>four hex digits and converts them to a single-character UTF16 string.</item>
    /// </list>
    /// </summary>
    public static StringP HexEncodedUnicode { get; set; } =
        Choice(
            Between('{', Many1Chars(Hex), '}').And(ParseUtf32),
            Array(4, Hex).Map(ConvertUtf16));

    /// <summary>
    /// Parses any of the possible JSON5 values (i.e. null, true, false, string, number, array, or object)
    /// and outputs them as a JSON string.
    /// </summary>
    public static StringP Json5Value { get; set; } =
        Choice(
            Json5Array,
            Json5Object,
            StringP("null").Lbl("null"),
            StringP("true").Lbl("bool"),
            StringP("false").Lbl("bool"),
            Json5String,
            Json5Number)
        .And(WSC);

    /// <summary>Converts two hex digits to a single-character string.</summary>
    /// <param name="twoHexDigits">The two hex digits as an array.</param>
    /// <returns>The converted character as a string.</returns>
    public static string ConvertUtf8(char[] twoHexDigits)
        => new((char)Convert.FromHexString(twoHexDigits)[0], 1);

    /// <summary>Converts four hex digits to a single-character UTF16 string.</summary>
    /// <param name="fourHexDigits">The four hex digits as an array.</param>
    /// <returns>The converted character as a UTF16 string.</returns>
    public static string ConvertUtf16(char[] fourHexDigits)
        => new((char)ReadUInt16BigEndian(Convert.FromHexString(fourHexDigits)), 1);

    /// <summary>
    /// <para>
    /// Constructs a parser that parses the input string as hex digits. The resulting value
    /// must be a valid Unicode code point, i.e. smaller than or equal to <c>10FFFF</c>.
    /// </para>
    /// <para>
    /// Depending on the parsed code point the parser will return a UTF16 string of length 1 or 2.
    /// </para>
    /// </summary>
    /// <param name="hexDigits">The hex digits as a string.</param>
    /// <returns>The constructed parser.</returns>
    public static StringP ParseUtf32(string hexDigits) {
        const NumberStyles sty = NumberStyles.AllowHexSpecifier;
        var cult = CultureInfo.InvariantCulture;
        const int maxCodePoint = 0x10FFFF;
        return int.TryParse(hexDigits, sty, cult, out var utf32) && utf32 <= maxCodePoint
            ? Return(char.ConvertFromUtf32(utf32))
            : Fail<string>($@"Invalid Unicode escape sequence: \u{{{hexDigits}}}");
    }
}
