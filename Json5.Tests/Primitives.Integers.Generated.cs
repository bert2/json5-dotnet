namespace Json5.Tests;

using FsCheck;
using FsCheck.Xunit;

using System.Numerics;

public partial class Primitives {
    public static partial class Integers {
        private const int N = 100_000;
        
        public partial class Bin {
            public class Generated {
                [Property(MaxTest = N)]
                public bool Int(int x) => RoundTripBin(x) == x;

                [Property(MaxTest = N)]
                public Property Long() => Prop.ForAll(@long, x => RoundTripBin(x) == x);

                [Property(MaxTest = N)]
                public Property ULong() => Prop.ForAll(@ulong, x => RoundTripBin(x) == x);

                [Property(MaxTest = N)]
                public Property BigInt() => Prop.ForAll(bigint, x => RoundTripBin(x) == x);

                private static T RoundTripBin<T>(T x)
                    where T : IComparisonOperators<T, T, bool>, IAdditiveIdentity<T, T>, IUnaryNegationOperators<T, T>
                    => Deserialize<T>(x < T.AdditiveIdentity ? $"-0b{-x:b}" : $"0b{x:b}");
            }
        }

        public partial class Dec {
            public class Generated {
                [Property(MaxTest = N)]
                public bool Int(int x) => RoundTripDec(x) == x;

                [Property(MaxTest = N)]
                public Property Long() => Prop.ForAll(@long, x => RoundTripDec(x) == x);

                [Property(MaxTest = N)]
                public Property ULong() => Prop.ForAll(@ulong, x => RoundTripDec(x) == x);

                [Property(MaxTest = N)]
                public Property BigInt() => Prop.ForAll(bigint, x => RoundTripDec(x) == x);

                private static T RoundTripDec<T>(T x) => Deserialize<T>($"{x:d}");
            }
        }

        public partial class Hex {
            public class Generated {
                [Property(MaxTest = N)]
                public bool Int(int x) => RoundTripHex(x) == x;
                
                [Property(MaxTest = N)]
                public Property Long() => Prop.ForAll(@long, x => RoundTripHex(x) == x);

                [Property(MaxTest = N)]
                public Property ULong() => Prop.ForAll(@ulong, x => RoundTripHex(x) == x);

                [Property(MaxTest = N)]
                public Property BigInt() => Prop.ForAll(bigint, x => RoundTripHex(x) == x);

                private static T RoundTripHex<T>(T x)
                    where T : IComparisonOperators<T, T, bool>, IAdditiveIdentity<T, T>, IUnaryNegationOperators<T, T>
                    => Deserialize<T>(x < T.AdditiveIdentity ? $"-0x{-x:x}" : $"0x{x:x}");
            }
        }

        private static T Deserialize<T>(string s) => Json5.Parse(s)!.GetValue<T>();

        // generates long values beyond the range of int
        private static readonly Arbitrary<long> @long = Arb.Default.DoNotSizeInt64()
            .Generator
            .Select(x => x.Item switch {
                > int.MaxValue => x.Item,
                < int.MinValue => x.Item,
                0 => int.MaxValue + 1L,
                > 0 => int.MaxValue + x.Item,
                < 0 => int.MinValue + x.Item,
            })
            .ToArbitrary();

        // generates ulong values greater than long.MaxValue
        private static readonly Arbitrary<ulong> @ulong = Arb.Default.DoNotSizeUInt64()
            .Generator
            .Select(x => x.Item switch {
                > long.MaxValue => x.Item,
                > 0 => long.MaxValue + x.Item,
                0 => long.MaxValue + 1UL,
            })
            .ToArbitrary();

        // generates BigInteger values beyond the range [long.MinValue .. ulong.MaxValue]
        private static readonly Arbitrary<BigInteger> bigint = @long
            .Generator
            .Select(x => BigInteger.Multiply(x, ulong.MaxValue))
            .ToArbitrary();
    }
}