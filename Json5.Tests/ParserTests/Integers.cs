#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests.ParserTests;

using FluentAssertions;

using Helpers;

using System.Numerics;

using static FluentAssertions.FluentActions;

public static partial class Integers {
    public partial class Bin {
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
        [Fact] void MinIntMinus1() => Parser.Parse("-0b10000000000000000000000000000001").Should().Be(int.MinValue - 1L);

        [Fact] void MaxInt() => Parser.Parse("0b1111111111111111111111111111111").Should().Be(int.MaxValue);
        [Fact] void MaxIntPlus1() => Parser.Parse("0b10000000000000000000000000000000").Should().Be(int.MaxValue + 1U);

        [Fact] void MaxUInt() => Parser.Parse("0b11111111111111111111111111111111").Should().Be(uint.MaxValue);
        [Fact] void MaxUIntPlus1() => Parser.Parse("0b100000000000000000000000000000000").Should().Be(uint.MaxValue + 1L);

        [Fact] void MinLong() => Parser.Parse("-0b1000000000000000000000000000000000000000000000000000000000000000").Should().Be(long.MinValue);
        [Fact] void MinLongMinus1() => Parser.Parse("-0b1000000000000000000000000000000000000000000000000000000000000001").Should().Be((Int128)long.MinValue - 1);

        [Fact] void MaxLong() => Parser.Parse("0b111111111111111111111111111111111111111111111111111111111111111").Should().Be(long.MaxValue);
        [Fact] void MaxLongPlus1() => Parser.Parse("0b1000000000000000000000000000000000000000000000000000000000000000").Should().Be(long.MaxValue + 1UL);

        [Fact] void MaxULong() => Parser.Parse("0b1111111111111111111111111111111111111111111111111111111111111111").Should().Be(ulong.MaxValue);
        [Fact] void MaxULongPlus1() => Parser.Parse("0b10000000000000000000000000000000000000000000000000000000000000000").Should().Be((Int128)ulong.MaxValue + 1);

        [Fact] void MinInt128() => Parser.Parse("-0b10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000").Should().Be(Int128.MinValue);
        [Fact] void MinInt128Minus1() => Parser.Parse("-0b10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001").Should().Be((BigInteger)Int128.MinValue - 1);

        [Fact] void MaxInt128() => Parser.Parse("0b1111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111").Should().Be(Int128.MaxValue);
        [Fact] void MaxInt128Plus1() => Parser.Parse("0b10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000").Should().Be((UInt128)Int128.MaxValue + 1);

        [Fact] void MaxUInt128() => Parser.Parse("0b11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111").Should().Be(UInt128.MaxValue);
        [Fact] void MaxUInt128Plus1() => Parser.Parse("0b100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000").Should().Be((BigInteger)UInt128.MaxValue + 1);
    }

    public partial class Dec {
        [Fact] void Positive() => Parser.Parse("+546").Should().Be(546);
        [Fact] void Negative() => Parser.Parse("-1656").Should().Be(-1656);
        [Fact] void LeadingZeros() => Parser.Parse("00123").Should().Be(123);

        [Fact] void MinInt() => Parser.Parse("-2147483648").Should().Be(int.MinValue);
        [Fact] void MinIntMinus1() => Parser.Parse("-2147483649").Should().Be(int.MinValue - 1L);

        [Fact] void MaxInt() => Parser.Parse("2147483647").Should().Be(int.MaxValue);
        [Fact] void MaxIntPlus1() => Parser.Parse("2147483648").Should().Be(int.MaxValue + 1U);

        [Fact] void MaxUInt() => Parser.Parse("4294967295").Should().Be(uint.MaxValue);
        [Fact] void MaxUIntPlus1() => Parser.Parse("4294967296").Should().Be(uint.MaxValue + 1L);

        [Fact] void MinLong() => Parser.Parse("-9223372036854775808").Should().Be(long.MinValue);
        [Fact] void MinLongMinus1() => Parser.Parse("-9223372036854775809").Should().Be((Int128)long.MinValue - 1);

