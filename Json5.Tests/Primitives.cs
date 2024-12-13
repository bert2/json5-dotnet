#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests;

using FluentAssertions;

using System.Numerics;

using static FluentAssertions.FluentActions;

public partial class Primitives {
    [Fact] void Null() => Json5.Parse("null").Should().BeNull();

    public class Booleans {
        [Fact] void True() => Json5.Parse("true").Should().Be(true);
        [Fact] void False() => Json5.Parse("false").Should().Be(false);
    }

    public static partial class Integers {
        public partial class Bin {
            [Fact] void IgnoreCase() => Json5.Parse("0b010101").Should().Be(Json5.Parse("0B010101"));
            [Fact] void Positive() => Json5.Parse("+0b1101010").Should().BeValue(106);
            [Fact] void Negative() => Json5.Parse("-0b1101110001").Should().BeValue(-881);
            [Fact] void LeadingZeros() => Json5.Parse("0b000000001").Should().BeValue(1);

            [Fact] void MinInt() => Json5.Parse("-0b10000000000000000000000000000000").Should().BeValue(int.MinValue);
            [Fact] void MinIntMinus1() => Json5.Parse("-0b10000000000000000000000000000001").Should().BeValue(int.MinValue - 1L);

            [Fact] void MaxInt() => Json5.Parse("0b1111111111111111111111111111111").Should().BeValue(int.MaxValue);
            [Fact] void MaxIntPlus1() => Json5.Parse("0b10000000000000000000000000000000").Should().BeValue(int.MaxValue + 1U);

            [Fact] void MaxUInt() => Json5.Parse("0b11111111111111111111111111111111").Should().BeValue(uint.MaxValue);
            [Fact] void MaxUIntPlus1() => Json5.Parse("0b100000000000000000000000000000000").Should().BeValue(uint.MaxValue + 1L);

            [Fact] void MinLong() => Json5.Parse("-0b1000000000000000000000000000000000000000000000000000000000000000").Should().BeValue(long.MinValue);
            [Fact] void MinLongMinus1() => Json5.Parse("-0b1000000000000000000000000000000000000000000000000000000000000001").Should().BeValue((Int128)long.MinValue - 1);

            [Fact] void MaxLong() => Json5.Parse("0b111111111111111111111111111111111111111111111111111111111111111").Should().BeValue(long.MaxValue);
            [Fact] void MaxLongPlus1() => Json5.Parse("0b1000000000000000000000000000000000000000000000000000000000000000").Should().BeValue(long.MaxValue + 1UL);

            [Fact] void MaxULong() => Json5.Parse("0b1111111111111111111111111111111111111111111111111111111111111111").Should().BeValue(ulong.MaxValue);
            [Fact] void MaxULongPlus1() => Json5.Parse("0b10000000000000000000000000000000000000000000000000000000000000000").Should().BeValue((Int128)ulong.MaxValue + 1);

            [Fact] void MinInt128() => Json5.Parse("-0b10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000").Should().BeValue(Int128.MinValue);
            [Fact] void MinInt128Minus1() => Json5.Parse("-0b10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001").Should().BeValue((BigInteger)Int128.MinValue - 1);

            [Fact] void MaxInt128() => Json5.Parse("0b1111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111").Should().BeValue(Int128.MaxValue);
            [Fact] void MaxInt128Plus1() => Json5.Parse("0b10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000").Should().BeValue((UInt128)Int128.MaxValue + 1);

            [Fact] void MaxUInt128() => Json5.Parse("0b11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111").Should().BeValue(UInt128.MaxValue);
            [Fact] void MaxUInt128Plus1() => Json5.Parse("0b100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000").Should().BeValue((BigInteger)UInt128.MaxValue + 1);
        }

        public partial class Dec {
            [Fact] void Positive() => Json5.Parse("+546").Should().BeValue(546);
            [Fact] void Negative() => Json5.Parse("-1656").Should().BeValue(-1656);
            [Fact] void LeadingZeros() => Json5.Parse("00123").Should().BeValue(123);

            [Fact] void MinInt() => Json5.Parse("-2147483648").Should().BeValue(int.MinValue);
            [Fact] void MinIntMinus1() => Json5.Parse("-2147483649").Should().BeValue(int.MinValue - 1L);

            [Fact] void MaxInt() => Json5.Parse("2147483647").Should().BeValue(int.MaxValue);
            [Fact] void MaxIntPlus1() => Json5.Parse("2147483648").Should().BeValue(int.MaxValue + 1U);

            [Fact] void MaxUInt() => Json5.Parse("4294967295").Should().BeValue(uint.MaxValue);
            [Fact] void MaxUIntPlus1() => Json5.Parse("4294967296").Should().BeValue(uint.MaxValue + 1L);

            [Fact] void MinLong() => Json5.Parse("-9223372036854775808").Should().BeValue(long.MinValue);
            [Fact] void MinLongMinus1() => Json5.Parse("-9223372036854775809").Should().BeValue((Int128)long.MinValue - 1);

