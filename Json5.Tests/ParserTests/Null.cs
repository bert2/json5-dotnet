namespace Json5.Tests.ParserTests;

using FluentAssertions;

public class Null {
    [Fact] public void Test() => Parser.Parse("null").Should().BeNull();
}
