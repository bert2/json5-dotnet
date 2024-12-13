#pragma warning disable IDE0065 // Misplaced using directive

using FParsec;
using FParsec.CSharp;

using Microsoft.FSharp.Core;

using System.Text.Json.Nodes;

using static FParsec.CharParsers;
using static FParsec.CSharp.CharParsersCS;
using static FParsec.CSharp.PrimitivesCS;

namespace Json5.Parsing;

using NodeP = FSharpFunc<CharStream<Unit>, Reply<JsonNode?>>;

public static partial class Json5Parser {
    public static ParserResult<JsonNode?, Unit> Parse(string json) => json5.Run(json);

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

    private static readonly NodeP jnum = NumberLiteral(numLiteralOpts, label: "number").Map(ParseNumLiteral);

    private static readonly NodeP json5 =
        Choice(
            jnull,
            jbool,
            jinf,
            jnum)
        .And(EOF);
}
