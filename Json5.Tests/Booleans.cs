#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests;

using FluentAssertions;

using Helpers;

public class Booleans {
    [Fact] void True() => Json5.Parse("true").Should().BeValue(true);
    [Fact] void False() => Json5.Parse("false").Should().BeValue(false);
}
