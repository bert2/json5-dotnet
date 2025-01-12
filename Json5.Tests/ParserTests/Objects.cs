#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests.ParserTests;

using FluentAssertions;

using Helpers;

using System.Text.Json.Nodes;

using static FluentAssertions.FluentActions;

public class Objects {
    [Fact] void Empty() => Parser.Parse2("{}").Should().BeEmptyObject();

    [Fact]
    void PrimitiveProperties() =>
        Parser.Parse2("{ a: null, b: true, c: 1, d: .1, e: 1., f: 'hi' }")
        .Should().Be(new { a = (string?)null, b = true, c = 1, d = .1, e = 1.0, f = "hi" });

    [Fact]
    void ComplexProperties() =>
        Parser.Parse2("{ a: { b: [], c: { d: [] } } }")
        .Should().Be(new { a = new { b = Array.Empty<object>(), c = new { d = Array.Empty<object>() } } });

    [Fact] void TrailingCommaAllowed() => Parser.Parse2("{ a: 1, }").Should().Be(new { a = 1 });

    [Fact]
    void OnlyCommaNotAllowed() =>
        Invoking(() => Parser.Parse2("{ , }"))
        .Should().Throw<Exception>().WithMessage(
            """
            Error in Ln: 1 Col: 3
            { , }
              ^
            Expecting: property or '}'
            """);

    [Fact]
    void Whitespace() =>
        Parser.Parse2(
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
        Parser.Parse2(
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
        Parser.Parse2(
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
        Invoking(() => Parser.Parse2("{ a: 1"))
        .Should().Throw<Exception>().WithMessage(
            """
            Error in Ln: 1 Col: 7
            { a: 1
                  ^
            Note: The error occurred at the end of the input stream.
            Expecting: ',' or '}'
            """);

    public class MemberNames {
        [Fact] void SingleQuotes() => Parser.Parse2("{ 'foo': 1, }").Should().Be(new { foo = 1 });

        [Fact] void DoubleQuotes() => Parser.Parse2("{ \"foo\": 1, }").Should().Be(new { foo = 1 });

        [Fact]
        void QuotesAllowEverything() =>
            Parser.Parse2("{ 'boo! 👻': 666 }")
            .Should().Be(new JsonObject([KeyValuePair.Create("boo! 👻", (JsonNode?)666)]));

        [Fact]
        void DuplicatesOverwrite() => Parser.Parse2("{a: 1, a: 2}").Should().Be(new { a = 2 });

        [Fact]
        void DuplicatesCrashDictionaryApi() =>
            Invoking(() => Parser.Parse2("{a: 1, a: 2}")!["a"])
            .Should().Throw<Exception>().WithMessage("An item with the same key has already been added. Key: a (Parameter 'key')");

        [Fact]
        void InvalidLeadingComma() =>
            Invoking(() => Parser.Parse2("{, a: 1}"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 2
                {, a: 1}
                 ^
                Expecting: property or '}'
                """);

        public class NoQuotes {
            [Fact] void Allowed() => Parser.Parse2("{ foo: 1, }").Should().Be(new { foo = 1 });

            [Fact]
            void DollarAllowed() =>
                Parser.Parse2("{ $foo: 1, }")
                .Should().Be(new JsonObject([KeyValuePair.Create("$foo", (JsonNode?)1)]));

            [Fact] void UnderscoreAllowed() => Parser.Parse2("{ _foo: 1, }").Should().Be(new { _foo = 1 });

            [Fact]
            void UmlautAllowed() =>
                Parser.Parse2("{ ümlåût: 'ümlaüt is löve, ümlaüt is lïfe' }")
                .Should().Be(new { ümlåût = "ümlaüt is löve, ümlaüt is lïfe" });

            [Fact] void NoQuotesAllowUtf16Escapes() => Parser.Parse2("{ \\u005Ffoo: 1, }").Should().Be(new { _foo = 1 });

            [Fact] void NoQuotesAllowUtf32Escapes() => Parser.Parse2("{ \\u{5F}foo: 1, }").Should().Be(new { _foo = 1 });

            [Fact]
            void Missing() =>
                Invoking(() => Parser.Parse2("{"))
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
                Invoking(() => Parser.Parse2("{10twenty:1}"))
                .Should().Throw<Exception>().WithMessage(
                    """
                    Error in Ln: 1 Col: 2
                    {10twenty:1}
                     ^
                    The identifier contains an invalid character at the indicated position.
                    """);

            [Fact]
            void InvalidAscii() =>
                Invoking(() => Parser.Parse2("{a?b:1}"))
                .Should().Throw<Exception>().WithMessage(
                    """
                    Error in Ln: 1 Col: 3
                    {a?b:1}
                      ^
                    The identifier contains an invalid character at the indicated position.
                    """);

            [Fact]
            void InvalidEncodedAscii() =>
                Invoking(() => Parser.Parse2(@"{a\u002Fb:1}"))
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
                Invoking(() => Parser.Parse2("{a\u200Ab:1}"))
                .Should().Throw<Exception>().WithMessage(
                    """
                    Error in Ln: 1 Col: 4
                    {a b:1}
                       ^
                    Expecting: ':'
                    """);

            [Fact]
            void InvalidUnicodeSurrogatePair() =>
                Invoking(() => Parser.Parse2("{a😈b:1}"))
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
                Invoking(() => Parser.Parse2(@"{a\u200Ab:1}"))
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
                Invoking(() => Parser.Parse2("{a\\uD83D\\uDE08b:1}"))
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
                Invoking(() => Parser.Parse2(@"{a\u0061b\u0062\u002Fc\u0063:1}"))
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
                Invoking(() => Parser.Parse2(@"{a\u{061}b\u{00062}\u{2F}c\u{0063}:1}"))
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
