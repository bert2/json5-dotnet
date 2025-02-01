#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests.Parse;

using FluentAssertions;

using Helpers;

using System.Text.Json.Nodes;

using static FluentAssertions.FluentActions;
using static Json5.JSON5;

public class Arrays {
    [Fact] void Empty() => Parse("[]").Should().Be(Array.Empty<object>());
    [Fact] void Singleton() => Parse("[1]").Should().Be<int[]>([1]);
    [Fact] void Multiple() => Parse("[1,2,3]").Should().Be<int[]>([1, 2, 3]);

    [Fact]
    void MixedTypes() =>
        Parse("[null,1,0.2,'foo',true]")
        .Should().Be(new JsonArray(null, 1, 0.2, "foo", true));

    [Fact]
    void CommaRequired() =>
        Invoking(() => Parse(
            """
            [
                true
                false
            ]
            """))
        .Should().Throw<Exception>().WithMessage(
            """
            Error in Ln: 3 Col: 5
                false
                ^
            Expecting: ',' or ']'
            """);

    [Fact] void TrailingCommaAllowed() => Parse("['bar',]").Should().Be<string[]>(["bar"]);

    [Fact]
    void LeadingCommaNotAllowed() =>
        Invoking(() => Parse("[,'qux']"))
        .Should().Throw<Exception>().WithMessage(
            """
            Error in Ln: 1 Col: 2
            [,'qux']
             ^
            Expecting: array, bool, null, number, object, string or ']'
            """);

    [Fact]
    void Whitespace() =>
        Parse(
            """
               
               [      1,1,1,1,
                    1,        1,   
                   1,  0,  0,  1,
                  1,            1,
                  1,     7,     1,
                  1,            1,
                   1, 8,8,8,8, 1,
                    1,        1,
                      1,1,1,1,
                ]   

            """)
        .Should().Be<int[]>([1, 1, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 7, 1, 1, 1, 1, 8, 8, 8, 8, 1, 1, 1, 1, 1, 1, 1]);

    [Fact]
    void SingleLineComments() =>
        Parse(
            """
            // foo
            [// bar
            1// baz
            ,// qux
            ]// corge
            """)
        .Should().Be<int[]>([1]);

    [Fact]
    void MultilineComments() =>
        Parse(
            """
            /* foo
            */[/* bar
            */2/* qux
            */,/* baz
            */]/* corge
            */
            """)
        .Should().Be<int[]>([2]);

    [Fact]
    void MustBeClosed() =>
        Invoking(() => Parse("[true,false"))
        .Should().Throw<Exception>().WithMessage(
            """
            Error in Ln: 1 Col: 12
            [true,false
                       ^
            Note: The error occurred at the end of the input stream.
            Expecting: ',' or ']'
            """);

    public class Nested {
        [Fact] void Empty() => Parse("[[],[]]").Should().Be<object[][]>([[], []]);

        [Fact]
        void Multiple() =>
            Parse("[[1,2],[3,4],[5,6]]")
            .Should().Be<int[][]>([[1, 2], [3, 4], [5, 6]]);

        [Fact]
        void MixedTypes() =>
            Parse("[[null,1,0.2],['bar',true]]")
            .Should().Be(new JsonArray(new JsonArray(null, 1, 0.2), new JsonArray("bar", true)));
    }
}
