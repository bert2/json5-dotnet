#pragma warning disable IDE0065 // Misplaced using directive

using FParsec.CSharp;

using Microsoft.FSharp.Core;

using System.Text.Json.Nodes;

using static FParsec.CSharp.CharParsersCS;
using static FParsec.CSharp.PrimitivesCS;

namespace Json5.Parsing;

using Chars = FParsec.CharStream<Unit>;
using UnitR = FParsec.Reply<Unit>;
using CharR = FParsec.Reply<char>;
using StringR = FParsec.Reply<string>;
using UnitP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<Unit>>;
using CharP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<char>>;
using StringP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<string>>;
using JsonNodeP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<JsonNode?>>;
using ParserResult = FParsec.CharParsers.ParserResult<JsonNode?, Unit>;

public static partial class Json5Parser {
    public static ParserResult Parse(string json) => json5.Run(json);

    private static readonly JsonNodeP jnull = StringP<JsonNode?>("null", null).Lbl("null");

    private static readonly JsonNodeP jbool = StringP<JsonNode?>("true", true).Or(StringP<JsonNode?>("false", false)).Lbl("bool");

    private static readonly JsonNodeP infinitySymbol =
        Choice(
            CharP<JsonNode?>('∞', double.PositiveInfinity),
            StringP<JsonNode?>("+∞", double.PositiveInfinity),
            StringP<JsonNode?>("-∞", double.NegativeInfinity))
        .Lbl("number");

    private static readonly CharP escapableChar = AnyOf("nt'\"\\\nrf0bv").Lbl("any char in ‘nt'\"\\rf0bv’ or newline");

    private static UnitP SkipIndent(long col) => SkipMany(
        PositionP
        .And(p => p.Column < col ? Return<Unit>(null!) : Zero<Unit>())
        .AndR(SkipAnyOf(" \t")));

    private static StringP EscapedChar(long indent) =>
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

    private static StringP quotedStr = FSharpFunc.From<Chars, StringR>(chars => {
        var (status, quote, err) = AnyOf("'\"").Invoke(chars);

        if (status != FParsec.ReplyStatus.Ok) return new(status, null!, err);

        var strStart = chars.Position.Column;

        var normalStrPart = ManyChars(NoneOf($"{quote}\\\r\n").Lbl($"any char (except {quote}, \\, or newline)"));

        var p = ManyStrings(normalStrPart, sep: EscapedChar(strStart)).And(Skip(quote));

        return p.Invoke(chars);
    });

    private static readonly JsonNodeP jstring =
        quotedStr
        .Map(s => (JsonNode?)s)
        .Lbl_("string");

    private static readonly JsonNodeP jnum = NumberLiteral(numLiteralOpts, label: "number").Map(ParseNumLiteral);

    private static readonly JsonNodeP json5 =
        WS
        .And(Choice(
            jnull,
            jbool,
            infinitySymbol,
            jstring,
            jnum))
        .And(EOF);
}
