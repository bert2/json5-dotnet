#pragma warning disable IDE0065 // Misplaced using directive

using Microsoft.FSharp.Core;

namespace Json5.Internal;

using FParsec.CSharp;

using System.Text;

using static CommonParsers;
using static FParsec.CSharp.CharParsersCS;
using static FParsec.CSharp.PrimitivesCS;

using StringP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<string>>;

public static class StringParser {
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

        string Escape(string s) => s
            .Aggregate(new StringBuilder(), (sb, c) => c switch {
                '"' => sb.Append(@"\"""),
                '\\' => sb.Append(@"\\"),
                <= '\x1F' => sb.Append($@"\u{(int)c:X4}"),
                _ => sb.Append(c)
            })
            .ToString();

        StringP StringContentAndQuote(char quote) =>
            ManyStrings(
                ManyChars(NoneOf($"{quote}\\{BreakingWhitespaceChars}").Lbl("string character")),
                sep: escapeSequence)
            .And(Skip(quote))
            .Map(s => '"' + Escape(s) + '"');

        Json5String = AnyOf("'\"").And(StringContentAndQuote).Lbl("string");
    }
}
