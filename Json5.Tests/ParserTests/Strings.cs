#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests.ParserTests;

using FluentAssertions;

using Helpers;

using static FluentAssertions.FluentActions;

public class Strings {
    [Fact] void DoubleQuoted() => Parser.Parse("\"strings are 'fun'\"").Should().BeValue("strings are 'fun'");
    [Fact] void SingleQuoted() => Parser.Parse("'strings are \"fun\"'").Should().BeValue("strings are \"fun\"");

    public static class Escaping {
        public class SingleCharacters {
            [Fact] void SingleQuote() => Parser.Parse(@"'let\'s go'").Should().BeValue("let's go");
            [Fact] void DoubleQuote() => Parser.Parse("\"...\\\"go\\\" where?\"").Should().BeValue("...\"go\" where?");
            [Fact] void Backslash() => Parser.Parse(@"'escape with \\'").Should().BeValue(@"escape with \");
            [Fact] void Backspace() => Parser.Parse(@"'go back with \b'").Should().BeValue("go back with \b");
            [Fact] void FormFeed() => Parser.Parse(@"'next page with \f'").Should().BeValue("next page with \f");
            [Fact] void Newline() => Parser.Parse(@"'break\nme'").Should().BeValue("break\nme");
            [Fact] void CarriageReturn() => Parser.Parse(@"'yea \r whatever'").Should().BeValue("yea \r whatever");
            [Fact] void Tab() => Parser.Parse(@"'space\tcreated'").Should().BeValue("space\tcreated");
            [Fact] void VerticalTab() => Parser.Parse(@"'space\vcreated'").Should().BeValue("space\vcreated");
            [Fact] void NullChar() => Parser.Parse(@"'terminate me\0'").Should().BeValue("terminate me\0");

            [Fact]
            void DigitsAreNotAllowed() => "123456789".ForEach(c =>
                Invoking(() => Parser.Parse($@"'\{c}'"))
                .Should().Throw<Exception>().WithMessage(
                    $"""
                    Error in Ln: 1 Col: 2
                    '\{c}'
                     ^
                    Expecting: string character or escape sequence

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
                    Parser.Parse($@"'\{c}'")
                    .Should().BeValue(c.ToString(), because: $"Char {c} (\\u{(int)c:X4}) should be mapped to itself"));
        }

        public class HexSequences {
            [Fact] void Example() => Parser.Parse(@"'\xFF'").Should().BeValue("ÿ");

            [Fact]
            void HexNumberIsConvertedToChar() => Enumerable
                .Range(0, 256)
                .Select(x => (Char: (char)x, Hex: x.ToString("x2")))
                .ForEach(x =>
                    Parser.Parse($@"'\x{x.Hex}'")
                    .Should().BeValue(x.Char.ToString(), because: $@"escape sequence \x{x.Hex} should be mapped to {x.Char}"));
        }

        public class UnicodeSequences {
            [Fact] void Example() => Parser.Parse(@"'\uACDC'").Should().BeValue("곜");

            [Fact] void SurrogatePairExample() => Parser.Parse(@"'\uD83C\uDFBC'").Should().BeValue("\U0001F3BC");

            [Fact]
            void HexNumberIsConvertedToChar() => Enumerable
                .Range(0, ushort.MaxValue + 1)
                .Select(x => (Char: new string((char)x, 1), Hex: x.ToString("x4")))
                .ForEach(x =>
                    Parser.Parse($@"'\u{x.Hex}'")
                    .Should().BeValue(x.Char, because: $@"escape sequence \u{x.Hex} should be mapped to {x.Char}"));

            public class ExplicitCodepoints {
                [Fact] void Example() => Parser.Parse(@"'\u{000061}'").Should().BeValue("a");

                [Fact] void VariableLength() => Parser.Parse(@"'\u{62}'").Should().BeValue("b");

                [Fact] void PrependedZerosAreIgnored() => Parser.Parse(@"'\u{00000000000000000000063}'").Should().BeValue("c");

                [Fact] void MaxSize() => Parser.Parse(@"'\u{10FFFF}'").Should().BeValue("\U0010ffff");

                [Fact]
                void MaxSizePlus1() =>
                    Invoking(() => Parser.Parse(@"'\u{110000}'"))
                    .Should().Throw<Exception>().WithMessage(
                        """
                        Error in Ln: 1 Col: 2
                        '\u{110000}'
                         ^
                        Expecting: string character or escape sequence

                        escape sequence could not be parsed because:
                          Error in Ln: 1 Col: 12
                          '\u{110000}'
                                     ^
                          Invalid Unicode escape sequence: \u{110000}
                        """);

                [Fact]
                void Empty() =>
                    Invoking(() => Parser.Parse(@"'\u{}'"))
                    .Should().Throw<Exception>().WithMessage(
                        """
                        Error in Ln: 1 Col: 2
                        '\u{}'
                         ^
                        Expecting: string character or escape sequence

                        escape sequence could not be parsed because:
                          Error in Ln: 1 Col: 5
                          '\u{}'
                              ^
                          Expecting: hexadecimal digit
                        """);

                [Fact]
                void UnclodesBraces() =>
                    Invoking(() => Parser.Parse(@"'\u{64'"))
                    .Should().Throw<Exception>().WithMessage(
                        """
                        Error in Ln: 1 Col: 2
                        '\u{64'
                         ^
                        Expecting: string character or escape sequence

                        escape sequence could not be parsed because:
                          Error in Ln: 1 Col: 7
                          '\u{64'
                                ^
                          Expecting: hexadecimal digit or '}'
                        """);
            }
        }

        public class LineContinuations {
            [Fact]
            void NoUnescapedLineTerminator() =>
                Invoking(() => Parser.Parse("\"let's have a break, \nshall we\""))
                .Should().Throw<Exception>().WithMessage(
                    """
                    Error in Ln: 1 Col: 22
                    "let's have a break, 
                                         ^
                    Note: The error occurred at the end of the line.
                    Expecting: escape sequence, string character or '"'
                    """);

            [Fact]
            void EscapedNewlinesAreIgnored() =>
                Parser.Parse(
                    """
                    "Look mom, I'm on \
                    multiple \
                    lines!"
                    """)
                .Should().BeValue("Look mom, I'm on multiple lines!");

            [Fact]
            void IgnoresIndentation() =>
                Parser.Parse(
                    """
                        "Look mom, I'm on \
                         multiple \
                         indented \
                         lines!"
                    """)
                .Should().BeValue("Look mom, I'm on multiple indented lines!");

            [Fact]
            void IndentationDepthDoesNotMatter() =>
                Parser.Parse(
                    """
                        "Look mom, I'm on \
                         multiple \
                              weirdly indented \
                    lines!"
                    """)
                .Should().BeValue("Look mom, I'm on multiple weirdly indented lines!");

            [Fact]
            void DoesNotSkipUnescapedNewlineWhenSkippingIndentation() =>
                Invoking(() => Parser.Parse(
                    """
                    "The next line \
                    
                     is invalid because \
                     of the unescaped newline."
                    """))
                .Should().Throw<Exception>().WithMessage(
                    """"
                    Error in Ln: 2 Col: 1
                    Note: The error occurred on an empty line.
                    Expecting: escape sequence, string character or '"'
                    """");

            [Fact] void AcceptsEscapedLf() => Parser.Parse("'break \\\nhere'").Should().BeValue("break here");
            [Fact] void AcceptsEscapedCr() => Parser.Parse("'break \\\rhere'").Should().BeValue("break here");
            [Fact] void AcceptsEscapedCrLf() => Parser.Parse("'break \\\r\nhere'").Should().BeValue("break here");
            [Fact] void AcceptsEscapedNextLineCharacter() => Parser.Parse("'break \\\u0085here'").Should().BeValue("break here");
            [Fact] void AcceptsEscapedLineSeparator() => Parser.Parse("'break \\\u2028here'").Should().BeValue("break here");
            [Fact] void AcceptsEscapedParagraphSeparator() => Parser.Parse("'break \\\u2029here'").Should().BeValue("break here");
        }
    }
}