            [Fact] void MaxLong() => Json5.Parse("9223372036854775807").Should().BeValue(long.MaxValue);
            [Fact] void MaxLongPlus1() => Json5.Parse("9223372036854775808").Should().BeValue(long.MaxValue + 1UL);

            [Fact] void MaxULong() => Json5.Parse("18446744073709551615").Should().BeValue(ulong.MaxValue);
            [Fact] void MaxULongPlus1() => Json5.Parse("18446744073709551616").Should().BeValue((Int128)ulong.MaxValue + 1);

            [Fact] void MinInt128() => Json5.Parse("-170141183460469231731687303715884105728").Should().BeValue(Int128.MinValue);
            [Fact] void MinInt128Minus1() => Json5.Parse("-170141183460469231731687303715884105729").Should().BeValue((BigInteger)Int128.MinValue - 1);

            [Fact] void MaxInt128() => Json5.Parse("170141183460469231731687303715884105727").Should().BeValue(Int128.MaxValue);
            [Fact] void MaxInt128Plus1() => Json5.Parse("170141183460469231731687303715884105728").Should().BeValue((UInt128)Int128.MaxValue + 1);

            [Fact] void MaxUInt128() => Json5.Parse("340282366920938463463374607431768211455").Should().BeValue(UInt128.MaxValue);
            [Fact] void MaxUInt128Plus1() => Json5.Parse("340282366920938463463374607431768211456").Should().BeValue((BigInteger)UInt128.MaxValue + 1);
        }

        public partial class Hex {
            [Fact] void IgnoreCase() => Json5.Parse("0xABCDEF").Should().Be(Json5.Parse("0Xabcdef"));
            [Fact] void Positive() => Json5.Parse("+0x0023FF").Should().BeValue(9215);
            [Fact] void Negative() => Json5.Parse("-0x12A").Should().BeValue(-298);
            [Fact] void LeadingZeros() => Json5.Parse("0x000B").Should().BeValue(11);

            [Fact] void MinInt() => Json5.Parse("-0x80000000").Should().BeValue(int.MinValue);
            [Fact] void MinIntMinus1() => Json5.Parse("-0x80000001").Should().BeValue(int.MinValue - 1L);

            [Fact] void MaxInt() => Json5.Parse("0x7fffffff").Should().BeValue(int.MaxValue);
            [Fact] void MaxIntPlus1() => Json5.Parse("0x80000000").Should().BeValue(int.MaxValue + 1U);

            [Fact] void MaxUInt() => Json5.Parse("0xffffffff").Should().BeValue(uint.MaxValue);
            [Fact] void MaxUIntPlus1() => Json5.Parse("0x100000000").Should().BeValue(uint.MaxValue + 1L);

            [Fact] void MinLong() => Json5.Parse("-0x8000000000000000").Should().BeValue(long.MinValue);
            [Fact] void MinLongMinus1() => Json5.Parse("-0x8000000000000001").Should().BeValue((Int128)long.MinValue - 1);

            [Fact] void MaxLong() => Json5.Parse("0x7fffffffffffffff").Should().BeValue(long.MaxValue);
            [Fact] void MaxLongPlus1() => Json5.Parse("0x8000000000000000").Should().BeValue(long.MaxValue + 1UL);

            [Fact] void MaxULong() => Json5.Parse("0xffffffffffffffff").Should().BeValue(ulong.MaxValue);
            [Fact] void MaxULongPlus1() => Json5.Parse("0x10000000000000000").Should().BeValue((Int128)ulong.MaxValue + 1);

            [Fact] void MinInt128() => Json5.Parse("-0x80000000000000000000000000000000").Should().BeValue(Int128.MinValue);
            [Fact] void MinInt128Minus1() => Json5.Parse("-0x80000000000000000000000000000001").Should().BeValue((BigInteger)Int128.MinValue - 1);

            [Fact] void MaxInt128() => Json5.Parse("0x7fffffffffffffffffffffffffffffff").Should().BeValue(Int128.MaxValue);
            [Fact] void MaxInt128Plus1() => Json5.Parse("0x80000000000000000000000000000000").Should().BeValue((UInt128)Int128.MaxValue + 1);

            [Fact] void MaxUInt128() => Json5.Parse("0xffffffffffffffffffffffffffffffff").Should().BeValue(UInt128.MaxValue);
            [Fact] void MaxUInt128Plus1() => Json5.Parse("0x100000000000000000000000000000000").Should().BeValue((BigInteger)UInt128.MaxValue + 1);
        }
    }

    public partial class Floats {
        public class Infinity {
            [Fact] void Positive() => Json5.Parse("Infinity").Should().BeValue(double.PositiveInfinity);
            [Fact] void Negative() => Json5.Parse("-Infinity").Should().BeValue(double.NegativeInfinity);
            [Fact] void ExplicitPositive() => Json5.Parse("+Infinity").Should().BeValue(double.PositiveInfinity);
            [Fact] void Short() => Json5.Parse("+Inf").Should().BeValue(double.PositiveInfinity);
            [Fact] void Symbol() => Json5.Parse("-∞").Should().BeValue(double.NegativeInfinity);
            [Fact] void UpperCase() => Json5.Parse("INFINITY").Should().BeValue(double.PositiveInfinity);
            [Fact] void LowerCase() => Json5.Parse("-infinity").Should().BeValue(double.NegativeInfinity);
        }

