#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests.ParserTests;

using FluentAssertions;

using Helpers;

using static Json5.Json5Parser;

public class Booleans {
    [Fact] void True() => Parse("true").Should().Be(true);
    [Fact] void False() => Parse("false").Should().Be(false);
}