        [Fact] void MaxLong() => Parser.Parse("9223372036854775807").Should().Be(long.MaxValue);
        [Fact] void MaxLongPlus1() => Parser.Parse("9223372036854775808").Should().Be(long.MaxValue + 1UL);

        [Fact] void MaxULong() => Parser.Parse("18446744073709551615").Should().Be(ulong.MaxValue);
        [Fact] void MaxULongPlus1() => Parser.Parse("18446744073709551616").Should().Be((Int128)ulong.MaxValue + 1);

        [Fact] void MinInt128() => Parser.Parse("-170141183460469231731687303715884105728").Should().Be(Int128.MinValue);
        [Fact] void MinInt128Minus1() => Parser.Parse("-170141183460469231731687303715884105729").Should().Be((BigInteger)Int128.MinValue - 1);

        [Fact] void MaxInt128() => Parser.Parse("170141183460469231731687303715884105727").Should().Be(Int128.MaxValue);
        [Fact] void MaxInt128Plus1() => Parser.Parse("170141183460469231731687303715884105728").Should().Be((UInt128)Int128.MaxValue + 1);

        [Fact] void MaxUInt128() => Parser.Parse("340282366920938463463374607431768211455").Should().Be(UInt128.MaxValue);
        [Fact] void MaxUInt128Plus1() => Parser.Parse("340282366920938463463374607431768211456").Should().Be((BigInteger)UInt128.MaxValue + 1);
    }

    public partial class Hex {
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

        [Fact] void MinInt() => Parser.Parse("-0x80000000").Should().Be(int.MinValue);
        [Fact] void MinIntMinus1() => Parser.Parse("-0x80000001").Should().Be(int.MinValue - 1L);

        [Fact] void MaxInt() => Parser.Parse("0x7fffffff").Should().Be(int.MaxValue);
        [Fact] void MaxIntPlus1() => Parser.Parse("0x80000000").Should().Be(int.MaxValue + 1U);

        [Fact] void MaxUInt() => Parser.Parse("0xffffffff").Should().Be(uint.MaxValue);
        [Fact] void MaxUIntPlus1() => Parser.Parse("0x100000000").Should().Be(uint.MaxValue + 1L);

        [Fact] void MinLong() => Parser.Parse("-0x8000000000000000").Should().Be(long.MinValue);
        [Fact] void MinLongMinus1() => Parser.Parse("-0x8000000000000001").Should().Be  ((Int128)long.MinValue - 1);

        [Fact] void MaxLong() => Parser.Parse("0x7fffffffffffffff").Should().Be(long.MaxValue);
        [Fact] void MaxLongPlus1() => Parser.Parse("0x8000000000000000").Should().Be(long.MaxValue + 1UL);

        [Fact] void MaxULong() => Parser.Parse("0xffffffffffffffff").Should().Be(ulong.MaxValue);
        [Fact] void MaxULongPlus1() => Parser.Parse("0x10000000000000000").Should().Be((Int128)ulong.MaxValue + 1);

        [Fact] void MinInt128() => Parser.Parse("-0x80000000000000000000000000000000").Should().Be(Int128.MinValue);
        [Fact] void MinInt128Minus1() => Parser.Parse("-0x80000000000000000000000000000001").Should().Be((BigInteger)Int128.MinValue - 1);

        [Fact] void MaxInt128() => Parser.Parse("0x7fffffffffffffffffffffffffffffff").Should().Be(Int128.MaxValue);
        [Fact] void MaxInt128Plus1() => Parser.Parse("0x80000000000000000000000000000000").Should().Be((UInt128)Int128.MaxValue + 1);

        [Fact] void MaxUInt128() => Parser.Parse("0xffffffffffffffffffffffffffffffff").Should().Be(UInt128.MaxValue);
        [Fact] void MaxUInt128Plus1() => Parser.Parse("0x100000000000000000000000000000000").Should().Be((BigInteger)UInt128.MaxValue + 1);
    }
}
