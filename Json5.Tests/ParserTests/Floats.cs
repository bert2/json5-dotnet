#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests.ParserTests;

using FluentAssertions;

using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

using Helpers;

using static FluentAssertions.FluentActions;
using static Helpers.Constants;

public class Floats {
    public class Infinity {
        [Fact] void Positive() => Parser.Parse("Infinity").Should().Be(double.PositiveInfinity);
        [Fact] void Negative() => Parser.Parse("-Infinity").Should().Be(double.NegativeInfinity);
        [Fact] void ExplicitPositive() => Parser.Parse("+Infinity").Should().Be(double.PositiveInfinity);
        [Fact] void Short() => Parser.Parse("+Inf").Should().Be(double.PositiveInfinity);
        [Fact] void Symbol() => Parser.Parse("-∞").Should().Be(double.NegativeInfinity);
        [Fact] void UpperCase() => Parser.Parse("INFINITY").Should().Be(double.PositiveInfinity);
        [Fact] void LowerCase() => Parser.Parse("-infinity").Should().Be(double.NegativeInfinity);
    }

    public class NaN {
        [Fact] void MixedCase() => Parser.Parse("NaN").Should().BeNaN();
        [Fact] void UpperCase() => Parser.Parse("NAN").Should().BeNaN();
        [Fact] void LowerCase() => Parser.Parse("nan").Should().BeNaN();
    }

    public class Exponent {
        [Fact] void Example() => Parser.Parse("1.2e3").Should().Be(1.2e3);
        [Fact] void LowerCaseExponentIndicator() => Parser.Parse("1e3").Should().Be(1e3);
        [Fact] void UpperCaseExponentIndicator() => Parser.Parse("1E3").Should().Be(1e3);
        [Fact] void WithoutFractionDigits() => Parser.Parse("5.e4").Should().Be(5.0e4);
        [Fact] void NegativeInteger() => Parser.Parse("2e-23").Should().Be(2e-23);
        [Fact] void PositiveInteger() => Parser.Parse("1e+2").Should().Be(1e+2);
        [Fact] void Zero() => Parser.Parse("5e0").Should().Be(5e0);
        [Fact] void NegativeZero() => Parser.Parse("5e-0").Should().Be(5e-0);
        [Fact] void PositiveZero() => Parser.Parse("5e+0").Should().Be(5e+0);

        [Fact]
        void Float() =>
            Invoking(() => Parser.Parse("1e2.3"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 4
                1e2.3
                   ^
                Expecting: end of input
                """);

        [Fact]
        void NegativeFloat() =>
            Invoking(() => Parser.Parse("1e-2.3"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 5
                1e-2.3
                    ^
                Expecting: end of input
                """);

        [Fact]
        void PositiveFloat() =>
            Invoking(() => Parser.Parse("1e+2.3"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 5
                1e+2.3
                    ^
                Expecting: end of input
                """);

        [Fact]
        void Hex() =>
            Invoking(() => Parser.Parse("1e0x4"))
            .Should().Throw<Exception>().WithMessage("Format of the number literal 1e0x is not supported.");

        [Fact]
        void NegativeHex() =>
            Invoking(() => Parser.Parse("1e-0x4"))
            .Should().Throw<Exception>().WithMessage("Format of the number literal 1e-0x is not supported.");

        [Fact]
        void PositiveHex() =>
            Invoking(() => Parser.Parse("1e+0x4"))
            .Should().Throw<Exception>().WithMessage("Format of the number literal 1e+0x is not supported.");

    }

    [Fact] void LeadingZero() => Parser.Parse("0.123").Should().Be(0.123);
    [Fact] void NegativeLeadingZero() => Parser.Parse("-0.123").Should().Be(-0.123);
    [Fact] void LeadingZeros() => Parser.Parse("000.123").Should().Be(0.123);
    [Fact] void WithoutIntegerPart() => Parser.Parse(".0123").Should().Be(.0123);
    [Fact] void NegativeWithoutIntegerPart() => Parser.Parse("-.5").Should().Be(-.5);
    [Fact] void PositiveWithoutIntegerPart() => Parser.Parse("+.5").Should().Be(+.5);
    [Fact] void WithoutFractionDigits() => Parser.Parse("1.").Should().Be(1d);
    [Fact] void NegativeWithoutFractionDigits() => Parser.Parse("-1.").Should().Be(-1d);
    [Fact] void PositiveWithoutFractionDigits() => Parser.Parse("+1.").Should().Be(+1d);

    [Fact] void MinDouble() => Parser.Parse("-1.7976931348623157E+308").Should().Be(double.MinValue);
    [Fact] void MaxDouble() => Parser.Parse("1.7976931348623157E+308").Should().Be(double.MaxValue);
    [Fact] void Epsilon() => Parser.Parse("5E-324").Should().Be(double.Epsilon);

    [Fact]
    void LoneDecimalPoint() =>
        Invoking(() => Parser.Parse("."))
        .Should().Throw<Exception>().WithMessage(
            """
            Error in Ln: 1 Col: 2
            .
             ^
            Note: The error occurred at the end of the input stream.
            Expecting: decimal digit
            """);

    [Property(MaxTest = N)] Property Generated() => Prop.ForAll(@double, x => RoundTrip(x) == x || double.IsNaN(x));

    private static double RoundTrip(double x) => Parser.Parse(x.ToString())!.GetValue<double>();

    // generates double values that are never integers (except +/- infinity)
    private static readonly Arbitrary<double> @double = ArbMap.Default.ArbFor<double>()
        .Generator
        .Select(x => double.IsInteger(x) && !double.IsInfinity(x) ? 1 / x : x)
        .ToArbitrary();
}
