﻿#pragma warning disable IDE0065 // Misplaced using directive

using Microsoft.FSharp.Core;

using System.Text.Json.Nodes;

namespace Json5.Internal;

using FParsec.CSharp;

using System.Globalization;
using System.Numerics;

using static FParsec.CharParsers;
using static FParsec.CSharp.CharParsersCS;
using static FParsec.CSharp.PrimitivesCS;

using JsonNodeP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<JsonNode?>>;
using StringP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<string>>;

public static class NumberParser {
    public static JsonNodeP Json5Number { get; set; }

    public static StringP Json5NumberS { get; set; } = NumberLiteral(numLiteralOptsS, label: "number").Map(ParseNumLiteralS);

    public const NumberLiteralOptions numLiteralOpts =
        NumberLiteralOptions.AllowBinary
        | NumberLiteralOptions.AllowHexadecimal
        | NumberLiteralOptions.AllowMinusSign
        | NumberLiteralOptions.AllowPlusSign
        | NumberLiteralOptions.AllowFraction
        | NumberLiteralOptions.AllowFractionWOIntegerPart
        | NumberLiteralOptions.AllowExponent
        | NumberLiteralOptions.AllowInfinity
        | NumberLiteralOptions.AllowNaN
        | NumberLiteralOptions.AllowSuffix
        | NumberLiteralOptions.IncludeSuffixCharsInString;

    public const NumberLiteralOptions numLiteralOptsS =
        NumberLiteralOptions.AllowBinary
        | NumberLiteralOptions.AllowHexadecimal
        | NumberLiteralOptions.AllowMinusSign
        | NumberLiteralOptions.AllowPlusSign
        | NumberLiteralOptions.AllowFraction
        | NumberLiteralOptions.AllowFractionWOIntegerPart
        | NumberLiteralOptions.AllowExponent
        | NumberLiteralOptions.AllowInfinity
        | NumberLiteralOptions.AllowNaN;

    public static readonly CultureInfo invCult = CultureInfo.InvariantCulture;

    static NumberParser() {
        var infinitySymbol =
            Choice(
                CharP('∞', double.PositiveInfinity),
                StringP("+∞", double.PositiveInfinity),
                StringP("-∞", double.NegativeInfinity))
            .Map(x => (JsonNode?)x) // deferred conversion ensures a new JsonNode is created every time
            .Lbl("number");

        var numLiteral = NumberLiteral(numLiteralOpts, label: "number").Map(ParseNumLiteral);

        Json5Number = Choice(infinitySymbol, numLiteral);
    }

    public static JsonNode? ParseNumLiteral(NumberLiteral nl) => nl switch {
        { IsInfinity: true, HasMinusSign: true } => double.NegativeInfinity,
        { IsInfinity: true } => double.PositiveInfinity,
        { IsNaN: true } => double.NaN,
        { SuffixLength: > 0 } => ParseMoney(nl),
        { IsInteger: false } => ParseFloat(nl),
        { IsDecimal: true } => ParseIntDec(nl),
        { IsHexadecimal: true } => ParseIntHex(nl),
        { IsBinary: true } => ParseIntBin(nl),
        _ => throw new NotSupportedException($"Format of the number literal {nl.String} is not supported.")
    };

    public static string ParseNumLiteralS(NumberLiteral nl) => nl switch {
        { IsInfinity: true, HasMinusSign: true } => '"' + double.NegativeInfinity.ToString(invCult) + '"',
        { IsInfinity: true } => '"' + double.PositiveInfinity.ToString(invCult) + '"',
        { IsNaN: true } => '"' + double.NaN.ToString(invCult) + '"',
        { IsInteger: false } => nl.Normalize().String,
        { IsDecimal: true } => nl.Normalize().String,
        { IsHexadecimal: true } => ParseIntHexS(nl).ToString(invCult),
        { IsBinary: true } => ParseIntBinS(nl).ToString(invCult),
        _ => throw new NotSupportedException($"Format of the number literal {nl.String} is not supported.")
    };

    public static JsonNode ParseMoney(NumberLiteral nl) {
        if (nl.SuffixLength > 1 || nl.SuffixChar1 is not 'm' and not 'M')
            throw new NotSupportedException($"Format of the number literal {nl.String} is not supported.");

        var s = nl.String.AsSpan(..^1);
        const NumberStyles sty = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;
        return decimal.TryParse(s, sty, invCult, out var @decimal)
            ? @decimal
            : throw new FormatException($"Number literal {nl.String} could not be parsed as a decimal value.");
    }

    public static JsonNode ParseFloat(NumberLiteral nl) {
        var s = nl.String;
        const NumberStyles sty = NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign;
        return double.TryParse(s, sty, invCult, out var @double)
            ? @double
            : throw new FormatException($"Number literal {nl.String} could not be parsed as a double value.");
    }

