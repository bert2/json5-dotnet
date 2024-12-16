#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests;

using FluentAssertions;

using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

using Helpers;

using static Helpers.Constants;

public class Floats {
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

    [Property(MaxTest = N)] Property Generated() => Prop.ForAll(@double, x => RoundTrip(x) == x || double.IsNaN(x));

    private static double RoundTrip(double x) => Json5.Parse(x.ToString())!.GetValue<double>();

    // generates double values that are never integers (except +/- infinity)
    private static readonly Arbitrary<double> @double = ArbMap.Default.ArbFor<double>()
        .Generator
        .Select(x => double.IsInteger(x) && !double.IsInfinity(x) ? 1 / x : x)
        .ToArbitrary();
}
