#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests;

using System.Numerics;

public partial class Primitives {
    [Fact] public void Null() => Json5.Parse("null").Should().BeNull();

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

            [Fact] void MinInt() => Json5.Parse("-0b10000000000000000000000000000000").Should().BeValue((long)int.MinValue);
            [Fact] void MinIntMinus1() => Json5.Parse("-0b10000000000000000000000000000001").Should().BeValue(int.MinValue - 1L);

            [Fact] void MaxInt() => Json5.Parse("0b1111111111111111111111111111111").Should().BeValue(int.MaxValue);
            [Fact] void MaxIntPlus1() => Json5.Parse("0b10000000000000000000000000000000").Should().BeValue(int.MaxValue + 1L);

            [Fact] void MinLong() => Json5.Parse("-0b1000000000000000000000000000000000000000000000000000000000000000").Should().BeValue((BigInteger)long.MinValue);
            [Fact] void MinLongMinus1() => Json5.Parse("-0b1000000000000000000000000000000000000000000000000000000000000001").Should().BeValue((BigInteger)long.MinValue - 1);

            [Fact] void MaxLong() => Json5.Parse("0b111111111111111111111111111111111111111111111111111111111111111").Should().BeValue(long.MaxValue);
            [Fact] void MaxLongPlus1() => Json5.Parse("0b1000000000000000000000000000000000000000000000000000000000000000").Should().BeValue(long.MaxValue + 1UL);

            [Fact] void MaxULong() => Json5.Parse("0b1111111111111111111111111111111111111111111111111111111111111111").Should().BeValue(ulong.MaxValue);
            [Fact] void MaxULongPlus1() => Json5.Parse("0b10000000000000000000000000000000000000000000000000000000000000000").Should().BeValue((BigInteger)ulong.MaxValue + 1);
        }

        public partial class Dec {
            [Fact] void Positive() => Json5.Parse("+546").Should().BeValue(546);
            [Fact] void Negative() => Json5.Parse("-1656").Should().BeValue(-1656);
            [Fact] void LeadingZeros() => Json5.Parse("00123").Should().BeValue(123);

            [Fact] void MinInt() => Json5.Parse("-2147483648").Should().BeValue((long)int.MinValue);
            [Fact] void MinIntMinus1() => Json5.Parse("-2147483649").Should().BeValue(int.MinValue - 1L);

            [Fact] void MaxInt() => Json5.Parse("2147483647").Should().BeValue(int.MaxValue);
            [Fact] void MaxIntPlus1() => Json5.Parse("2147483648").Should().BeValue(int.MaxValue + 1L);

            [Fact] void MinLong() => Json5.Parse("-9223372036854775808").Should().BeValue((BigInteger)long.MinValue);
            [Fact] void MinLongMinus1() => Json5.Parse("-9223372036854775809").Should().BeValue((BigInteger)long.MinValue - 1);

            [Fact] void MaxLong() => Json5.Parse("9223372036854775807").Should().BeValue(long.MaxValue);
            [Fact] void MaxLongPlus1() => Json5.Parse("9223372036854775808").Should().BeValue(long.MaxValue + 1UL);

            [Fact] void MaxULong() => Json5.Parse("18446744073709551615").Should().BeValue(ulong.MaxValue);
            [Fact] void MaxULongPlus1() => Json5.Parse("18446744073709551616").Should().BeValue((BigInteger)ulong.MaxValue + 1);
        }

        public partial class Hex {
            [Fact] void IgnoreCase() => Json5.Parse("0xABCDEF").Should().Be(Json5.Parse("0Xabcdef"));
            [Fact] void Positive() => Json5.Parse("+0x0023FF").Should().BeValue(9215);
            [Fact] void Negative() => Json5.Parse("-0x12A").Should().BeValue(-298);
            [Fact] void LeadingZeros() => Json5.Parse("0x000B").Should().BeValue(11);

            [Fact] void MinInt() => Json5.Parse("-0x80000000").Should().BeValue((long)int.MinValue);
            [Fact] void MinIntMinus1() => Json5.Parse("-0x80000001").Should().BeValue(int.MinValue - 1L);

            [Fact] void MaxInt() => Json5.Parse("0x7fffffff").Should().BeValue(int.MaxValue);
            [Fact] void MaxIntPlus1() => Json5.Parse("0x80000000").Should().BeValue(int.MaxValue + 1L);

            [Fact] void MinLong() => Json5.Parse("-0x8000000000000000").Should().BeValue((BigInteger)long.MinValue);
            [Fact] void MinLongMinus1() => Json5.Parse("-0x8000000000000001").Should().BeValue((BigInteger)long.MinValue - 1);

            [Fact] void MaxLong() => Json5.Parse("0x7fffffffffffffff").Should().BeValue(long.MaxValue);
            [Fact] void MaxLongPlus1() => Json5.Parse("0x8000000000000000").Should().BeValue(long.MaxValue + 1UL);

            [Fact] void MaxULong() => Json5.Parse("0xffffffffffffffff").Should().BeValue(ulong.MaxValue);
            [Fact] void MaxULongPlus1() => Json5.Parse("0x10000000000000000").Should().BeValue((BigInteger)ulong.MaxValue + 1);
        }
    }
}
