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

/// <summary>Implements parsing of JSON5 numbers.</summary>
public static class NumberParser {
    /// <summary>Parses a JSON5 number literal and renders it as a JSON-compatible string.</summary>
    public static StringP Json5Number { get; set; } =
        NumberLiteral(NumLiteralOpts, label: "number")
        .Map(ConertToJsonNumLiteral);

    /// <summary>Specifies which parts are allowed in a number literal.</summary>
    public const NumberLiteralOptions NumLiteralOpts =
        AllowBinary | AllowHexadecimal
        | AllowMinusSign | AllowPlusSign
        | AllowFraction | AllowFractionWOIntegerPart
        | AllowExponent
        | AllowInfinity | AllowNaN;

    /// <summary>The culture to write number literals with after parsing. Is also used to parse
    /// binary and hexadecimal literals.</summary>
    public static CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

    /// <summary>
    /// Converts a parsed JSON5 number literal to a JSON-compatible number literal.
    /// </summary>
    /// <param name="nl">The parsed <see cref="FParsec.CharParsers.NumberLiteral"/>.</param>
    /// <returns>A JSON-compatible string of the input.</returns>
    public static string ConertToJsonNumLiteral(NumberLiteral nl) => nl switch {
        { IsInfinity: true, HasMinusSign: true } => '"' + double.NegativeInfinity.ToString(Culture) + '"',
        { IsInfinity: true } => '"' + double.PositiveInfinity.ToString(Culture) + '"',
        { IsNaN: true } => '"' + double.NaN.ToString(Culture) + '"',
        { IsHexadecimal: true } => ParseHexNum(nl).ToString(Culture),
        { IsBinary: true } => ParseBinNum(nl).ToString(Culture),
        _ => nl.Normalize().String,
    };

    /// <summary>
    /// Parses the string value of an arbitrarily sized hexadecimal
    /// <see cref="FParsec.CharParsers.NumberLiteral"/> as a <see cref="BigInteger"/>.
    /// </summary>
    /// <param name="nl">The parsed <see cref="FParsec.CharParsers.NumberLiteral"/>.</param>
    /// <returns>A <see cref="BigInteger"/>.</returns>
    public static BigInteger ParseHexNum(NumberLiteral nl) {
        var s = nl.HasMinusSign || nl.HasPlusSign ? nl.String[3..] : nl.String[2..];
        var sign = nl.HasMinusSign ? -1 : 1;
        // Prepend 0 so the most significant bit is never set and the result will always be positive.
        return BigInteger.TryParse('0' + s, NumberStyles.AllowHexSpecifier, Culture, out var val)
            ? sign * val
            : throw new NotSupportedException($"Format of the number literal {nl.String} is not supported.");
    }

    /// <summary>
    /// Parses the string value of an arbitrarily sized binary
    /// <see cref="FParsec.CharParsers.NumberLiteral"/> as a <see cref="BigInteger"/>.
    /// </summary>
    /// <param name="nl">The parsed <see cref="FParsec.CharParsers.NumberLiteral"/>.</param>
    /// <returns>A <see cref="BigInteger"/>.</returns>
    public static BigInteger ParseBinNum(NumberLiteral nl) {
        var s = nl.HasMinusSign || nl.HasPlusSign ? nl.String[3..] : nl.String[2..];
        var sign = nl.HasMinusSign ? -1 : 1;
        // Prepend 0 so the most significant bit is never set and the result will always be positive.
        return BigInteger.TryParse('0' + s, NumberStyles.AllowBinarySpecifier, Culture, out var val)
            ? sign * val
            : throw new NotSupportedException($"Format of the number literal {nl.String} is not supported.");
    }
}
