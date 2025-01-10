#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests.ParserTests;

using FluentAssertions;

using FsCheck;
using FsCheck.Xunit;

using Helpers;

using System.Text.Json;

using static FluentAssertions.FluentActions;
using static Helpers.Constants;

public class Money {
    [Fact] void RequiresSuffix() => Parser.Parse("0.123456789m").Should().Be(0.123456789m);
    [Fact] void ParsesDoubleWithoutSuffix() => Parser.Parse("0.3333333333333333333333333333").Should().Be(0.3333333333333333d);
    [Fact] void LowerCaseLiteralSuffix() => Parser.Parse("1.23m").Should().Be(1.23m);
    [Fact] void UpperCaseLiteralSuffix() => Parser.Parse("1.23M").Should().Be(1.23m);

    [Fact]
    void OnlyMSuffixIsAllowed() =>
        Invoking(() => Parser.Parse("123L"))
        .Should().Throw<NotSupportedException>().WithMessage("Format of the number literal 123L is not supported.");

    [Fact] void MinDecimal() => Parser.Parse("-79228162514264337593543950335m").Should().Be(decimal.MinValue);
    [Fact] void MaxDecimal() => Parser.Parse("79228162514264337593543950335m").Should().Be(decimal.MaxValue);
    [Fact] void Epsilon() => Parser.Parse("0.0000000000000000000000000001m").Should().Be(0.0000000000000000000000000001m);
    [Fact] void SmallerThanEpsilon() => Parser.Parse("0.00000000000000000000000000001m").Should().Be(0m);

    [Property(MaxTest = N)] bool Generated(decimal x) => RoundTrip(x) == x;

    private static decimal RoundTrip(decimal x) => Parser.Parse($"{x}m")!.Deserialize<decimal>();
}
