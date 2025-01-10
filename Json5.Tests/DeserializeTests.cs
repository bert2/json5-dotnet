namespace Json5.Tests;

using FluentAssertions;

using System;
using System.Numerics;
using System.Text.Json;

public class DeserializeTests {
    [Fact]
    public void BigInteger() =>
        Parser.Parse("340282366920938463463374607431768211456")
        .Deserialize<BigInteger>()
        .Should().Be((BigInteger)UInt128.MaxValue + 1);
}
