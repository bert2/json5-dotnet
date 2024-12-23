#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests;

using Helpers;

using FluentAssertions;

public class Objects {
    [Fact]
    void InvalidAsciiInMemberName() => Parser.Parse(@"{a\b:1}").Should().BeValue("");

    [Fact]
    void InvalidAsciiInMemberName2() => Parser.Parse("{\n\n    a\\b:1}").Should().BeValue("");

    [Fact]
    void EncodedInvalidAsciiInMemberName() => Parser.Parse(@"{a\u005Cb:1}").Should().BeValue("");

    [Fact]
    void EncodedInvalidAsciiInMemberName2() => Parser.Parse(@"{a\u0061b\u0062\u005Cc\u0063:1}").Should().BeValue("");

    [Fact]
    void EncodedInvalidAsciiInMemberName3() => Parser.Parse(@"{a\u{061}b\u{00062}\u{5C}c\u{0063}:1}").Should().BeValue("");

    [Fact]
    void InvalidUnicodeInMemberName() => Parser.Parse("{a\u200Ab:1}").Should().BeValue("");

    [Fact]
    void InvalidEncodedUnicodeInMemberName() => Parser.Parse(@"{a\u200Ab:1}").Should().BeValue("");
}
