namespace Json5.Tests;

using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

public partial class Primitives {
    private const int N = 1_000; // 100_000

    public partial class Floats {
        [Property(MaxTest = N)] Property Generated() => Prop.ForAll(@double, x => RoundTrip(x) == x || double.IsNaN(x));

        private static double RoundTrip(double x) => Json5.Parse(x.ToString())!.GetValue<double>();

        // generates double values that are never integers (except +/- infinity)
        private static readonly Arbitrary<double> @double = ArbMap.Default.ArbFor<double>()
            .Generator
            .Select(x => double.IsInteger(x) && !double.IsInfinity(x) ? 1 / x : x)
            .ToArbitrary();
    }

    public partial class Money {
        [Property(MaxTest = N)] bool Generated(decimal x) => RoundTrip(x) == x;

        private static decimal RoundTrip(decimal x) => Json5.Parse($"{x}m")!.GetValue<decimal>();
    }
}
