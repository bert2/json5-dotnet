#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests.ParserTests;

using FluentAssertions;

using Helpers;

using System.Text.Json.Nodes;

using static FluentAssertions.FluentActions;

public class Objects {
    [Fact] void Empty() => Parser.Parse("{}").Should().BeEmptyObject();

    [Fact]
    void PrimitiveProperties() =>
        Parser.Parse("{ a: null, b: true, c: 1, d: .1, e: 1m, f: 'hi' }")
        .Should().Be(new { a = (string?)null, b = true, c = 1, d = .1, e = 1m, f = "hi" });

    [Fact]
    void ComplexProperties() =>
        Parser.Parse("{ a: { b: [], c: { d: [] } } }")
        .Should().Be(new { a = new { b = Array.Empty<object>(), c = new { d = Array.Empty<object>() } } });

    [Fact] void TrailingCommaAllowed() => Parser.Parse("{ a: 1, }").Should().Be(new { a = 1 });

    [Fact]
    void OnlyCommaNotAllowed() =>
        Invoking(() => Parser.Parse("{ , }"))
        .Should().Throw<Exception>().WithMessage(
            """
            Error in Ln: 1 Col: 3
            { , }
              ^
            Expecting: property or '}'
            """);

    [Fact]
    void Whitespace() =>
        Parser.Parse(
            """
               
               {
                     
                  a    
                  :   
                  1   

                  ,   
                    
               }

            """)
        .Should().Be(new { a = 1 });

    [Fact]
    void SingleLineComments() =>
        Parser.Parse(
            """
            // foo
            {// bar
            a// baz
            :// qux
            1// corge
            ,// garply
            }// waldo
            """)
        .Should().Be(new { a = 1 });

    [Fact]
    void MultilineComments() =>
        Parser.Parse(
            """
            /* foo
            */{/* bar
            */a/* baz
            */:/* qux
            */1/* corge
            */,/* garply
            */}/* waldo
            */
            """)
        .Should().Be(new { a = 1 });

    [Fact]
    void MustBeClosed() =>
        Invoking(() => Parser.Parse("{ a: 1"))
        .Should().Throw<Exception>().WithMessage(
            """
            Error in Ln: 1 Col: 7
            { a: 1
                  ^
            Note: The error occurred at the end of the input stream.
            Expecting: ',' or '}'
            """);

    public class MemberNames {
        [Fact] void SingleQuotes() => Parser.Parse("{ 'foo': 1, }").Should().Be(new { foo = 1 });

        [Fact] void DoubleQuotes() => Parser.Parse("{ \"foo\": 1, }").Should().Be(new { foo = 1 });

        [Fact]
        void QuotesAllowEverything() =>
            Parser.Parse("{ 'boo! 👻': 666 }")
            .Should().Be(new JsonObject([KeyValuePair.Create("boo! 👻", (JsonNode?)666)]));

        [Fact]
        void Duplicates() =>
            Invoking(() => Parser.Parse("{a: 1, a: 2}"))
            .Should().Throw<Exception>().WithMessage("An item with the same key has already been added. Key: a (Parameter 'key')");

