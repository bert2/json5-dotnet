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

    private static readonly UnitP wsc = Purify(SkipMany(Choice(
        WS1,
        Skip("//").AndR(SkipRestOfLine(skipNewline: true)),
        Skip("/*").AndR(SkipCharsTillString("*/", maxCount: int.MaxValue, skipString: true)))));

    private static readonly JsonNodeP jnull = StringP<JsonNode?>("null", null).Lbl("null");

    private static readonly JsonNodeP jbool =
        Choice(
            StringP("true", true),
            StringP("false", false))
        .Map(x => (JsonNode?)x) // deferred conversion ensures a new JsonNode is created every time
        .Lbl("bool");

    private static readonly JsonNodeP infinitySymbol =
        Choice(
            CharP('∞', double.PositiveInfinity),
            StringP("+∞", double.PositiveInfinity),
            StringP("-∞", double.NegativeInfinity))
        .Map(x => (JsonNode?)x) // deferred conversion ensures a new JsonNode is created every time
        .Lbl("number");

    private static readonly JsonNodeP jstring =
        AnyOf("'\"")
        .And(PositionP.Map(p => p.Column))
        .And(ParseStringContent)
        .Map(s => (JsonNode?)s)
        .Lbl("string");

    private static readonly JsonNodeP jnumber = NumberLiteral(numLiteralOpts, label: "number").Map(ParseNumLiteral);

    private static readonly JsonNodeP jarray =
        Between(
            Skip('[').AndR(wsc),
            Many(Rec(() => jvalue), sep: Skip(',').AndR(wsc), canEndWithSep: true),
            Skip(']'))
        .Map(elems => (JsonNode?)new JsonArray([.. elems]))
        .Lbl("array");

    private static readonly JsonNodeP jvalue =
        Choice(
            jarray,
            jnull,
            jbool,
            infinitySymbol,
            jstring,
            jnumber)
        .And(wsc);

    private static readonly JsonNodeP json5 = wsc.And(jvalue).And(EOF);
}
