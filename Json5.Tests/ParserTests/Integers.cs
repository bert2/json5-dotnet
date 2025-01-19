#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests.ParserTests;

using FluentAssertions;

using Helpers;

using System.Numerics;
using System.Text.Json;

using static FluentAssertions.FluentActions;

public static class Integers {
    // required to deserialize integers beyond Int128.MinValue and UInt128.MaxValue
    static readonly JsonSerializerOptions opts = new() { Converters = { new BigIntegerJsonConverter() } };

    public class Bin {
        [Fact] void LowerCaseFormatSpecifier() => Parser.Parse("0b010101").Should().Be(21);
        [Fact] void UpperCaseFormatSpecifier() => Parser.Parse("0B010101").Should().Be(21);
        [Fact] void Positive() => Parser.Parse("+0b1101010").Should().Be(106);
        [Fact] void Negative() => Parser.Parse("-0b1101110001").Should().Be(-881);
        [Fact] void LeadingZeros() => Parser.Parse("0b000000001").Should().Be(1);

        [Fact]
        void Empty() =>
            Invoking(() => Parser.Parse("0b"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 3
                0b
                  ^
                Note: The error occurred at the end of the input stream.
                Expecting: binary digit
                """);

        [Fact] void MinInt() => Parser.Parse("-0b10000000000000000000000000000000").Should().Be(int.MinValue);
        [Fact] void MaxInt() => Parser.Parse("0b1111111111111111111111111111111").Should().Be(int.MaxValue);
        [Fact] void MaxUInt() => Parser.Parse("0b11111111111111111111111111111111").Should().Be(uint.MaxValue);
        [Fact] void MinLong() => Parser.Parse("-0b1000000000000000000000000000000000000000000000000000000000000000").Should().Be(long.MinValue);
        [Fact] void MaxLong() => Parser.Parse("0b111111111111111111111111111111111111111111111111111111111111111").Should().Be(long.MaxValue);
        [Fact] void MaxULong() => Parser.Parse("0b1111111111111111111111111111111111111111111111111111111111111111").Should().Be(ulong.MaxValue);
        [Fact] void MinInt128() => Parser.Parse("-0b10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000").Should().Be(Int128.MinValue);
        [Fact] void MaxInt128() => Parser.Parse("0b1111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111").Should().Be(Int128.MaxValue);
        [Fact] void MaxUInt128() => Parser.Parse("0b11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111").Should().Be(UInt128.MaxValue);
        [Fact] void PositiveBigInteger() => Parser.Parse("0b100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001001001100101100000001011010001").Should().Be((BigInteger)UInt128.MaxValue + 1234567890, opts);
        [Fact] void NegativeBigInteger() => Parser.Parse("-0b10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001001001100101100000001011010010").Should().Be((BigInteger)Int128.MinValue - 1234567890, opts);
    }

    public class Dec {
        [Fact] void Positive() => Parser.Parse("+546").Should().Be(546);
        [Fact] void Negative() => Parser.Parse("-1656").Should().Be(-1656);
        [Fact] void LeadingZeros() => Parser.Parse("00123").Should().Be(123);

        [Fact] void MinInt() => Parser.Parse("-2147483648").Should().Be(int.MinValue);
        [Fact] void MaxInt() => Parser.Parse("2147483647").Should().Be(int.MaxValue);
        [Fact] void MaxUInt() => Parser.Parse("4294967295").Should().Be(uint.MaxValue);
        [Fact] void MinLong() => Parser.Parse("-9223372036854775808").Should().Be(long.MinValue);
        [Fact] void MaxLong() => Parser.Parse("9223372036854775807").Should().Be(long.MaxValue);
        [Fact] void MaxULong() => Parser.Parse("18446744073709551615").Should().Be(ulong.MaxValue);
        [Fact] void MinInt128() => Parser.Parse("-170141183460469231731687303715884105728").Should().Be(Int128.MinValue);
        [Fact] void MaxInt128() => Parser.Parse("170141183460469231731687303715884105727").Should().Be(Int128.MaxValue);
        [Fact] void MaxUInt128() => Parser.Parse("340282366920938463463374607431768211455").Should().Be(UInt128.MaxValue);
        [Fact] void PositiveBigInteger() => Parser.Parse("340282366920938463463374607433002779345").Should().Be((BigInteger)UInt128.MaxValue + 1234567890, opts);
        [Fact] void NegativeBigInteger() => Parser.Parse("-170141183460469231731687303717118673618").Should().Be((BigInteger)Int128.MinValue - 1234567890, opts);
    }

    public class Hex {
        [Fact] void LowerCaseFormatSpecifier() => Parser.Parse("0xABCDEF").Should().Be(11259375);
        [Fact] void UpperCaseFormatSpecifier() => Parser.Parse("0XABCDEF").Should().Be(11259375);
        [Fact] void Positive() => Parser.Parse("+0x0023FF").Should().Be(9215);
        [Fact] void Negative() => Parser.Parse("-0x12A").Should().Be(-298);
        [Fact] void LeadingZeros() => Parser.Parse("0x000B").Should().Be(11);
        [Fact] void WithFakeExponent() => Parser.Parse("0xC8e4").Should().Be(51428);

        [Fact]
        void Empty() =>
            Invoking(() => Parser.Parse("0x"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 3
                0x
                  ^
                Note: The error occurred at the end of the input stream.
                Expecting: hexadecimal digit
                """);

        [Fact]
        void FractionsNotSupported() =>
            Invoking(() => Parser.Parse("0x0.ABC"))
            .Should().Throw<Exception>().WithMessage("Format of the number literal 0x0.ABC is not supported.");

        [Fact]
        void ExponentsNotSupported() =>
            Invoking(() => Parser.Parse("0x1p-3"))
            .Should().Throw<Exception>().WithMessage("Format of the number literal 0x1p-3 is not supported.");

        [Fact] void MinInt() => Parser.Parse("-0x80000000").Should().Be(int.MinValue);
        [Fact] void MaxInt() => Parser.Parse("0x7fffffff").Should().Be(int.MaxValue);
        [Fact] void MaxUInt() => Parser.Parse("0xffffffff").Should().Be(uint.MaxValue);
        [Fact] void MinLong() => Parser.Parse("-0x8000000000000000").Should().Be(long.MinValue);
        [Fact] void MaxLong() => Parser.Parse("0x7fffffffffffffff").Should().Be(long.MaxValue);
        [Fact] void MaxULong() => Parser.Parse("0xffffffffffffffff").Should().Be(ulong.MaxValue);
        [Fact] void MinInt128() => Parser.Parse("-0x80000000000000000000000000000000").Should().Be(Int128.MinValue);
        [Fact] void MaxInt128() => Parser.Parse("0x7fffffffffffffffffffffffffffffff").Should().Be(Int128.MaxValue);
        [Fact] void MaxUInt128() => Parser.Parse("0xffffffffffffffffffffffffffffffff").Should().Be(UInt128.MaxValue);
        [Fact] void PositiveBigInteger() => Parser.Parse("0x1000000000000000000000000499602D1").Should().Be((BigInteger)UInt128.MaxValue + 1234567890, opts);
        [Fact] void NegativeBigInteger() => Parser.Parse("-0x800000000000000000000000499602D2").Should().Be((BigInteger)Int128.MinValue - 1234567890, opts);
    }
}
