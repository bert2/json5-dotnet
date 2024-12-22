#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests;

using FluentAssertions;

using Helpers;

using static FluentAssertions.FluentActions;

public static class Comments {
    public class SingleLine {
        [Fact]
        void Leading() => Parser.Parse(
            """
            // this is a string
            'hello world'
            """).Should().BeValue("hello world");

        [Fact] void Trailing() => Parser.Parse("true // this is a bool").Should().BeValue(true);
    }

    public class Multiline {
        [Fact]
        void Leading() => Parser.Parse(
            """
            /* there is a money value
               on this line */ 23.45m
            """).Should().BeValue(23.45m);

        [Fact]
        void Trailing() => Parser.Parse(
            """
            null /* this line
                    has a null value */
            """).Should().BeNull();

        [Fact]
        void MustBeClosed() =>
            Invoking(() => Parser.Parse("/* blah"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 8
                /* blah
                       ^
                Note: The error occurred at the end of the input stream.
                Could not find the string '*/'.
                
                """);
    }
}
