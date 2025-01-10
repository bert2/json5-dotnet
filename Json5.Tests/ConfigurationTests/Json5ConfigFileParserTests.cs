#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests.ConfigurationTests;

using FluentAssertions;

using System.Text;

using static FluentAssertions.FluentActions;
using static Microsoft.Extensions.Configuration.Json5.Json5ConfigFileParser;

public class Json5ConfigFileParserTests {
    [Fact]
    void Primitives() =>
        Parse(S(
            """
            {
              a: null,
              b: true,
              c: false,
              d: 'test',
              e: 123,
              f: +Infinity,
              g: -∞,
              h: Inf,
              i: NaN,
              j: 0xBEEF,
              k: 0b101
            }
            """))
        .Should().Equal(
            P("a", null),
            P("b", "True"),
            P("c", "False"),
            P("d", "test"),
            P("e", "123"),
            P("f", "Infinity"),
            P("g", "-Infinity"),
            P("h", "Infinity"),
            P("i", "NaN"),
            P("j", "48879"),
            P("k", "5"));

    [Fact] void NestedObjects() => Parse(S("{ a: { b: { c: 'test' } } }")).Should().Equal([P("a:b:c", "test")]);

    [Fact] void Array() => Parse(S("{ a: ['foo', 'bar'] }")).Should().Equal(P("a:0", "foo"), P("a:1", "bar"));

    [Fact] void EmptyArray() => Parse(S("{ a: [] }")).Should().Equal([P("a", null)]);

    [Fact] void EmptyObject() => Parse(S("{ a: {} }")).Should().Equal([P("a", null)]);

    [Fact] void EmptyRoot() => Parse(S("{}")).Should().Equal([]);

    [Fact]
    void InvalidRoot() =>
        Invoking(() => Parse(S("[]")))
        .Should().Throw<Exception>().WithMessage("Top-level JSON5 element must be an object. Instead, Array was found.");

    [Fact]
    void InvalidRootNull() =>
        Invoking(() => Parse(S("null")))
        .Should().Throw<Exception>().WithMessage("Top-level JSON5 element must be an object. Instead, null was found.");

    static MemoryStream S(string s) => new(Encoding.Default.GetBytes(s));

    static KeyValuePair<string, string?> P(string k, string? v) => KeyValuePair.Create(k, v);
}
