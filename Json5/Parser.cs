#pragma warning disable IDE0065 // Misplaced using directive
#pragma warning disable IDE0130 // Namespace does not match folder structure

using FParsec;
using FParsec.CSharp;

using Microsoft.FSharp.Core;

using System.Globalization;
using System.Numerics;
using System.Text.Json.Nodes;

using static FParsec.CharParsers;
using static FParsec.CSharp.CharParsersCS;
using static FParsec.CSharp.PrimitivesCS;

namespace Json5.Parsing;

using NodeP = FSharpFunc<CharStream<Unit>, Reply<JsonNode?>>;

public static class Parser {
    public static ParserResult<JsonNode?, Unit> Parse(string s) => json5.Run(s);

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
        .Map(nl => {
            if (nl.IsDecimal) return ParseIntDec(nl);
            if (nl.IsBinary) return ParseIntBin(nl);

            var lit = nl.GetStringWithoutPrefixes();
            var style = nl.ToNumberStyle();
            var sign = nl.GetSign();
            var positive = sign > 0;
            return int.TryParse(lit, style, null, out var @int) && @int > -1 ? CreateNode(@int * sign)
                : long.TryParse(lit, style, null, out var @long) && @long > -1 ? CreateNode(@long * sign)
                : positive && ulong.TryParse(lit, style, null, out var @ulong) ? CreateNode(@ulong)
                : CreateNode(ParseBigInt(lit, style) * sign);
        });

    private static readonly NodeP json5 = Choice(
        jnull,
        jtrue,
        jfalse,
        jnum);

    private static JsonValue ParseIntDec(NumberLiteral nl) {
        var lit = nl.String;
        var cult = CultureInfo.InvariantCulture;
        const NumberStyles style = NumberStyles.AllowLeadingSign;
        return int.TryParse(lit, style, cult, out var @int) ? JsonValue.Create(@int)
            : long.TryParse(lit, style, cult, out var @long) ? JsonValue.Create(@long)
            : !nl.HasMinusSign && ulong.TryParse(lit, style, cult, out var @ulong) ? JsonValue.Create(@ulong)
            : Int128.TryParse(lit, style, cult, out var @int128) ? JsonValue.Create(@int128)!
            : JsonValue.Create(BigInteger.Parse(lit, style))!;
    }

    private static JsonValue ParseIntBin(NumberLiteral nl) {
        var lit = nl.HasMinusSign || nl.HasPlusSign ? nl.String.AsSpan(3) : nl.String.AsSpan(2);
        const NumberStyles style = NumberStyles.AllowBinarySpecifier;
        var sign = nl.HasMinusSign ? -1 : 1;
        var positive = sign > 0;
        return int.TryParse(lit, style, null, out var @int) && @int > -1 ? JsonValue.Create(@int * sign)
            : long.TryParse(lit, style, null, out var @long) && @long > -1 ? JsonValue.Create(@long * sign)
            : positive && ulong.TryParse(lit, style, null, out var @ulong) ? JsonValue.Create(@ulong)
            : JsonValue.Create(ParseBigInt(lit, style) * sign)!;
    }

    private static int GetSign(this NumberLiteral nl) => nl.HasMinusSign ? -1 : 1;

    private static NumberStyles ToNumberStyle(this NumberLiteral nl) => nl switch {
      { IsBinary: true } => NumberStyles.AllowBinarySpecifier,
      { IsHexadecimal: true } => NumberStyles.AllowHexSpecifier,
        _ => NumberStyles.None
    };

    private static ReadOnlySpan<char> GetStringWithoutPrefixes(this NumberLiteral nl) {
        var signed = nl.HasMinusSign || nl.HasPlusSign;
        var prefixed = !nl.IsDecimal;
        var start = (signed ? 1 : 0) + (prefixed ? 2 : 0);
        return nl.String.AsSpan(start);
    }

    private static JsonNode? CreateNode<T>(T v) => JsonValue.Create(v);

    // Prepends 0 so the most significant bit is always 0 and the resulting bigint will always be positive.
    private static BigInteger ParseBigInt(ReadOnlySpan<char> lit, NumberStyles style) => BigInteger.Parse($"0{lit}", style);
}
