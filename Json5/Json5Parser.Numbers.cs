namespace Json5.Parsing;

using System.Globalization;
using System.Numerics;
using System.Text.Json.Nodes;

using static FParsec.CharParsers;

public static partial class Json5Parser {
    private const NumberLiteralOptions numLiteralOpts =
        NumberLiteralOptions.AllowBinary
        | NumberLiteralOptions.AllowHexadecimal
        | NumberLiteralOptions.AllowMinusSign
        | NumberLiteralOptions.AllowPlusSign
        | NumberLiteralOptions.AllowFraction
        | NumberLiteralOptions.AllowFractionWOIntegerPart
        | NumberLiteralOptions.AllowExponent
        | NumberLiteralOptions.AllowInfinity
        | NumberLiteralOptions.AllowNaN;

    private static readonly CultureInfo invCult = CultureInfo.InvariantCulture;

    private static JsonNode? ParseNumLiteral(NumberLiteral nl) => nl switch {
        { IsDecimal: true } => ParseIntDec(nl),
        { IsHexadecimal: true } => ParseIntHex(nl),
        { IsBinary: true } => ParseIntBin(nl),
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
            : JsonValue.Create(BigInteger.Parse(s, sty, invCult))!;
    }

    private static JsonValue ParseIntHex(NumberLiteral nl) {
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

    private static JsonValue ParseIntBin(NumberLiteral nl) {
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

    private static JsonValue Parse4Bytes(ReadOnlySpan<char> s, int sign, NumberStyles style) {
        var @int = int.Parse(s, style, invCult);
        return sign switch {
            < 0 when @int == int.MinValue => JsonValue.Create(int.MinValue),
            < 0 when @int < 0 => JsonValue.Create((uint)@int * sign),
            > 0 when @int < 0 => JsonValue.Create((uint)@int),
            _ => JsonValue.Create(@int * sign)
        };
    }

    private static JsonValue Parse8Bytes(ReadOnlySpan<char> s, int sign, NumberStyles style) {
        var @long = long.Parse(s, style, invCult);
        return sign switch {
            < 0 when @long == long.MinValue => JsonValue.Create(long.MinValue),
            < 0 when @long < 0 => JsonValue.Create((ulong)@long * (Int128)sign)!,
            > 0 when @long < 0 => JsonValue.Create((ulong)@long),
            _ => JsonValue.Create(@long * sign)
        };
    }

    private static JsonValue Parse16Bytes(ReadOnlySpan<char> s, int sign, NumberStyles style) {
        var int128 = Int128.Parse(s, style, invCult);
        return sign switch {
            < 0 when int128 == Int128.MinValue => JsonValue.Create(Int128.MinValue)!,
            < 0 when int128 < 0 => JsonValue.Create((UInt128)int128 * (BigInteger)sign)!,
            > 0 when int128 < 0 => JsonValue.Create((UInt128)int128)!,
            _ => JsonValue.Create(int128 * sign)!
        };
    }

    // Prepends 0 so the most significant bit is never set and the resulting bigint will always be positive.
    static JsonValue ParseVarBytes(ReadOnlySpan<char> s, int sign, NumberStyles style)
        => JsonValue.Create(BigInteger.Parse($"0{s}", style, invCult) * sign)!;
}
