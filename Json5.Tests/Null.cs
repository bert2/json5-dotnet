namespace Json5.Tests;

using FluentAssertions;

public class Null {
    [Fact] public void LowerCase() => Json5.Parse("null").Should().BeNull();
    [Fact] public void UpperCase() => Json5.Parse("NULL").Should().BeNull();
    [Fact] public void MixedCase() => Json5.Parse("nULl").Should().BeNull();
}
