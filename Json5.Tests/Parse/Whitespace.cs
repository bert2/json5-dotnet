#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests.Parse;

using Helpers;

using static Json5.Json5Parser;

public class Whitespace {
    [Fact] void Leading() => Parse("  \n  \t  42").Should().Be(42);
    [Fact] void Trailing() => Parse("3.5  \n  \t  ").Should().Be(3.5);

    [Fact]
    void AllWhitespaceIsSkipped() =>
        Parse("\t\n\v\f\r \u00A0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A\u2028\u2029\u202F\u205F\u3000'we done yet?'")
        .Should().Be("we done yet?");

    [Fact] void BomIsSkipped() => Parse("\uFEFF'nope not yet'").Should().Be("nope not yet");

    // The "next line" character \u0085 is actually not whitespace according to the JSON5 spec but char.IsWhiteSpace()
    // says it is so we accept it as well.
    [Fact] void NextLineCharIsSkipped() => Parse("\u0085'now we done'").Should().Be("now we done");
}
