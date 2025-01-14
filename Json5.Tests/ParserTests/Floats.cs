#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests.ParserTests;

using FluentAssertions;

using Helpers;

using System.Text.Json;
using System.Text.Json.Serialization;

using static FluentAssertions.FluentActions;

public class Floats {
    // required to deserialize "Infinity" and "NaN"
    static readonly JsonSerializerOptions opts = new() {
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    [Fact] void LeadingZero() => Parser.Parse("0.123").Should().Be(0.123);
    [Fact] void NegativeLeadingZero() => Parser.Parse("-0.123").Should().Be(-0.123);
    [Fact] void PositiveLeadingZero() => Parser.Parse("+0.456").Should().Be(0.456);
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
        void LoneIndicator() =>
            Invoking(() => Parser.Parse("e"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 1
                e
                ^
                Expecting: array, bool, null, number, object or string
                """);

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
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 4
                1e0x4
                   ^
                Expecting: end of input
                """);

        [Fact]
        void NegativeHex() =>
            Invoking(() => Parser.Parse("1e-0x4"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 5
                1e-0x4
                    ^
                Expecting: end of input
                """);

        [Fact]
        void PositiveHex() =>
            Invoking(() => Parser.Parse("1e+0x4"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 5
                1e+0x4
                    ^
                Expecting: end of input
                """);
    }

    public class Infinity {
        [Fact] void Positive() => Parser.Parse("Infinity").Should().Be(double.PositiveInfinity, opts);
        [Fact] void Negative() => Parser.Parse("-Infinity").Should().Be(double.NegativeInfinity, opts);
        [Fact] void ExplicitPositive() => Parser.Parse("+Infinity").Should().Be(double.PositiveInfinity, opts);
        [Fact] void Short() => Parser.Parse("+Inf").Should().Be(double.PositiveInfinity, opts);
        [Fact] void UpperCase() => Parser.Parse("INFINITY").Should().Be(double.PositiveInfinity, opts);
        [Fact] void LowerCase() => Parser.Parse("-infinity").Should().Be(double.NegativeInfinity, opts);
    }

    public class NaN {
        [Fact] void MixedCase() => Parser.Parse("NaN").Should().Be(double.NaN, opts);
        [Fact] void UpperCase() => Parser.Parse("NAN").Should().Be(double.NaN, opts);
        [Fact] void LowerCase() => Parser.Parse("nan").Should().Be(double.NaN, opts);
        [Fact] void Positive() => Parser.Parse("+NaN").Should().Be(double.NaN, opts);
        [Fact] void Negative() => Parser.Parse("-NaN").Should().Be(double.NaN, opts);
    }
}
