#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests;

using FluentAssertions;

using FsCheck;
using FsCheck.Xunit;

using Helpers;

using static FluentAssertions.FluentActions;
using static Helpers.Constants;

public class Money {
    [Fact] void RequiresSuffix() => Parser.Parse("0.123456789m").Should().BeValue(0.123456789m);
    [Fact] void ParsesDoubleWithoutSuffix() => Parser.Parse("0.3333333333333333333333333333").Should().BeValue(0.3333333333333333d);
    [Fact] void IgnoresSuffixCase() => Parser.Parse("1.23m").Should().Be(Parser.Parse("1.23M"));

    [Fact]
    void OnlyMSuffixIsAllowed() =>
        Invoking(() => Parser.Parse("123L"))
        .Should().Throw<NotSupportedException>().WithMessage("Format of the number literal 123L is not supported.");

    [Fact] void MinDecimal() => Parser.Parse("-79228162514264337593543950335m").Should().BeValue(decimal.MinValue);
    [Fact] void MaxDecimal() => Parser.Parse("79228162514264337593543950335m").Should().BeValue(decimal.MaxValue);
    [Fact] void Epsilon() => Parser.Parse("0.0000000000000000000000000001m").Should().BeValue(0.0000000000000000000000000001m);
    [Fact] void SmallerThanEpsilon() => Parser.Parse("0.00000000000000000000000000001m").Should().BeValue(0m);

    [Property(MaxTest = N)] bool Generated(decimal x) => RoundTrip(x) == x;

    private static decimal RoundTrip(decimal x) => Parser.Parse($"{x}m")!.GetValue<decimal>();
}
