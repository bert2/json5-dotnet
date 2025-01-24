#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests.ParserTests;

using FluentAssertions;

using Helpers;

using static FluentAssertions.FluentActions;
using static Json5.Json5Parser;

public static class Comments {
    public class SingleLine {
        [Fact]
        void Leading() => Parse(
            """
            // this is a string
            'hello world'
            """).Should().Be("hello world");

        [Fact] void Trailing() => Parse("true // this is a bool").Should().Be(true);

        [Fact]
        void InsideString() =>
            Parse("\"This inline comment // isn't really an inline comment.\"")
            .Should().Be("This inline comment // isn't really an inline comment.");

        [Fact]
        void CommentOnlyNotAllowed() =>
            Invoking(() => Parse("// blah"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 8
                // blah
                       ^
                Note: The error occurred at the end of the input stream.
                Expecting: array, bool, null, number, object or string
                """);

        [Fact] void TerminatedWithCr() => Parse("//comment\r1.23").Should().Be(1.23);
        [Fact] void TerminatedWithLf() => Parse("//comment\n1.23").Should().Be(1.23);
        [Fact] void TerminatedWithCrLf() => Parse("//comment\r\n1.23").Should().Be(1.23);
        [Fact] void TerminatedWithNextLineCharacter() => Parse("//comment\u00851.23").Should().Be(1.23);
        [Fact] void TerminatedWithLineSeparator() => Parse("//comment\u20281.23").Should().Be(1.23);
        [Fact] void TerminatedWithParagraphSeparator() => Parse("//comment\u20291.23").Should().Be(1.23);
    }

    public class Multiline {
        [Fact]
        void Leading() =>
            Parse(
                """
                /* there is a number
                   on this line */ 23.45
                """)
            .Should().Be(23.45);

        [Fact]
        void Trailing() =>
            Parse(
                """
                13 /* this line
                        has a number value */
                """)
            .Should().Be(13);

        [Fact]
        void JavaDocStyle() =>
            Parse(
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
            Parse("\"This /* block comment */ isn't really a block comment.\"")
            .Should().Be("This /* block comment */ isn't really a block comment.");

        [Fact]
        void MustBeClosed() =>
            Invoking(() => Parse("/* blah"))
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
            Invoking(() => Parse("/* blah */"))
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
