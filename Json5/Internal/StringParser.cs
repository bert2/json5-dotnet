#pragma warning disable IDE0065 // Misplaced using directive

using Microsoft.FSharp.Core;

namespace Json5.Internal;

using FParsec.CSharp;

using static Common;
using static FParsec.CSharp.CharParsersCS;
using static FParsec.CSharp.PrimitivesCS;

using StringP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<string>>;

/// <summary>Implements parsing of JSON5 strings.</summary>
public static class StringParser {
    /// <summary>
    /// <para>
    /// Parses a JSON5 string and translates it to JSON, i.e.:
    /// <list type="bullet">
    /// <item>Single quotes will become double quotes,</item>
    /// <item>Multiline strings will be merged into a single line,</item>
    /// <item>Escape sequences will be translated to the character they represent.</item>
    /// </list>
    /// </para>
    /// <item>
    /// The final string will be encoded with
    /// <see cref="System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping"/>.
    /// </item>
    /// </summary>
    public static StringP Json5String { get; set; }

    static StringParser() {
        var escapableChar = NoneOf("123456789");

        var escapeSequence =
            Skip('\\')
            .And(escapableChar)
            .And(c => c switch {
                'n' => Return("\n"),
                't' => Return("\t"),
                'r' => Return("\r"),
                'f' => Return("\f"),
                '0' => Return("\0"),
                'b' => Return("\b"),
                'v' => Return("\v"),
                'x' => HexEncodedAscii,
                'u' => HexEncodedUnicode,
                _ when BreakingWhitespaceChars.Contains(c) => NonBreakSpaces.Return(""),
                _ => Return(c.ToString())
            })
            .Lbl_("escape sequence");

        StringP ContentTilQuote(char quote) =>
            ManyStrings(
                ManyChars(NoneOf($"{quote}\\{BreakingWhitespaceChars}").Lbl("string character")),
                sep: escapeSequence)
            .And(Skip(quote))
            .Map(s => '"' + Encoder.Encode(s) + '"');

        Json5String = AnyOf("'\"").And(ContentTilQuote).Lbl("string");
    }
}
