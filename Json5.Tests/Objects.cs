#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests;

using Helpers;

using FluentAssertions;

public class Objects {
    [Fact]
    void InvalidAsciiInMemberName() => Json5.Parse(@"{a\b:1}").Should().BeValue("");

    [Fact]
    void InvalidAsciiInMemberName2() => Json5.Parse("{\n\n    a\\b:1}").Should().BeValue("");

    [Fact]
    void EncodedInvalidAsciiInMemberName() => Json5.Parse(@"{a\u005Cb:1}").Should().BeValue("");

    [Fact]
    void EncodedInvalidAsciiInMemberName2() => Json5.Parse(@"{a\u005Cb\u0061:1}").Should().BeValue("");

    [Fact]
    void InvalidUnicodeInMemberName() => Json5.Parse("{a\u200Ab:1}").Should().BeValue("");

    [Fact]
    void InvalidEncodedUnicodeInMemberName() => Json5.Parse(@"{a\u200Db:1}").Should().BeValue("");
}
