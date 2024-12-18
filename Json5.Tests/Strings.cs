#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests;

using FluentAssertions;

using Helpers;

using static FluentAssertions.FluentActions;

public class Strings {
    [Fact] void DoubleQuoted() => Json5.Parse("\"strings are 'fun'\"").Should().BeValue("strings are 'fun'");
    [Fact] void SingleQuoted() => Json5.Parse("'strings are \"fun\"'").Should().BeValue("strings are \"fun\"");

    public static class Escaping {
        public class SingleCharacters {
            [Fact] void SingleQuote() => Json5.Parse(@"'let\'s go'").Should().BeValue("let's go");
            [Fact] void DoubleQuote() => Json5.Parse("\"...\\\"go\\\" where?\"").Should().BeValue("...\"go\" where?");
            [Fact] void Backslash() => Json5.Parse(@"'escape with \\'").Should().BeValue(@"escape with \");
            [Fact] void Backspace() => Json5.Parse(@"'go back with \b'").Should().BeValue("go back with \b");
            [Fact] void FormFeed() => Json5.Parse(@"'next page with \f'").Should().BeValue("next page with \f");
            [Fact] void Newline() => Json5.Parse(@"'break\nme'").Should().BeValue("break\nme");
            [Fact] void CarriageReturn() => Json5.Parse(@"'yea \r whatever'").Should().BeValue("yea \r whatever");
            [Fact] void Tab() => Json5.Parse(@"'space\tcreated'").Should().BeValue("space\tcreated");
            [Fact] void VerticalTab() => Json5.Parse(@"'space\vcreated'").Should().BeValue("space\vcreated");
            [Fact] void NullChar() => Json5.Parse(@"'terminate me\0'").Should().BeValue("terminate me\0");

            [Fact]
            void DigitsAreNotAllowed() => "123456789".ForEach(c =>
                Invoking(() => Json5.Parse($@"'\{c}'"))
                .Should().Throw<Exception>().WithMessage(
                    $"""
                    Error in Ln: 1 Col: 2
                    '\{c}'
                     ^
                    Expecting: next string character or escape sequence

                    escape sequence could not be parsed because:
                      Error in Ln: 1 Col: 3
                      '\{c}'
                        ^
                      Expecting: any char not in ‘123456789’

                    """));

            [Fact]
            void AnyOtherIsMappedToItself() => Enumerable
                .Range(0, 256)
                .Select(Convert.ToChar)
                .Except("'xubfnrtv0123456789\n\r\u0085")
                .ForEach(c =>
                    Json5.Parse($@"'\{c}'")
                    .Should().BeValue(c.ToString(), because: $"Char {c} (\\u{(int)c:X4}) should be mapped to itself"));
        }

        public class HexSequences {
            [Fact] void Example() => Json5.Parse(@"'\xFF'").Should().BeValue("ÿ");

            [Fact]
            void HexNumberIsConvertedToChar() => Enumerable
                .Range(0, 256)
                .Select(x => (Char: (char)x, Hex: x.ToString("x2")))
                .ForEach(x =>
                    Json5.Parse($@"'\x{x.Hex}'")
                    .Should().BeValue(x.Char.ToString(), because: $@"escape sequence \x{x.Hex} should be mapped to {x.Char}"));
        }

        public class UnicodeSequences {
            [Fact] void Example() => Json5.Parse(@"'\uACDC'").Should().BeValue("곜");

            [Fact] void SurrogatePairExample() => Json5.Parse(@"'\uD83C\uDFBC'").Should().BeValue("\U0001F3BC");

            [Fact]
            void HexNumberIsConvertedToChar() => Enumerable
                .Range(0, ushort.MaxValue + 1)
                .Select(x => (Char: new string((char)x, 1), Hex: x.ToString("x4")))
                .ForEach(x =>
                    Json5.Parse($@"'\u{x.Hex}'")
                    .Should().BeValue(x.Char, because: $@"escape sequence \u{x.Hex} should be mapped to {x.Char}"));
        }

        public class LineContinuations {
            [Fact]
            void NoUnescapedLineTerminator() =>
                Invoking(() => Json5.Parse("\"let's have a break, \nshall we\""))
                .Should().Throw<Exception>().WithMessage(
                    """
                    Error in Ln: 1 Col: 22
                    "let's have a break, 
                                         ^
                    Note: The error occurred at the end of the line.
                    Expecting: escape sequence, next string character or '"'

                    """);

            [Fact]
            void EscapedNewlinesAreIgnored() =>
                Json5.Parse(
                    """
                    "Look mom, I'm on \
                    multiple \
                    lines!"
                    """)
                .Should().BeValue("Look mom, I'm on multiple lines!");

            [Fact]
            void IgnoresIndentation() =>
                Json5.Parse(
                    """
                        "Look mom, I'm on \
                         multiple \
                         indented \
                         lines!"
                    """)
                .Should().BeValue("Look mom, I'm on multiple indented lines!");

            [Fact]
            void IndentationDepthDoesNotMatter() =>
                Json5.Parse(
                    """
                        "Look mom, I'm on \
                         multiple \
                              weirdly indented \
                    lines!"
                    """)
                .Should().BeValue("Look mom, I'm on multiple weirdly indented lines!");

            [Fact]
            void DoesNotSkipUnescapedNewlineWhenSkippingIndentation() =>
                Invoking(() => Json5.Parse(
                    """
                    "The next line \
                    
                     is invalid because \
                     of the unescaped newline."
                    """))
                .Should().Throw<Exception>().WithMessage(
                    """"
                    Error in Ln: 2 Col: 1
                    Note: The error occurred on an empty line.
                    Expecting: escape sequence, next string character or '"'
                    
                    """");

            [Fact] void AcceptsEscapedLf() => Json5.Parse("'break \\\nhere'").Should().BeValue("break here");
            [Fact] void AcceptsEscapedCr() => Json5.Parse("'break \\\rhere'").Should().BeValue("break here");
            [Fact] void AcceptsEscapedCrLf() => Json5.Parse("'break \\\r\nhere'").Should().BeValue("break here");
            [Fact] void AcceptsEscapedUnicodeLineSeparator() => Json5.Parse("'break \\\u2028here'").Should().BeValue("break here");
            [Fact] void AcceptsEscapedUnicodeParagraphSeparator() => Json5.Parse("'break \\\u2029here'").Should().BeValue("break here");
        }
    }
}
