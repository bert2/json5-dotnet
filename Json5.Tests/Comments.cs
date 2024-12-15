#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests;

public static class Comments {
    public class SingleLine {
        [Fact]
        void Leading() => Json5.Parse(
            """
            // this is a string
            'hello world'
            """).Should().BeValue("hello world");

        [Fact] void Trailing() => Json5.Parse("true // this is a bool").Should().BeValue(true);
    }

    public class Multiline {
        [Fact]
        void Leading() => Json5.Parse(
            """
            /* there is a money value
               on this line */ 23.45m
            """).Should().BeValue(23.45m);

        [Fact]
        void Trailing() => Json5.Parse(
            """
            null /* this line
                    has a null value */
            """).Should().BeNull();
    }
}
