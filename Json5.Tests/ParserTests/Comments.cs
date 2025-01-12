#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests.ParserTests;

using FluentAssertions;

using Helpers;

using static FluentAssertions.FluentActions;

public static class Comments {
    public class SingleLine {
        [Fact]
        void Leading() => Parser.Parse2(
            """
            // this is a string
            'hello world'
            """).Should().Be("hello world");

        [Fact] void Trailing() => Parser.Parse2("true // this is a bool").Should().Be(true);

        [Fact]
        void InsideString() =>
            Parser.Parse2("\"This inline comment // isn't really an inline comment.\"")
            .Should().Be("This inline comment // isn't really an inline comment.");

        [Fact]
        void CommentOnlyNotAllowed() =>
            Invoking(() => Parser.Parse2("// blah"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 8
                // blah
                       ^
                Note: The error occurred at the end of the input stream.
                Expecting: array, bool, null, number, object or string
                """);

        [Fact] void TerminatedWithCr() => Parser.Parse2("//comment\r1.23").Should().Be(1.23);
        [Fact] void TerminatedWithLf() => Parser.Parse2("//comment\n1.23").Should().Be(1.23);
        [Fact] void TerminatedWithCrLf() => Parser.Parse2("//comment\r\n1.23").Should().Be(1.23);
        [Fact] void TerminatedWithNextLineCharacter() => Parser.Parse2("//comment\u00851.23").Should().Be(1.23);
        [Fact] void TerminatedWithLineSeparator() => Parser.Parse2("//comment\u20281.23").Should().Be(1.23);
        [Fact] void TerminatedWithParagraphSeparator() => Parser.Parse2("//comment\u20291.23").Should().Be(1.23);
    }

    public class Multiline {
        [Fact]
        void Leading() => Parser.Parse2(
            """
            /* there is a number
               on this line */ 23.45
            """).Should().Be(23.45);

        [Fact]
        void Trailing() =>
            Parser.Parse2(
                """
                null /* this line
                        has a null value */
                """)
            .Should().BeNull();

        [Fact]
        void JavaDocStyle() =>
            Parser.Parse2(
                """
                /**
                 * This is a JavaDoc-like block comment.
                 * It contains asterisks inside of it.
                 * It might also be closed with multiple asterisks.
                 * Like this:
                 **/
                true
                """)
            .Should().Be(true);

        [Fact]
        void InsideString() =>
            Parser.Parse2("\"This /* block comment */ isn't really a block comment.\"")
            .Should().Be("This /* block comment */ isn't really a block comment.");

        [Fact]
        void MustBeClosed() =>
            Invoking(() => Parser.Parse2("/* blah"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 8
                /* blah
                       ^
                Note: The error occurred at the end of the input stream.
                Could not find the string '*/'.
                """);

        [Fact]
        void CommentOnlyNotAllowed() =>
            Invoking(() => Parser.Parse2("/* blah */"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 11
                /* blah */
                          ^
                Note: The error occurred at the end of the input stream.
                Expecting: array, bool, null, number, object or string
                """);
    }
}
