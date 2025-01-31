#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests.Parse;

using FluentAssertions;

using Helpers;

using static FluentAssertions.FluentActions;
using static Json5.Json5Parser;

public class Strings {
    [Fact] void DoubleQuoted() => Parse("\"strings are 'fun'\"").Should().Be("strings are 'fun'");
    [Fact] void SingleQuoted() => Parse("'strings are \"fun\"'").Should().Be("strings are \"fun\"");

    public static class Escaping {
        public class SingleCharacters {
            [Fact] void SingleQuote() => Parse(@"'let\'s go'").Should().Be("let's go");
            [Fact] void DoubleQuote() => Parse("\"...\\\"go\\\" where?\"").Should().Be("...\"go\" where?");
            [Fact] void Backslash() => Parse(@"'escape with \\'").Should().Be(@"escape with \");
            [Fact] void Backspace() => Parse(@"'go back with \b'").Should().Be("go back with \b");
            [Fact] void FormFeed() => Parse(@"'next page with \f'").Should().Be("next page with \f");
            [Fact] void Newline() => Parse(@"'break\nme'").Should().Be("break\nme");
            [Fact] void CarriageReturn() => Parse(@"'yea \r whatever'").Should().Be("yea \r whatever");
            [Fact] void Tab() => Parse(@"'space\tcreated'").Should().Be("space\tcreated");
            [Fact] void VerticalTab() => Parse(@"'space\vcreated'").Should().Be("space\vcreated");
            [Fact] void NullChar() => Parse(@"'terminate me\0'").Should().Be("terminate me\0");

            [Fact]
            void DigitsAreNotAllowed() => "123456789".ForEach(c =>
                Invoking(() => Parse($@"'\{c}'"))
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
                .Range(0, ushort.MaxValue + 1)
                .Select(Convert.ToChar)
                .Where(c => !char.IsSurrogate(c))
                .Except("'xubfnrtv0123456789\n\r\u0085\u2028\u2029\uFFFF")
                .ForEach(c => Parse($@"'\{c}'") .Should().Be(c.ToString()));
        }

        public class HexSequences {
            [Fact] void Example() => Parse(@"'\xFF'").Should().Be("ÿ");

            [Fact]
            void HexNumberIsConvertedToChar() => Enumerable
                .Range(0, 256)
                .Select(x => (Char: (char)x, Hex: x.ToString("x2")))
                .ForEach(x =>
                    Parse($@"'\x{x.Hex}'")
                    .Should().Be(x.Char.ToString(), because: $@"escape sequence \x{x.Hex} should be mapped to {x.Char}"));
        }

        public class UnicodeSequences {
            [Fact] void Example() => Parse(@"'\uACDC'").Should().Be("곜");

            [Fact] void SurrogatePair() => Parse(@"'\uD83C\uDFBC'").Should().Be("\U0001F3BC");

            [Fact]
            void BasicMultilingualPlaneExceptSurrogates() => Enumerable
                .Range(0, ushort.MaxValue + 1)
                .Where(x => !char.IsSurrogate((char)x))
                .ForEach(x => Parse($@"'\u{x:X4}'").Should().Be(new string((char)x, 1)));

            [Fact]
            void LoneSurrogatesAreEncodedWithReplacementCharacter() => Enumerable
                .Range(0, ushort.MaxValue + 1)
                .Where(x => char.IsSurrogate((char)x))
                .ForEach(x => Parse($@"'\u{x:X4}'").Should().Be("\uFFFD"));

            public class ExplicitCodePoints {
                [Fact] void Example() => Parse(@"'\u{000061}'").Should().Be("a");

                [Fact] void VariableLength() => Parse(@"'\u{62}'").Should().Be("b");

                [Fact] void PrependedZerosAreIgnored() => Parse(@"'\u{00000000000000000000063}'").Should().Be("c");

                [Fact] void MaxSize() => Parse(@"'\u{10FFFF}'").Should().Be("\U0010FFFF");

                [Fact]
                void AllCodePoints() => Enumerable
                    .Range(0, 0x10FFFF + 1)
                    .Where(i => i > 0xFFFF || !char.IsSurrogate((char)i))
                    .ForEach(i => Parse($@"'\u{{{i:X6}}}'").Should().Be(char.ConvertFromUtf32(i)));

                [Fact]
                void MaxSizePlus1() =>
                    Invoking(() => Parse(@"'\u{110000}'"))
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
                    Invoking(() => Parse(@"'\u{}'"))
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
                void UnclosedBraces() =>
                    Invoking(() => Parse(@"'\u{64'"))
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
                Invoking(() => Parse("\"let's have a break, \nshall we\""))
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
                Parse(
                    """
                    "Look mom, I'm on \
                    multiple \
                    lines!"
                    """)
                .Should().Be("Look mom, I'm on multiple lines!");

            [Fact]
            void IgnoresIndentation() =>
                Parse(
                    """
                        "Look mom, I'm on \
                         multiple \
                         indented \
                         lines!"
                    """)
                .Should().Be("Look mom, I'm on multiple indented lines!");

            [Fact]
            void IndentationDepthDoesNotMatter() =>
                Parse(
                    """
                        "Look mom, I'm on \
                         multiple \
                              weirdly indented \
                    lines!"
                    """)
                .Should().Be("Look mom, I'm on multiple weirdly indented lines!");

            [Fact]
            void DoesNotSkipUnescapedNewlineWhenSkippingIndentation() =>
                Invoking(() => Parse(
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

            [Fact] void AcceptsEscapedLf() => Parse("'break \\\nhere'").Should().Be("break here");
            [Fact] void AcceptsEscapedCr() => Parse("'break \\\rhere'").Should().Be("break here");
            [Fact] void AcceptsEscapedCrLf() => Parse("'break \\\r\nhere'").Should().Be("break here");
            [Fact] void AcceptsEscapedNextLineCharacter() => Parse("'break \\\u0085here'").Should().Be("break here");
            [Fact] void AcceptsEscapedLineSeparator() => Parse("'break \\\u2028here'").Should().Be("break here");
            [Fact] void AcceptsEscapedParagraphSeparator() => Parse("'break \\\u2029here'").Should().Be("break here");
        }
    }
}
