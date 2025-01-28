namespace Json5.Tests.Parse;

using FluentAssertions;

using static Json5.Json5Parser;

public class Null {
    [Fact] public void Test() => Parse("null").Should().BeNull();
}
