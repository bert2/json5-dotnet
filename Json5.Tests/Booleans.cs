#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests;

using FluentAssertions;

using Helpers;

public class Booleans {
    [Fact] void True() => Parser.Parse("true").Should().BeValue(true);
    [Fact] void False() => Parser.Parse("false").Should().BeValue(false);
}
