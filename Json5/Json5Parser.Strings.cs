#pragma warning disable IDE0065 // Misplaced using directive

using Microsoft.FSharp.Core;

using static FParsec.CSharp.CharParsersCS;
using static FParsec.CSharp.PrimitivesCS;

namespace Json5.Parsing;

using Chars = FParsec.CharStream<Unit>;
using CharP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<char>>;
using StringP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<string>>;
using UnitP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<Unit>>;

public static partial class Json5Parser {
    private static readonly CharP escapableChar = NoneOf("123456789");

    private static UnitP SkipIndent(long col) => SkipMany(
        PositionP
        .And(p => p.Column < col ? Return<Unit>(null!) : Zero<Unit>())
        .AndR(SkipAnyOf(" \t")));

    private static StringP EscapedCharOrLineContinuation(long indent) =>
        Skip('\\')
        .And(escapableChar)
        .And(c => c switch {
            '\n' => SkipIndent(indent).Return(""),
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

    private static StringP ParseStringContent(char quote, long strStart) =>
        ManyStrings(
            ManyChars(NoneOf($"{quote}\\\n").Lbl($"any char (except {quote}, \\, or newline)")),
            sep: EscapedCharOrLineContinuation(strStart))
        .And(Skip(quote));
}
