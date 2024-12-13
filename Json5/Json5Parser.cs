#pragma warning disable IDE0065 // Misplaced using directive
#pragma warning disable IDE0130 // Namespace does not match folder structure

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

    private static readonly NodeP jtrue = StringP<JsonNode?>("true", true).Lbl("true");

    private static readonly NodeP jfalse = StringP<JsonNode?>("false", false).Lbl("false");

    private static readonly NodeP jnum =
        NumberLiteral(
            NumberLiteralOptions.AllowBinary
            | NumberLiteralOptions.AllowHexadecimal
            | NumberLiteralOptions.AllowMinusSign
            | NumberLiteralOptions.AllowPlusSign
            | NumberLiteralOptions.AllowFraction
            | NumberLiteralOptions.AllowFractionWOIntegerPart
            | NumberLiteralOptions.AllowExponent
            | NumberLiteralOptions.AllowInfinity
            | NumberLiteralOptions.AllowNaN,
            "number")
        .Map(ParseNumberLiteral);

    private static readonly NodeP json5 = Choice(
        jnull,
        jtrue,
        jfalse,
        jnum);
}
