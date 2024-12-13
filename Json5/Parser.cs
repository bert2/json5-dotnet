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
        .Map(ParseNumberLiteral);

    private static readonly NodeP json5 = Choice(
        jnull,
        jtrue,
        jfalse,
        jnum);

    private static readonly CultureInfo invCult = CultureInfo.InvariantCulture;

    private static JsonNode? ParseNumberLiteral(NumberLiteral nl) => nl switch {
        { IsDecimal: true } => ParseIntDec(nl),
        { IsBinary: true } => ParseIntBin(nl),
        { IsHexadecimal: true } => ParseIntHex(nl),
        _ => throw new NotSupportedException($"Format of the number literal {nl.String} is not supported.")
    };

    private static JsonValue ParseIntDec(NumberLiteral nl) {
        var s = nl.String;
        const NumberStyles sty = NumberStyles.AllowLeadingSign;
        var positive = !nl.HasMinusSign;
        return int.TryParse(s, sty, invCult, out var @int) ? JsonValue.Create(@int)
            : positive && uint.TryParse(s, sty, invCult, out var @uint) ? JsonValue.Create(@uint)
            : long.TryParse(s, sty, invCult, out var @long) ? JsonValue.Create(@long)
            : positive && ulong.TryParse(s, sty, invCult, out var @ulong) ? JsonValue.Create(@ulong)
            : Int128.TryParse(s, sty, invCult, out var @int128) ? JsonValue.Create(@int128)!
            : positive && UInt128.TryParse(s, sty, invCult, out var uint128) ? JsonValue.Create(uint128)!
            : JsonValue.Create(BigInteger.Parse(s, sty))!;
    }

    private static JsonValue ParseIntBin(NumberLiteral nl) {
        var s = nl.HasMinusSign || nl.HasPlusSign ? nl.String.AsSpan(3) : nl.String.AsSpan(2);
        var sign = nl.HasMinusSign ? -1 : 1;
        return s.Length switch {
            <= 32 => Parse4Bytes(s, sign, NumberStyles.AllowBinarySpecifier),
            <= 64 => Parse8Bytes(s, sign, NumberStyles.AllowBinarySpecifier),
            <= 128 => Parse16Bytes(s, sign, NumberStyles.AllowBinarySpecifier),
            _ => ParseVarBytes(s, sign, NumberStyles.AllowBinarySpecifier)
        };
    }

    private static JsonValue ParseIntHex(NumberLiteral nl) {
        var s = nl.HasMinusSign || nl.HasPlusSign ? nl.String.AsSpan(3) : nl.String.AsSpan(2);
        var sign = nl.HasMinusSign ? -1 : 1;
        return s.Length switch {
            <= 8 => Parse4Bytes(s, sign, NumberStyles.AllowHexSpecifier),
            <= 16 => Parse8Bytes(s, sign, NumberStyles.AllowHexSpecifier),
            <= 32 => Parse16Bytes(s, sign, NumberStyles.AllowHexSpecifier),
            _ => ParseVarBytes(s, sign, NumberStyles.AllowHexSpecifier)
        };
    }

    private static JsonValue Parse4Bytes(ReadOnlySpan<char> s, int sign, NumberStyles style) {
        var @int = int.Parse(s, style, invCult);

        if (sign < 0 && @int == int.MinValue)
            return JsonValue.Create(int.MinValue);
        else if (@int < 0)
            return sign < 0 ? JsonValue.Create(unchecked((uint)@int) * sign) : JsonValue.Create(unchecked((uint)@int));
        else
            return JsonValue.Create(@int * sign)!;
    }

    private static JsonValue Parse8Bytes(ReadOnlySpan<char> s, int sign, NumberStyles style) {
        var @long = long.Parse(s, style, invCult);

        if (sign < 0 && @long == long.MinValue)
            return JsonValue.Create(long.MinValue);
        else if (@long < 0)
            return sign < 0 ? JsonValue.Create(unchecked((ulong)@long) * (Int128)sign)! : JsonValue.Create(unchecked((ulong)@long));
        else
            return JsonValue.Create(@long * sign)!;
    }

    private static JsonValue Parse16Bytes(ReadOnlySpan<char> s, int sign, NumberStyles style) {
        var int128 = Int128.Parse(s, style, invCult);

        if (sign < 0 && int128 == Int128.MinValue)
            return JsonValue.Create(Int128.MinValue)!;
        else if (int128 < 0)
            return sign < 0 ? JsonValue.Create(unchecked((UInt128)int128) * (BigInteger)sign)! : JsonValue.Create(unchecked((UInt128)int128))!;
        else
            return JsonValue.Create(int128 * sign)!;
    }

    // Prepends 0 so the most significant bit is always 0 and the resulting bigint will always be positive.
    static JsonValue ParseVarBytes(ReadOnlySpan<char> s, int sign, NumberStyles style)
        => JsonValue.Create(BigInteger.Parse($"0{s}", style, invCult) * sign)!;
}
