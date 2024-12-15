#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests;

public class Whitespace {
    [Fact] void Leading() => Json5.Parse("  \n  \t  42").Should().BeValue(42);
    [Fact] void Trailing() => Json5.Parse("3.5  \n  \t  ").Should().BeValue(3.5);
}
