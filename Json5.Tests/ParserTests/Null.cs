namespace Json5.Tests.ParserTests;

using FluentAssertions;

using static Json5.Json5Parser;

public class Null {
    [Fact] public void Test() => Parse("null").Should().BeNull();
}