        [Fact]
        void InvalidLeadingComma() =>
            Invoking(() => Parser.Parse("{, a: 1}"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 2
                {, a: 1}
                 ^
                Expecting: property or '}'
                """);

        public class NoQuotes {
            [Fact] void Allowed() => Parser.Parse("{ foo: 1, }").Should().Be(new { foo = 1 });

            [Fact]
            void DollarAllowed() =>
                Parser.Parse("{ $foo: 1, }")
                .Should().Be(new JsonObject([KeyValuePair.Create("$foo", (JsonNode?)1)]));

            [Fact] void UnderscoreAllowed() => Parser.Parse("{ _foo: 1, }").Should().Be(new { _foo = 1 });

            [Fact]
            void UmlautAllowed() =>
                Parser.Parse("{ ümlåût: 'ümlaüt is löve, ümlaüt is lïfe' }")
                .Should().Be(new { ümlåût = "ümlaüt is löve, ümlaüt is lïfe" });

            [Fact] void NoQuotesAllowUtf16Escapes() => Parser.Parse("{ \\u005Ffoo: 1, }").Should().Be(new { _foo = 1 });

            [Fact] void NoQuotesAllowUtf32Escapes() => Parser.Parse("{ \\u{5F}foo: 1, }").Should().Be(new { _foo = 1 });

            [Fact]
            void Missing() =>
                Invoking(() => Parser.Parse("{"))
                .Should().Throw<Exception>().WithMessage(
                    """
                    Error in Ln: 1 Col: 2
                    {
                     ^
                    Note: The error occurred at the end of the input stream.
                    Expecting: property or '}'
                    """);

            [Fact]
            void MustNotStartWithDigit() =>
                Invoking(() => Parser.Parse("{10twenty:1}"))
                .Should().Throw<Exception>().WithMessage(
                    """
                    Error in Ln: 1 Col: 2
                    {10twenty:1}
                     ^
                    The identifier contains an invalid character at the indicated position.
                    """);

            [Fact]
            void InvalidAscii() =>
                Invoking(() => Parser.Parse("{a?b:1}"))
                .Should().Throw<Exception>().WithMessage(
                    """
                    Error in Ln: 1 Col: 3
                    {a?b:1}
                      ^
                    The identifier contains an invalid character at the indicated position.
                    """);

            [Fact]
            void InvalidEncodedAscii() =>
                Invoking(() => Parser.Parse(@"{a\u002Fb:1}"))
                .Should().Throw<Exception>().WithMessage(
                    """
                    Error in Ln: 1 Col: 3
                    {a\u002Fb:1}
                      ^
                    The unicode escape sequence starting at the indicated position was replaced
                    with the invalid character '/'. The full identifier string after replacing all
                    escape sequences was: 'a/b'.
                    """);

            [Fact]
            void InvalidUnicodeWhitespace() =>
                Invoking(() => Parser.Parse("{a\u200Ab:1}"))
                .Should().Throw<Exception>().WithMessage(
                    """
                    Error in Ln: 1 Col: 4
                    {a b:1}
                       ^
                    Expecting: ':'
                    """);

            [Fact]
            void InvalidUnicodeSurrogatePair() =>
                Invoking(() => Parser.Parse("{a😈b:1}"))
                .Should().Throw<Exception>().WithMessage(
                    """
                    Error in Ln: 1 Col: 3
                    {a😈b:1}
                      ^
                    Note: The error occurred at the beginning of the surrogate pair '\ud83d\ude08'.
                    The identifier contains an invalid character at the indicated position.
                    """);

            [Fact]
            void InvalidEncodedUnicodeWhitespace() =>
                Invoking(() => Parser.Parse(@"{a\u200Ab:1}"))
                .Should().Throw<Exception>().WithMessage(
                    """
                    Error in Ln: 1 Col: 3
                    {a\u200Ab:1}
                      ^
                    The unicode escape sequence starting at the indicated position was replaced
                    with the invalid character ' '. The full identifier string after replacing all
                    escape sequences was: 'a b'.
                    """);

            [Fact]
            void InvalidEncodedUnicodeSurrogatePair() =>
                Invoking(() => Parser.Parse("{a\\uD83D\\uDE08b:1}"))
                .Should().Throw<Exception>().WithMessage(
                    // The invalid char is actually a '�' (the high surrogate of the surrogate pair)
                    // instead of a '?' (wildcard) but it seems like Shouldly can't handle that.
                    """
                    Error in Ln: 1 Col: 3
                    {a\uD83D\uDE08b:1}
                      ^
                    The unicode escape sequence starting at the indicated position was replaced
                    with the invalid character '?'. The full identifier string after replacing all
                    escape sequences was: 'a😈b'.
                    """);

            [Fact]
            void ErrorPositionCorrectDespiteUtf16Escapes() =>
                Invoking(() => Parser.Parse(@"{a\u0061b\u0062\u002Fc\u0063:1}"))
                .Should().Throw<Exception>().WithMessage(
                    """
                    Error in Ln: 1 Col: 16
                    {a\u0061b\u0062\u002Fc\u0063:1}
                                   ^
                    The unicode escape sequence starting at the indicated position was replaced
                    with the invalid character '/'. The full identifier string after replacing all
                    escape sequences was: 'aabb/cc'.
                    """);

            [Fact]
            void ErrorPositionCorrectDespiteUtf32Escapes() =>
                Invoking(() => Parser.Parse(@"{a\u{061}b\u{00062}\u{2F}c\u{0063}:1}"))
                .Should().Throw<Exception>().WithMessage(
                    """
                    Error in Ln: 1 Col: 20
                    {a\u{061}b\u{00062}\u{2F}c\u{0063}:1}
                                       ^
                    The unicode escape sequence starting at the indicated position was replaced
                    with the invalid character '/'. The full identifier string after replacing all
                    escape sequences was: 'aabb/cc'.
                    """);
        }
    }
}
