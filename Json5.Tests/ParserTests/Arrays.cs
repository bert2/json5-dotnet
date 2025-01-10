#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests.ParserTests;

using FluentAssertions;

using Helpers;

using static FluentAssertions.FluentActions;
using static Helpers.Util;

public class Arrays {
    [Fact] void Empty() => Parser.Parse("[]").Should().BeArray();
    [Fact] void Singleton() => Parser.Parse("[1]").Should().BeArray(1);
    [Fact] void Multiple() => Parser.Parse("[1,2,3]").Should().BeArray(1, 2, 3);

    [Fact]
    void MixedTypes() =>
        Parser.Parse("[null,1,0.2,3m,'foo',true]")
        .Should().BeArray(null, 1, 0.2, 3m, "foo", true);

    [Fact]
    void CommaRequired() =>
        Invoking(() => Parser.Parse(
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

    [Fact] void TrailingCommaAllowed() => Parser.Parse("['bar',]").Should().BeArray("bar");

    [Fact]
    void LeadingCommaNotAllowed() =>
        Invoking(() => Parser.Parse("[,'qux']"))
        .Should().Throw<Exception>().WithMessage(
            """
            Error in Ln: 1 Col: 2
            [,'qux']
             ^
            Expecting: array, bool, null, number, object, string or ']'
            """);

    [Fact]
    void Whitespace() =>
        Parser.Parse(
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
        .Should().BeArray(1, 1, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 7, 1, 1, 1, 1, 8, 8, 8, 8, 1, 1, 1, 1, 1, 1, 1);

    [Fact]
    void SingleLineComments() =>
        Parser.Parse(
            """
            // foo
            [// bar
            1// baz
            ,// qux
            ]// corge
            """)
        .Should().BeArray(1);

    [Fact]
    void MultilineComments() =>
        Parser.Parse(
            """
            /* foo
            */[/* bar
            */2/* qux
            */,/* baz
            */]/* corge
            */
            """)
        .Should().BeArray(2);

    [Fact]
    void MustBeClosed() =>
        Invoking(() => Parser.Parse("[true,false"))
        .Should().Throw<Exception>().WithMessage(
            """
            Error in Ln: 1 Col: 12
            [true,false
                       ^
            Note: The error occurred at the end of the input stream.
            Expecting: ',' or ']'
            """);

    public class Nested {
        [Fact] void Empty() => Parser.Parse("[[],[]]").Should().BeArray(Arr(), Arr());

        [Fact]
        void Multiple() =>
            Parser.Parse("[[1,2],[3,4],[5,6]]")
            .Should().BeArray(Arr(1, 2), Arr(3, 4), Arr(5, 6));

        [Fact]
        void MixedTypes() =>
            Parser.Parse("[[null,1,0.2],[3m,'bar',true]]")
            .Should().BeArray(Arr(null, 1, 0.2), Arr(3m, "bar", true));
    }
}
