#pragma warning disable IDE0065 // Misplaced using directive

using Microsoft.FSharp.Core;

namespace Json5.Internal;

using FParsec.CSharp;

using System.Globalization;
using System.Numerics;

using static FParsec.CharParsers;
using static FParsec.CharParsers.NumberLiteralOptions;
using static FParsec.CSharp.CharParsersCS;
using static FParsec.CSharp.PrimitivesCS;

using StringP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<string>>;

public static class NumberParser {
    public static StringP Json5Number { get; set; } =
        NumberLiteral(NumLiteralOpts, label: "number")
        .Map(ParseNumLiteral);

    public const NumberLiteralOptions NumLiteralOpts =
        AllowBinary | AllowHexadecimal
        | AllowMinusSign | AllowPlusSign
        | AllowFraction | AllowFractionWOIntegerPart | AllowExponent
        | AllowInfinity | AllowNaN;

    public static CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

    public static string ParseNumLiteral(NumberLiteral nl) => nl switch {
        { IsInfinity: true, HasMinusSign: true } => '"' + double.NegativeInfinity.ToString(Culture) + '"',
        { IsInfinity: true } => '"' + double.PositiveInfinity.ToString(Culture) + '"',
        { IsNaN: true } => '"' + double.NaN.ToString(Culture) + '"',
        { IsInteger: false } => nl.Normalize().String,
        { IsDecimal: true } => nl.Normalize().String,
        { IsHexadecimal: true } => ParseHexNum(nl).ToString(Culture),
        { IsBinary: true } => ParseBinNum(nl).ToString(Culture),
        _ => throw new NotSupportedException($"Format of the number literal {nl.String} is not supported.")
    };

    public static string ParseHexNum(NumberLiteral nl) {
        var s = nl.HasMinusSign || nl.HasPlusSign ? nl.String[3..] : nl.String[2..];
        var sign = nl.HasMinusSign ? -1 : 1;
        // Prepend 0 so the most significant bit is never set and the result will always be positive.
        var val = BigInteger.Parse('0' + s, NumberStyles.AllowHexSpecifier, Culture) * sign;
        return val.ToString(Culture);
    }

    public static string ParseBinNum(NumberLiteral nl) {
        var s = nl.HasMinusSign || nl.HasPlusSign ? nl.String[3..] : nl.String[2..];
        var sign = nl.HasMinusSign ? -1 : 1;
        // Prepend 0 so the most significant bit is never set and the result will always be positive.
        var val = BigInteger.Parse('0' + s, NumberStyles.AllowBinarySpecifier, Culture) * sign;
        return val.ToString(Culture);
    }
}
