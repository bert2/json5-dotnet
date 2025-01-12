#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests.ParserTests;

using FluentAssertions;

using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

using Helpers;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

using static FluentAssertions.FluentActions;
using static Helpers.Constants;

public class Floats {
    public class Infinity {
        [Fact] void Positive() => Parser.Parse2("Infinity").Should().Be(double.PositiveInfinity);
        [Fact] void Negative() => Parser.Parse2("-Infinity").Should().Be(double.NegativeInfinity);
        [Fact] void ExplicitPositive() => Parser.Parse2("+Infinity").Should().Be(double.PositiveInfinity);
        [Fact] void Short() => Parser.Parse2("+Inf").Should().Be(double.PositiveInfinity);
        [Fact(Skip = "no ∞")] void Symbol() => Parser.Parse2("-∞").Should().Be(double.NegativeInfinity);
        [Fact] void UpperCase() => Parser.Parse2("INFINITY").Should().Be(double.PositiveInfinity);
        [Fact] void LowerCase() => Parser.Parse2("-infinity").Should().Be(double.NegativeInfinity);
    }

    public class NaN {
        [Fact] void MixedCase() => Parser.Parse2("NaN").Should().Be(double.NaN);
        [Fact] void UpperCase() => Parser.Parse2("NAN").Should().Be(double.NaN);
        [Fact] void LowerCase() => Parser.Parse2("nan").Should().Be(double.NaN);
        [Fact] void Positive() => Parser.Parse2("+NaN").Should().Be(double.NaN);
        [Fact] void Negative() => Parser.Parse2("-NaN").Should().Be(double.NaN);
    }

    public class Exponent {
        [Fact] void Example() => Parser.Parse2("1.2e3").Should().Be(1.2e3);
        [Fact] void LowerCaseExponentIndicator() => Parser.Parse2("1e3").Should().Be(1e3);
        [Fact] void UpperCaseExponentIndicator() => Parser.Parse2("1E3").Should().Be(1e3);
        [Fact] void WithoutFractionDigits() => Parser.Parse2("5.e4").Should().Be(5.0e4);
        [Fact] void NegativeInteger() => Parser.Parse2("2e-23").Should().Be(2e-23);
        [Fact] void PositiveInteger() => Parser.Parse2("1e+2").Should().Be(1e+2);
        [Fact] void Zero() => Parser.Parse2("5e0").Should().Be(5e0);
        [Fact] void NegativeZero() => Parser.Parse2("5e-0").Should().Be(5e-0);
        [Fact] void PositiveZero() => Parser.Parse2("5e+0").Should().Be(5e+0);

        [Fact]
        void LoneIndicator() =>
            Invoking(() => Parser.Parse2("e"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 1
                e
                ^
                Expecting: array, bool, null, number, object or string
                """);

        [Fact]
        void Float() =>
            Invoking(() => Parser.Parse2("1e2.3"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 4
                1e2.3
                   ^
                Expecting: end of input
                """);

        [Fact]
        void NegativeFloat() =>
            Invoking(() => Parser.Parse2("1e-2.3"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 5
                1e-2.3
                    ^
                Expecting: end of input
                """);

        [Fact]
        void PositiveFloat() =>
            Invoking(() => Parser.Parse2("1e+2.3"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 5
                1e+2.3
                    ^
                Expecting: end of input
                """);

        [Fact]
        void Hex() =>
            Invoking(() => Parser.Parse2("1e0x4"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 4
                1e0x4
                   ^
                Expecting: end of input
                """);

        [Fact]
        void NegativeHex() =>
            Invoking(() => Parser.Parse2("1e-0x4"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 5
                1e-0x4
                    ^
                Expecting: end of input
                """);

        [Fact]
        void PositiveHex() =>
            Invoking(() => Parser.Parse2("1e+0x4"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 5
                1e+0x4
                    ^
                Expecting: end of input
                """);
    }

    [Fact] void LeadingZero() => Parser.Parse2("0.123").Should().Be(0.123);
    [Fact] void NegativeLeadingZero() => Parser.Parse2("-0.123").Should().Be(-0.123);
    [Fact] void PositiveLeadingZero() => Parser.Parse2("+0.456").Should().Be(0.456);
    [Fact] void LeadingZeros() => Parser.Parse2("000.123").Should().Be(0.123);
    [Fact] void WithoutIntegerPart() => Parser.Parse2(".0123").Should().Be(.0123);
    [Fact] void NegativeWithoutIntegerPart() => Parser.Parse2("-.5").Should().Be(-.5);
    [Fact] void PositiveWithoutIntegerPart() => Parser.Parse2("+.5").Should().Be(+.5);
    [Fact] void WithoutFractionDigits() => Parser.Parse2("1.").Should().Be(1d);
    [Fact] void NegativeWithoutFractionDigits() => Parser.Parse2("-1.").Should().Be(-1d);
    [Fact] void PositiveWithoutFractionDigits() => Parser.Parse2("+1.").Should().Be(+1d);

    [Fact] void MinDouble() => Parser.Parse2("-1.7976931348623157E+308").Should().Be(double.MinValue);
    [Fact] void MaxDouble() => Parser.Parse2("1.7976931348623157E+308").Should().Be(double.MaxValue);
    [Fact] void Epsilon() => Parser.Parse2("5E-324").Should().Be(double.Epsilon);

    [Fact]
    void LoneDecimalPoint() =>
        Invoking(() => Parser.Parse2("."))
        .Should().Throw<Exception>().WithMessage(
            """
            Error in Ln: 1 Col: 2
            .
             ^
            Note: The error occurred at the end of the input stream.
            Expecting: decimal digit
            """);

    [Property(MaxTest = N)] Property Generated() => Prop.ForAll(@double, x => RoundTrip(x) == x || double.IsNaN(x));

    private static readonly JsonSerializerOptions jsonOpts = new() { NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals };

    private static double RoundTrip(double x)
        => Parser.Parse2(x.ToString(CultureInfo.InvariantCulture))!.Deserialize<double>(jsonOpts);

    // generates double values that are never integers (except +/- infinity)
    private static readonly Arbitrary<double> @double = ArbMap.Default.ArbFor<double>()
        .Generator
        .Select(x => double.IsInteger(x) && !double.IsInfinity(x) ? 1 / x : x)
        .ToArbitrary();
}
