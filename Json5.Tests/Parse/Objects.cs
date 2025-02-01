#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests.Parse;

using FluentAssertions;

using Helpers;

using System.Text.Json.Nodes;

using static FluentAssertions.FluentActions;
using static Json5.JSON5;

public class Objects {
    [Fact] void Empty() => Parse("{}").Should().BeEmptyObject();

    [Fact]
    void PrimitiveProperties() =>
        Parse("{ a: null, b: true, c: 1, d: .1, e: 1., f: 'hi' }")
        .Should().Be(new { a = (string?)null, b = true, c = 1, d = .1, e = 1.0, f = "hi" });

    [Fact]
    void ComplexProperties() =>
        Parse("{ a: { b: [], c: { d: [] } } }")
        .Should().Be(new { a = new { b = Array.Empty<object>(), c = new { d = Array.Empty<object>() } } });

    [Fact] void TrailingCommaAllowed() => Parse("{ a: 1, }").Should().Be(new { a = 1 });

    [Fact]
    void OnlyCommaNotAllowed() =>
        Invoking(() => Parse("{ , }"))
        .Should().Throw<Exception>().WithMessage(
            """
            Error in Ln: 1 Col: 3
            { , }
              ^
            Expecting: property or '}'
            """);

    [Fact]
    void Whitespace() =>
        Parse(
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
        Parse(
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
        Parse(
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
        Invoking(() => Parse("{ a: 1"))
        .Should().Throw<Exception>().WithMessage(
            """
            Error in Ln: 1 Col: 7
            { a: 1
                  ^
            Note: The error occurred at the end of the input stream.
            Expecting: ',' or '}'
            """);

    public class MemberNames {
        [Fact] void SingleQuotes() => Parse("{ 'foo': 1, }").Should().Be(new { foo = 1 });

        [Fact] void DoubleQuotes() => Parse("{ \"foo\": 1, }").Should().Be(new { foo = 1 });

        [Fact]
        void QuotesAllowUnicode() =>
            Parse("{ 'boo! 👻': 666 }")
            .Should().Be(new JsonObject([KeyValuePair.Create("boo! 👻", (JsonNode?)666)]));

        [Fact]
        void QuotesAllowAllCodePoints() => Enumerable
            .Range(0, 0x10FFFF + 1)
            .Where(i => i > 0xFFFF || !char.IsSurrogate((char)i))
            .ForEach(i =>
                Parse($@"{{ '\u{{{i:X6}}}': 1 }}")
                .Should().Be(new JsonObject([KeyValuePair.Create(char.ConvertFromUtf32(i), (JsonNode?)1)])));

        [Fact]
        void DuplicatesOverwrite() => Parse("{a: 1, a: 2}").Should().Be(new { a = 2 });

        [Fact]
        void DuplicatesCrashDictionaryApi() =>
            Invoking(() => Parse("{a: 1, a: 2}")!["a"])
            .Should().Throw<Exception>().WithMessage("An item with the same key has already been added. Key: a (Parameter 'key')");

        [Fact]
        void InvalidLeadingComma() =>
            Invoking(() => Parse("{, a: 1}"))
            .Should().Throw<Exception>().WithMessage(
                """
                Error in Ln: 1 Col: 2
                {, a: 1}
                 ^
                Expecting: property or '}'
                """);

        public class NoQuotes {
            [Fact] void Allowed() => Parse("{ foo: 1, }").Should().Be(new { foo = 1 });

            [Fact]
            void DollarAllowed() =>
                Parse("{ $foo: 1, }")
                .Should().Be(new JsonObject([KeyValuePair.Create("$foo", (JsonNode?)1)]));

            [Fact] void UnderscoreAllowed() => Parse("{ _foo: 1, }").Should().Be(new { _foo = 1 });

            [Fact]
            void UmlautAllowed() =>
                Parse("{ ümlåût: 'ümlaüt is löve, ümlaüt is lïfe' }")
                .Should().Be(new { ümlåût = "ümlaüt is löve, ümlaüt is lïfe" });

            [Fact] void UnescapedUtf32Allowed() => Parse("{ 𐊅: 1 }").Should().Be(new JsonObject([KeyValuePair.Create("𐊅", (JsonNode?)1)]));

            [Fact] void NoQuotesAllowUtf16Escapes() => Parse("{ \\u005Ffoo: 1, }").Should().Be(new { _foo = 1 });

            [Fact] void NoQuotesAllowUtf32Escapes() => Parse("{ \\u{5F}foo: 1, }").Should().Be(new { _foo = 1 });

            [Fact]
            void Missing() =>
                Invoking(() => Parse("{"))
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
                Invoking(() => Parse("{10twenty:1}"))
                .Should().Throw<Exception>().WithMessage(
                    """
                    Error in Ln: 1 Col: 2
                    {10twenty:1}
                     ^
                    The identifier contains an invalid character at the indicated position.
                    """);

            [Fact]
            void InvalidAscii() =>
                Invoking(() => Parse("{a?b:1}"))
                .Should().Throw<Exception>().WithMessage(
                    """
                    Error in Ln: 1 Col: 3
                    {a?b:1}
                      ^
                    The identifier contains an invalid character at the indicated position.
                    """);

            [Fact]
            void InvalidEncodedAscii() =>
                Invoking(() => Parse(@"{a\u002Fb:1}"))
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
                Invoking(() => Parse("{a\u200Ab:1}"))
                .Should().Throw<Exception>().WithMessage(
                    """
                    Error in Ln: 1 Col: 4
                    {a b:1}
                       ^
                    Expecting: ':'
                    """);

            [Fact]
            void InvalidUnicodeSurrogatePair() =>
                Invoking(() => Parse("{a😈b:1}"))
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
                Invoking(() => Parse(@"{a\u200Ab:1}"))
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
                Invoking(() => Parse("{a\\uD83D\\uDE08b:1}"))
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
                Invoking(() => Parse(@"{a\u0061b\u0062\u002Fc\u0063:1}"))
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
                Invoking(() => Parse(@"{a\u{061}b\u{00062}\u{2F}c\u{0063}:1}"))
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
