#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests.Parse;

using FluentAssertions;

using Helpers;

using System.Text.Json;
using System.Text.Json.Serialization;

using static FluentAssertions.FluentActions;
using static Json5.JSON5;

public class Floats {
    // required to deserialize "Infinity" and "NaN"
    static readonly JsonSerializerOptions opts = new() {
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    [Fact] void LeadingZero() => Parse("0.123").Should().Be(0.123);
    [Fact] void LeadingZeroAndNonZero() => Parse("01.123").Should().Be(1.123);
    [Fact] void LeadingZeros() => Parse("000.123").Should().Be(0.123);
    [Fact] void LeadingZerosAndNonZero() => Parse("0001.123").Should().Be(1.123);
    [Fact] void NegativeLeadingZero() => Parse("-0.123").Should().Be(-0.123);
    [Fact] void PositiveLeadingZero() => Parse("+0.456").Should().Be(0.456);
    [Fact] void WithoutIntegerPart() => Parse(".0123").Should().Be(.0123);
    [Fact] void NegativeWithoutIntegerPart() => Parse("-.5").Should().Be(-.5);
    [Fact] void PositiveWithoutIntegerPart() => Parse("+.5").Should().Be(+.5);
    [Fact] void WithoutFractionDigits() => Parse("1.").Should().Be(1d);
    [Fact] void NegativeWithoutFractionDigits() => Parse("-1.").Should().Be(-1d);
    [Fact] void PositiveWithoutFractionDigits() => Parse("+1.").Should().Be(+1d);
    [Fact] void MinDouble() => Parse("-1.7976931348623157E+308").Should().Be(double.MinValue);
    [Fact] void MaxDouble() => Parse("1.7976931348623157E+308").Should().Be(double.MaxValue);
    [Fact] void Epsilon() => Parse("5E-324").Should().Be(double.Epsilon);

    [Fact]
    void LoneDecimalPoint() =>
        Invoking(() => Parse("."))
        .Should().Throw<Exception>().WithMessage(
            """
            Error in Ln: 1 Col: 2
            .
             ^
            Note: The error occurred at the end of the input stream.
            Expecting: decimal digit
            """);

    public class Exponent {
        [Fact] void Example() => Parse("1.2e3").Should().Be(1.2e3);
        [Fact] void LowerCaseExponentIndicator() => Parse("1e3").Should().Be(1e3);
        [Fact] void UpperCaseExponentIndicator() => Parse("1E3").Should().Be(1e3);
        [Fact] void WithoutFractionDigits() => Parse("5.e4").Should().Be(5.0e4);
        [Fact] void NegativeInteger() => Parse("2e-23").Should().Be(2e-23);
        [Fact] void PositiveInteger() => Parse("1e+2").Should().Be(1e+2);
        [Fact] void Zero() => Parse("5e0").Should().Be(5e0);
        [Fact] void NegativeZero() => Parse("5e-0").Should().Be(5e-0);
        [Fact] void PositiveZero() => Parse("5e+0").Should().Be(5e+0);

        [Fact]
        void LoneIndicator() =>
            Invoking(() => Parse("e"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 1
                e
                ^
                Expecting: array, bool, null, number, object or string
                """);

        [Fact]
        void Float() =>
            Invoking(() => Parse("1e2.3"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 4
                1e2.3
                   ^
                Expecting: end of input
                """);

        [Fact]
        void NegativeFloat() =>
            Invoking(() => Parse("1e-2.3"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 5
                1e-2.3
                    ^
                Expecting: end of input
                """);

        [Fact]
        void PositiveFloat() =>
            Invoking(() => Parse("1e+2.3"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 5
                1e+2.3
                    ^
                Expecting: end of input
                """);

        [Fact]
        void Hex() =>
            Invoking(() => Parse("1e0x4"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 4
                1e0x4
                   ^
                Expecting: end of input
                """);

        [Fact]
        void NegativeHex() =>
            Invoking(() => Parse("1e-0x4"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 5
                1e-0x4
                    ^
                Expecting: end of input
                """);

        [Fact]
        void PositiveHex() =>
            Invoking(() => Parse("1e+0x4"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 5
                1e+0x4
                    ^
                Expecting: end of input
                """);
    }

    public class Infinity {
        [Fact] void Positive() => Parse("Infinity").Should().Be(double.PositiveInfinity, opts);
        [Fact] void Negative() => Parse("-Infinity").Should().Be(double.NegativeInfinity, opts);
        [Fact] void ExplicitPositive() => Parse("+Infinity").Should().Be(double.PositiveInfinity, opts);
        [Fact] void Short() => Parse("+Inf").Should().Be(double.PositiveInfinity, opts);
        [Fact] void UpperCase() => Parse("INFINITY").Should().Be(double.PositiveInfinity, opts);
        [Fact] void LowerCase() => Parse("-infinity").Should().Be(double.NegativeInfinity, opts);
    }

    public class NaN {
        [Fact] void MixedCase() => Parse("NaN").Should().Be(double.NaN, opts);
        [Fact] void UpperCase() => Parse("NAN").Should().Be(double.NaN, opts);
        [Fact] void LowerCase() => Parse("nan").Should().Be(double.NaN, opts);
        [Fact] void Positive() => Parse("+NaN").Should().Be(double.NaN, opts);
        [Fact] void Negative() => Parse("-NaN").Should().Be(double.NaN, opts);
    }
}
