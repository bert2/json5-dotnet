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
    void EncodedInvalidAsciiInMemberName2() => Parser.Parse(@"{a\u005Cb\u0061:1}").Should().BeValue("");

    [Fact]
    void InvalidUnicodeInMemberName() => Parser.Parse("{a\u200Ab:1}").Should().BeValue("");

    [Fact]
    void InvalidEncodedUnicodeInMemberName() => Parser.Parse(@"{a\u200Db:1}").Should().BeValue("");
}
