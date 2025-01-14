namespace Json5.Tests.ConfigurationTests;

using FluentAssertions;

using Microsoft.Extensions.Configuration;

using System.Numerics;
using System.Text;

public class ConfigurationBuilderTests {
    [Fact]
    public void Example() {
        var cfg = new ConfigurationBuilder()
            .AddJson5Stream(S(
                """
                {
                  a: null,
                  b: 'test',
                  c: [ +Infinity, -Inf, NaN ],
                  d: [
                    { a: true, b: .1 },
                    { a: false, b: 1.e-3 }
                  ],
                  e: 0xBEEF,
                  f: 0b101
                }
                """))
            .Build()
            .Get<Config>();

        cfg.Should().NotBeNull();
        cfg!.A.Should().BeNull();
        cfg!.B.Should().Be("test");
        cfg!.C.Should().Equal(double.PositiveInfinity, double.NegativeInfinity, double.NaN);
        cfg!.D.Should().SatisfyRespectively(
            d1 => { d1.A.Should().BeTrue(); d1.B.Should().Be(0.1f); },
            d2 => { d2.A.Should().BeFalse(); d2.B.Should().Be(1.0e-3f); });
        cfg!.E.Should().Be(48879);
        cfg!.F.Should().Be(5);
    }

    static MemoryStream S(string s) => new(Encoding.Default.GetBytes(s));

    public class Config {
        public string? A { get; set; }
        public required string B { get; set; }
        public required double[] C { get; set; }
        public required SubConfig[] D { get; set; }
        public int E { get; set; }
        public int F { get; set; }
        public BigInteger G { get; set; }

        public class SubConfig {
            public bool A { get; set; }
            public float B { get; set; }
        }
    }
}