    public static JsonNode ParseIntDec(NumberLiteral nl) {
        var s = nl.String;
        const NumberStyles sty = NumberStyles.AllowLeadingSign;
        var positive = !nl.HasMinusSign;
        return int.TryParse(s, sty, invCult, out var @int) ? @int
            : positive && uint.TryParse(s, sty, invCult, out var @uint) ? @uint
            : long.TryParse(s, sty, invCult, out var @long) ? @long
            : positive && ulong.TryParse(s, sty, invCult, out var @ulong) ? @ulong
            : Int128.TryParse(s, sty, invCult, out var @int128) ? JsonValue.Create(@int128)!
            : positive && UInt128.TryParse(s, sty, invCult, out var uint128) ? JsonValue.Create(uint128)!
            : JsonValue.Create(BigInteger.Parse(s, sty, invCult))!;
    }

    public static JsonNode ParseIntHex(NumberLiteral nl) {
        var s = nl.HasMinusSign || nl.HasPlusSign ? nl.String.AsSpan(3) : nl.String.AsSpan(2);
        var sign = nl.HasMinusSign ? -1 : 1;
        const NumberStyles sty = NumberStyles.AllowHexSpecifier;
        return s.Length switch {
            <= 8 => Parse4Bytes(s, sign, sty),
            <= 16 => Parse8Bytes(s, sign, sty),
            <= 32 => Parse16Bytes(s, sign, sty),
            _ => ParseVarBytes(s, sign, sty)
        };
    }

    public static string ParseIntHexS(NumberLiteral nl) {
        var s = nl.HasMinusSign || nl.HasPlusSign ? nl.String[3..] : nl.String[2..];
        var sign = nl.HasMinusSign ? -1 : 1;
        // Prepend 0 so the most significant bit is never set and the result will always be positive.
        var val = BigInteger.Parse('0' + s, NumberStyles.AllowHexSpecifier, invCult) * sign;
        return val.ToString(invCult);
    }

    public static string ParseIntBinS(NumberLiteral nl) {
        var s = nl.HasMinusSign || nl.HasPlusSign ? nl.String[3..] : nl.String[2..];
        var sign = nl.HasMinusSign ? -1 : 1;
        // Prepend 0 so the most significant bit is never set and the result will always be positive.
        var val = BigInteger.Parse('0' + s, NumberStyles.AllowBinarySpecifier, invCult) * sign;
        return val.ToString(invCult);
    }

    public static JsonNode ParseIntBin(NumberLiteral nl) {
        var s = nl.HasMinusSign || nl.HasPlusSign ? nl.String.AsSpan(3) : nl.String.AsSpan(2);
        var sign = nl.HasMinusSign ? -1 : 1;
        const NumberStyles sty = NumberStyles.AllowBinarySpecifier;
        return s.Length switch {
            <= 32 => Parse4Bytes(s, sign, sty),
            <= 64 => Parse8Bytes(s, sign, sty),
            <= 128 => Parse16Bytes(s, sign, sty),
            _ => ParseVarBytes(s, sign, sty)
        };
    }

    public static JsonNode Parse4Bytes(ReadOnlySpan<char> s, int sign, NumberStyles style) {
        var @int = int.Parse(s, style, invCult);
        return sign switch {
            < 0 when @int == int.MinValue => int.MinValue,
            < 0 when @int < 0 => (uint)@int * sign,
            > 0 when @int < 0 => (uint)@int,
            _ => (JsonNode)(@int * sign)
        };
    }

    public static JsonNode Parse8Bytes(ReadOnlySpan<char> s, int sign, NumberStyles style) {
        var @long = long.Parse(s, style, invCult);
        return sign switch {
            < 0 when @long == long.MinValue => long.MinValue,
            < 0 when @long < 0 => JsonValue.Create((ulong)@long * (Int128)sign)!,
            > 0 when @long < 0 => (ulong)@long,
            _ => @long * sign
        };
    }

    public static JsonNode Parse16Bytes(ReadOnlySpan<char> s, int sign, NumberStyles style) {
        var int128 = Int128.Parse(s, style, invCult);
        return sign switch {
            < 0 when int128 == Int128.MinValue => JsonValue.Create(Int128.MinValue)!,
            < 0 when int128 < 0 => JsonValue.Create((UInt128)int128 * (BigInteger)sign)!,
            > 0 when int128 < 0 => JsonValue.Create((UInt128)int128)!,
            _ => JsonValue.Create(int128 * sign)!
        };
    }

    // Internally prepends 0 to `s` so the most significant bit is never set and the result
    // of BigInteger.Parse() will always be positive.
    public static JsonNode ParseVarBytes(ReadOnlySpan<char> s, int sign, NumberStyles style)
        => JsonValue.Create(BigInteger.Parse($"0{s}", style, invCult) * sign)!;
}
