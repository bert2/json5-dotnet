#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests;

using FluentAssertions;

using Helpers;

using System.Text.Json.Nodes;

using static FluentAssertions.FluentActions;

public class Arrays {
    [Fact] void Empty() => Json5.Parse("[]").Should().BeArray().And.BeEmpty();
    [Fact] void Singleton() => Json5.Parse("[1]").Should().BeArray().And.ContainSingle().Which.Should().BeValue(1);
    [Fact] void Multiple() => Json5.Parse("[1,2,3]").Should().BeArray().And.Equal(1, 2, 3);

    [Fact]
    void MixedTypes() =>
        Json5.Parse("[null,1,0.2,3m,'foo',true]")
        .Should().BeArray().And.Equal(null, 1, 0.2, 3m, "foo", true);

    [Fact] void TrailingCommaAllowed() => Json5.Parse("['bar',]").Should().BeArray().And.Equal("bar");

    [Fact]
    void Whitespace() =>
        Json5.Parse(
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
        .Should().BeArray().And.NotBeEmpty();

    [Fact]
    void SingleLineComments() =>
        Json5.Parse(
            """
            // foo
            [// bar
            1// qux
            ,// baz
            ]// kyn
            """)
        .Should().BeArray().And.Equal(1);

    [Fact]
    void MultilineComments() =>
        Json5.Parse(
            """
            /* foo
            */[/* bar
            */2/* qux
            */,/* baz
            */]/* kyn
            */
            """)
        .Should().BeArray().And.Equal(2);

    [Fact]
    void MustBeClosed() =>
        Invoking(() => Json5.Parse("[true,false"))
        .Should().Throw<Exception>().WithMessage(
            """
            Error in Ln: 1 Col: 12
            [true,false
                       ^
            Note: The error occurred at the end of the input stream.
            Expecting: ',' or ']'
            
            """);

    public class Nested {
        [Fact] void Empty() => Json5.Parse("[[],[]]").Should().BeArray().And.Equal(new JsonArray(), new JsonArray());

        [Fact]
        void Multiple() =>
            Json5.Parse("[[1,2],[3,4],[5,6]]")
            .Should().BeArray().And.Equal(new JsonArray(1, 2), new JsonArray(3, 4), new JsonArray(5, 6));

        [Fact]
        void MixedTypes() =>
            Json5.Parse("[[null,1,0.2],[3m,'bar',true]]")
            .Should().BeArray().And.Equal(new JsonArray(null, 1, 0.2), new JsonArray(3m, "bar", true));
    }
}
