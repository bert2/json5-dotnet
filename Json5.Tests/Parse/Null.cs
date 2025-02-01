namespace Json5.Tests.Parse;

using FluentAssertions;

using static Json5.JSON5;

public class Null {
    [Fact] public void Test() => Parse("null").Should().BeNull();
}
