#pragma warning disable IDE0065 // Misplaced using directive

using Microsoft.FSharp.Core;

namespace Json5.Parsing;

using static FParsec.CSharp.CharParsersCS;
using static FParsec.CSharp.PrimitivesCS;
using static System.Buffers.Binary.BinaryPrimitives;

using Chars = FParsec.CharStream<Unit>;
using CharP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<char>>;
using StringP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<string>>;
using UnitP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<Unit>>;

public static partial class Json5Parser {
    private static readonly CharP escapableChar = NoneOf("123456789");

    private static readonly UnitP nonBreakSpaces = Purify(SkipMany(AnyOf("\t\v \u00A0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A\u202F\u205F\u3000\uFEFF")));

    private static readonly StringP hexEncodedAscii = Array(2, Hex)
        .Map(x => new string((char)Convert.FromHexString(x)[0], 1));

    private static readonly StringP hexEncodedUnicode = Array(4, Hex)
        .Map(x => new string((char)ReadUInt16BigEndian(Convert.FromHexString(x)), 1));

    private static readonly StringP escapeSequence =
        Skip('\\')
        .And(escapableChar)
        .And(c => c switch {
            '\n' or '\u2028' or '\u2029' => nonBreakSpaces.Return(""),
            'x' => hexEncodedAscii,
            'u' => hexEncodedUnicode,
            'n' => Return("\n"),
            't' => Return("\t"),
            'r' => Return("\r"),
            'f' => Return("\f"),
            '0' => Return("\0"),
            'b' => Return("\b"),
            'v' => Return("\v"),
            _ => Return(c.ToString())
        })
        .Lbl_("escape sequence");

    private static StringP ParseStringContent(char quote) =>
        ManyStrings(
            ManyChars(NoneOf($"{quote}\\\n").Lbl("next string character")),
            sep: escapeSequence)
        .And(Skip(quote));
}
