namespace Json5.Tests.ParserTests;

using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

using System.Numerics;
using System.Text.Json;

using static Helpers.Constants;

public static partial class Integers {
    public partial class Bin {
        public class Generated {
            [Property(MaxTest = N)] bool Int(int x) => RoundTrip(x) == x;
            [Property(MaxTest = N)] Property UInt() => Prop.ForAll(@uint, x => RoundTrip(x) == x);
            [Property(MaxTest = N)] Property Long() => Prop.ForAll(@long, x => RoundTrip(x) == x);
            [Property(MaxTest = N)] Property ULong() => Prop.ForAll(@ulong, x => RoundTrip(x) == x);
            [Property(MaxTest = N)] Property Int128() => Prop.ForAll(int128, x => RoundTrip(x) == x);
            [Property(MaxTest = N)] Property UInt128() => Prop.ForAll(uint128, x => RoundTrip(x) == x);
            [Property(MaxTest = N)] Property BigInt() => Prop.ForAll(bigint, x => RoundTrip(x) == x);

            private static T RoundTrip<T>(T x)
                where T : IComparisonOperators<T, T, bool>, IAdditiveIdentity<T, T>, IUnaryNegationOperators<T, T>
                => Deserialize<T>(x < T.AdditiveIdentity ? $"-0b{-x:b}" : $"0b{x:b}");
        }
    }

    public partial class Dec {
        public class Generated {
            [Property(MaxTest = N)] bool Int(int x) => RoundTrip(x) == x;
            [Property(MaxTest = N)] Property UInt() => Prop.ForAll(@uint, x => RoundTrip(x) == x);
            [Property(MaxTest = N)] Property Long() => Prop.ForAll(@long, x => RoundTrip(x) == x);
            [Property(MaxTest = N)] Property ULong() => Prop.ForAll(@ulong, x => RoundTrip(x) == x);
            [Property(MaxTest = N)] Property Int128() => Prop.ForAll(int128, x => RoundTrip(x) == x);
            [Property(MaxTest = N)] Property UInt128() => Prop.ForAll(uint128, x => RoundTrip(x) == x);
            [Property(MaxTest = N)] Property BigInt() => Prop.ForAll(bigint, x => RoundTrip(x) == x);

            private static T RoundTrip<T>(T x) => Deserialize<T>($"{x:d}");
        }
    }

    public partial class Hex {
        public class Generated {
            [Property(MaxTest = N)] bool Int(int x) => RoundTrip(x) == x;
            [Property(MaxTest = N)] Property UInt() => Prop.ForAll(@uint, x => RoundTrip(x) == x);
            [Property(MaxTest = N)] Property Long() => Prop.ForAll(@long, x => RoundTrip(x) == x);
            [Property(MaxTest = N)] Property ULong() => Prop.ForAll(@ulong, x => RoundTrip(x) == x);
            [Property(MaxTest = N)] Property Int128() => Prop.ForAll(int128, x => RoundTrip(x) == x);
            [Property(MaxTest = N)] Property UInt128() => Prop.ForAll(uint128, x => RoundTrip(x) == x);
            [Property(MaxTest = N)] Property BigInt() => Prop.ForAll(bigint, x => RoundTrip(x) == x);

            private static T RoundTrip<T>(T x)
                where T : IComparisonOperators<T, T, bool>, IAdditiveIdentity<T, T>, IUnaryNegationOperators<T, T>
                => Deserialize<T>(x < T.AdditiveIdentity ? $"-0x{-x:x}" : $"0x{x:x}");
        }
    }

    private static T Deserialize<T>(string s) => Parser.Parse(s)!.Deserialize<T>();

    // generates uint values greater than int.MaxValue
    private static readonly Arbitrary<uint> @uint = ArbMap.Default.ArbFor<uint>()
        .Generator
        .Select(x => x switch {
            > int.MaxValue => x,
            > 0 => int.MaxValue + x,
            0 => int.MaxValue + 1U,
        })
        .ToArbitrary();

    // generates long values beyond the range [int.MinValue .. uint.MaxValue]
    private static readonly Arbitrary<long> @long = ArbMap.Default.ArbFor<long>()
        .Generator
        .Select(x => x switch {
            > uint.MaxValue => x,
            < int.MinValue => x,
            0 => uint.MaxValue + 1L,
            > 0 => uint.MaxValue + x,
            < 0 => int.MinValue + x,
        })
        .ToArbitrary();

    // generates ulong values greater than long.MaxValue
    private static readonly Arbitrary<ulong> @ulong = ArbMap.Default.ArbFor<ulong>()
        .Generator
        .Select(x => x switch {
            > long.MaxValue => x,
            > 0 => long.MaxValue + x,
            0 => long.MaxValue + 1UL,
        })
        .ToArbitrary();

    // generates Int128 values beyond the range [long.MinValue .. ulong.MaxValue]
    private static readonly Arbitrary<Int128> int128 = ArbMap.Default.ArbFor<Int128>()
        .Generator
        .Select(x => {
            if (x > ulong.MaxValue) return x;
            else if (x < long.MinValue) return x;
            else if (x == 0) return (Int128)ulong.MaxValue + 1;
            else if (x > 0) return (Int128)ulong.MaxValue + x;
            else /* x < 0 */ return (Int128)long.MinValue + x;
        })
        .ToArbitrary();

    // generates UInt128 values greater than Int128.MaxValue
    private static readonly Arbitrary<UInt128> uint128 = ArbMap.Default.ArbFor<UInt128>()
        .Generator
        .Select(x => {
            if (x > (UInt128)Int128.MaxValue) return x;
            else if (x > 0) return (UInt128)Int128.MaxValue + x;
            else /* x == 0 */ return (UInt128)Int128.MaxValue + 1;
        })
        .ToArbitrary();

    // generates BigInteger values beyond the range [Int128.MinValue .. UInt128.MaxValue]
    private static readonly Arbitrary<BigInteger> bigint = ArbMap.Default.ArbFor<BigInteger>()
        .Generator
        .Select(x => {
            if (x > UInt128.MaxValue) return x;
            else if (x < Int128.MinValue) return x;
            else if (x == 0) return (BigInteger)UInt128.MaxValue + 1;
            else if (x > 0) return (BigInteger)UInt128.MaxValue + x;
            else /* x < 0 */ return (BigInteger)Int128.MinValue + x;
        })
        .ToArbitrary();
}
