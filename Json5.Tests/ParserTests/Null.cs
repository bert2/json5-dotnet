#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests.ParserTests;

using FluentAssertions;

public class Null {
    [Fact] void Test() => Parser.Parse("null").Should().BeNull();
}
