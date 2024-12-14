#pragma warning disable IDE0065 // Misplaced using directive

using FParsec.CSharp;

using Microsoft.FSharp.Core;

using System.Text.Json.Nodes;

using static FParsec.CSharp.CharParsersCS;
using static FParsec.CSharp.PrimitivesCS;

namespace Json5.Parsing;

using Chars = FParsec.CharStream<Unit>;
using CharR = FParsec.Reply<char>;
using StringR = FParsec.Reply<string>;
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

    private static readonly StringP escapedChar = CharP('\\').AndR(escapableChar)
        .Map(c => c switch {
            'n' => "\n",
            't' => "\t",
            '\n' => "",
            'r' => "\r",
            'f' => "\f",
            '0' => "\0",
            'b' => "\b",
            'v' => "\v",
            _ => c.ToString()
        })
        .Lbl_("escape sequence");

    private static readonly StringP doubleQuoteStrPart = ManyChars(NoneOf("\"\\\r\n").Lbl("any char (except \", \\, or newline)"));

    private static readonly StringP singleQuoteStrPart = ManyChars(NoneOf("'\\\r\n").Lbl("any char (except ', \\, or newline)"));

    private static readonly JsonNodeP jstring =
        Choice(
            Between('"', ManyStrings(doubleQuoteStrPart, sep: escapedChar), '"'),
            Between('\'', ManyStrings(singleQuoteStrPart, sep: escapedChar), '\''))
        .Map(s => (JsonNode?)s)
        .Lbl_("string");

    private static readonly JsonNodeP jnum = NumberLiteral(numLiteralOpts, label: "number").Map(ParseNumLiteral);

    private static readonly JsonNodeP json5 =
        Choice(
            jnull,
            jbool,
            infinitySymbol,
            jstring,
            jnum)
        .And(EOF);
}
