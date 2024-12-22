#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests;

using Helpers;

public class Whitespace {
    [Fact] void Leading() => Parser.Parse("  \n  \t  42").Should().BeValue(42);
    [Fact] void Trailing() => Parser.Parse("3.5  \n  \t  ").Should().BeValue(3.5);

    [Fact]
    void AllWhitespaceIsSkipped() =>
        Parser.Parse("\t\n\v\f\r \u0085\u00A0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A\u2028\u2029\u202F\u205F\u3000'we done yet?'")
        .Should().BeValue("we done yet?");

    [Fact] void BomIsSkippedAsWell() => Parser.Parse("\uFEFF'nope not yet'").Should().BeValue("nope not yet");

    // The "next line" character \u0085 is actually not whitespace according to the JSON5 spec but char.IsWhiteSpace()
    // says it is so we accept it as well.
    [Fact] void NextLineCharIsSkippedToo() => Parser.Parse("\u0085'now we done'").Should().BeValue("now we done");
}
