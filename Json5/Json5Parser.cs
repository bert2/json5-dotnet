#pragma warning disable IDE0065 // Misplaced using directive

using FParsec.CSharp;

using Microsoft.FSharp.Core;

using System.Text.Json.Nodes;

using static FParsec.CSharp.CharParsersCS;
using static FParsec.CSharp.PrimitivesCS;

namespace Json5.Parsing;

using Chars = FParsec.CharStream<Unit>;
using CharP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<char>>;
using CharR = FParsec.Reply<char>;
using StringR = FParsec.Reply<string>;
using StringP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<string>>;
using NodeP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<JsonNode?>>;
using ParserResult = FParsec.CharParsers.ParserResult<JsonNode?, Unit>;

public static partial class Json5Parser {
    public static ParserResult Parse(string json) => json5.Run(json);

    private static readonly NodeP jnull = StringP<JsonNode?>("null", null).Lbl("null");

    private static readonly NodeP jbool =
        Choice(
            StringP<JsonNode?>("true", true),
            StringP<JsonNode?>("false", false))
        .Lbl("bool");

    private static readonly NodeP jinf =
        Choice(
            CharP<JsonNode?>('∞', double.PositiveInfinity),
            StringP<JsonNode?>("+∞", double.PositiveInfinity),
            StringP<JsonNode?>("-∞", double.NegativeInfinity))
        .Lbl("number");

    private static readonly StringP doubleQuoteStr = Between('"', ManyChars(c => c is not '"') , '"');

    private static readonly StringP singleQuoteStr = Between('\'', ManyChars(c => c is not '\''), '\'');

    private static readonly NodeP jstring = Choice(doubleQuoteStr, singleQuoteStr).Map(s => (JsonNode?)s).Lbl("string");

    private static readonly NodeP jnum = NumberLiteral(numLiteralOpts, label: "number").Map(ParseNumLiteral);

    private static readonly NodeP json5 =
        Choice(
            jnull,
            jbool,
            jinf,
            jstring,
            jnum)
        .And(EOF);
}