        public class NaN {
            [Fact] void MixedCase() => Json5.Parse("NaN").Should().BeNaN();
            [Fact] void UpperCase() => Json5.Parse("NAN").Should().BeNaN();
            [Fact] void LowerCase() => Json5.Parse("nan").Should().BeNaN();
        }

        [Fact] void WithoutIntegerPart() => Json5.Parse(".0123").Should().BeValue(.0123);
        [Fact] void WithoutFractionDigits() => Json5.Parse("1.").Should().BeValue(1d);
        [Fact] void WithExponent() => Json5.Parse("1.2e3").Should().BeValue(1.2e3);
        [Fact] void IgnoresExponentIndicatorCase() => Json5.Parse("1e-3").Should().Be(Json5.Parse("1E-3"));

        [Fact] void MinDouble() => Json5.Parse("-1.7976931348623157E+308").Should().BeValue(double.MinValue);
        [Fact] void MaxDouble() => Json5.Parse("1.7976931348623157E+308").Should().BeValue(double.MaxValue);
        [Fact] void Epsilon() => Json5.Parse("5E-324").Should().BeValue(double.Epsilon);
    }

    public partial class Money {
        [Fact] void RequiresSuffix() => Json5.Parse("0.123456789m").Should().BeValue(0.123456789m);
        [Fact] void ParsesDoubleWithoutSuffix() => Json5.Parse("0.3333333333333333333333333333").Should().BeValue(0.3333333333333333d);
        [Fact] void IgnoresSuffixCase() => Json5.Parse("1.23m").Should().Be(Json5.Parse("1.23M"));
        [Fact] void OnlyMSuffixIsAllowed() => Invoking(() => Json5.Parse("123L")).Should().Throw<NotSupportedException>().WithMessage("Format of the number literal 123L is not supported.");

        [Fact] void MinDecimal() => Json5.Parse("-79228162514264337593543950335m").Should().BeValue(decimal.MinValue);
        [Fact] void MaxDecimal() => Json5.Parse("79228162514264337593543950335m").Should().BeValue(decimal.MaxValue);
        [Fact] void Epsilon() => Json5.Parse("0.0000000000000000000000000001m").Should().BeValue(0.0000000000000000000000000001m);
        [Fact] void SmallerThanEpsilon() => Json5.Parse("0.00000000000000000000000000001m").Should().BeValue(0m);
    }

    public class Strings {
        [Fact] void DoubleQuoted() => Json5.Parse("\"strings are 'fun'\"").Should().BeValue("strings are 'fun'");
        [Fact] void SingleQuoted() => Json5.Parse("'strings are \"fun\"'").Should().BeValue("strings are \"fun\"");
        [Fact] void NoUnescapedLineTerminator() => Invoking(() => Json5.Parse("\"let's have a break\nshall we\"")).Should().Throw<Exception>().WithMessage(
            """
            Error in Ln: 1 Col: 1
            "let's have a break
            ^
            Expecting: bool, null, number or string

            string could not be parsed because:
              Error in Ln: 1 Col: 20
              "let's have a break
                                 ^
              Note: The error occurred at the end of the line.
              Expecting: any char (except ", \, or newline), escape sequence or '"'
            
            """);

        public class EscapeSequences {
            [Fact] void SingleQuote() => Json5.Parse(@"'let\'s go'").Should().BeValue("let's go");
            [Fact] void DoubleQuote() => Json5.Parse("\"...\\\"go\\\" where?\"").Should().BeValue("...\"go\" where?");
            [Fact] void Backslash() => Json5.Parse(@"'escape with \\'").Should().BeValue(@"escape with \");
            [Fact] void Backspace() => Json5.Parse(@"'go back with \b'").Should().BeValue("go back with \b");
            [Fact] void FormFeed() => Json5.Parse(@"'next page with \f'").Should().BeValue("next page with \f");
            [Fact] void Newline() => Json5.Parse(@"'break\nme'").Should().BeValue("break\nme");
            [Fact] void CarriageReturn() => Json5.Parse(@"'yea \r whatever'").Should().BeValue("yea \r whatever");
            [Fact] void Tab() => Json5.Parse(@"'space\tcreated'").Should().BeValue("space\tcreated");
            [Fact] void VerticalTab() => Json5.Parse(@"'space\vcreated'").Should().BeValue("space\vcreated");
            [Fact] void NullChar() => Json5.Parse(@"'terminate me\0'").Should().BeValue("terminate me\0");
            [Fact] void Unknown() => Invoking(() => Json5.Parse(@"'\?'")).Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 1
                '\?'
                ^
                Expecting: bool, null, number or string

                string could not be parsed because:
                  Error in Ln: 1 Col: 2
                  '\?'
                   ^
                  Expecting: any char (except ', \, or newline) or escape sequence

                  escape sequence could not be parsed because:
                    Error in Ln: 1 Col: 3
                    '\?'
                      ^
                    Expecting: any char in ‘bfnrtv0'"\’
                
                """);
        }
    }
}
