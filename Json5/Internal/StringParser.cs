#pragma warning disable IDE0065 // Misplaced using directive

using Microsoft.FSharp.Core;

using System.Text.Json.Nodes;

namespace Json5.Internal;

using FParsec.CSharp;

using Microsoft.Extensions.Primitives;

using System.Text;

using static CommonParsers;
using static FParsec.CSharp.CharParsersCS;
using static FParsec.CSharp.PrimitivesCS;

using JsonNodeP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<JsonNode?>>;
using StringP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<string>>;

public static class StringParser {
    public static JsonNodeP Json5String { get; set; }

    public static StringP Json5StringS { get; set; }

    public static StringP RawString { get; set; }

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

        StringP StringContent(char quote) =>
            ManyStrings(
                ManyChars(NoneOf($"{quote}\\{BreakingWhitespaceChars}").Lbl("string character")),
                sep: escapeSequence)
            .And(Skip(quote));

        RawString = AnyOf("'\"").And(StringContent).Lbl("string");
        Json5String = RawString.Map(s => (JsonNode?)s);

        string Escape(string s) => s
            .Aggregate(new StringBuilder(), (sb, c) => c switch {
                '"' => sb.Append(@"\"""),
                '\\' => sb.Append(@"\\"),
                <= '\x1F' => sb.Append($@"\u{(int)c:X4}"),
                _ => sb.Append(c)
            })
            .ToString();

        StringP StringContentS(char quote) =>
            ManyStrings(
                ManyChars(NoneOf($"{quote}\\{BreakingWhitespaceChars}").Lbl("string character")),
                sep: escapeSequence)
            .And(Skip(quote))
            .Map(s => '"' + Escape(s) + '"');

        Json5StringS = AnyOf("'\"").And(StringContentS).Lbl("string");
    }
}
