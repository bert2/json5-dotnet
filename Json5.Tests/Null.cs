#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests;

using FluentAssertions;

using Helpers;

public class Null {
    [Fact] void Test() => Json5.Parse("null").Should().BeNull();
}
