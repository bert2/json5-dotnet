#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests.ParserTests;

using FluentAssertions;

using Helpers;

public class Null {
    [Fact] void Test() => Parser.Parse2("null").Should().BeNull();
}
