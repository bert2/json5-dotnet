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
        [Fact] void Positive() => Parser.Parse("Infinity").Should().BeValue(double.PositiveInfinity);
        [Fact] void Negative() => Parser.Parse("-Infinity").Should().BeValue(double.NegativeInfinity);
        [Fact] void ExplicitPositive() => Parser.Parse("+Infinity").Should().BeValue(double.PositiveInfinity);
        [Fact] void Short() => Parser.Parse("+Inf").Should().BeValue(double.PositiveInfinity);
        [Fact] void Symbol() => Parser.Parse("-∞").Should().BeValue(double.NegativeInfinity);
        [Fact] void UpperCase() => Parser.Parse("INFINITY").Should().BeValue(double.PositiveInfinity);
        [Fact] void LowerCase() => Parser.Parse("-infinity").Should().BeValue(double.NegativeInfinity);
    }

    public class NaN {
        [Fact] void MixedCase() => Parser.Parse("NaN").Should().BeNaN();
        [Fact] void UpperCase() => Parser.Parse("NAN").Should().BeNaN();
        [Fact] void LowerCase() => Parser.Parse("nan").Should().BeNaN();
    }

    [Fact] void WithoutIntegerPart() => Parser.Parse(".0123").Should().BeValue(.0123);
    [Fact] void WithoutFractionDigits() => Parser.Parse("1.").Should().BeValue(1d);
    [Fact] void WithExponent() => Parser.Parse("1.2e3").Should().BeValue(1.2e3);
    [Fact] void IgnoresExponentIndicatorCase() => Parser.Parse("1e-3").Should().Be(Parser.Parse("1E-3"));

    [Fact] void MinDouble() => Parser.Parse("-1.7976931348623157E+308").Should().BeValue(double.MinValue);
    [Fact] void MaxDouble() => Parser.Parse("1.7976931348623157E+308").Should().BeValue(double.MaxValue);
    [Fact] void Epsilon() => Parser.Parse("5E-324").Should().BeValue(double.Epsilon);

    [Property(MaxTest = N)] Property Generated() => Prop.ForAll(@double, x => RoundTrip(x) == x || double.IsNaN(x));

    private static double RoundTrip(double x) => Parser.Parse(x.ToString())!.GetValue<double>();

    // generates double values that are never integers (except +/- infinity)
    private static readonly Arbitrary<double> @double = ArbMap.Default.ArbFor<double>()
        .Generator
        .Select(x => double.IsInteger(x) && !double.IsInfinity(x) ? 1 / x : x)
        .ToArbitrary();
}
