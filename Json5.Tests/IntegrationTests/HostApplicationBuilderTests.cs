namespace Json5.Tests.IntegrationTests;

using FluentAssertions;

using Json5.Tests.Helpers;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.Json5;
using Microsoft.Extensions.Hosting;

public class HostApplicationBuilderTests {
    [Fact]
    public void ReplacesJsonWithJson5() {
        var builder = Host.CreateApplicationBuilder();

        builder.WithJson5AppSettings();

        builder.Configuration.Sources.OfType<JsonConfigurationSource>().Should().BeEmpty();
        builder.Configuration.Sources.OfType<Json5ConfigSource>().Should().SatisfyRespectively(
            s => s.Path.Should().Be("appsettings.json5"),
            s => s.Path.Should().Be("appsettings.Production.json5"));
    }

    [Fact]
    public void PreservesOrderOfSourcesWithHigherPriority() {
        var builder = Host.CreateApplicationBuilder(["--arg", "value"]);

        builder.WithJson5AppSettings();

        builder.Configuration.Sources.TakeLast(4).Should().SatisfyRespectively(
            s => s.Should().BeOfType<Json5ConfigSource>(),
            s => s.Should().BeOfType<Json5ConfigSource>(),
            s => s.Should().BeOfType<EnvironmentVariablesConfigurationSource>(),
            s => s.Should().BeOfType<CommandLineConfigurationSource>());
    }

    [Fact]
    public void AddsEnvironmentSpecificJson5() {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { EnvironmentName = "Test" });

        builder.WithJson5AppSettings();

        builder.Configuration.Sources.OfType<Json5ConfigSource>().Should().SatisfyRespectively(
            s => s.Path.Should().Be("appsettings.json5"),
            s => s.Path.Should().Be("appsettings.Test.json5"));
    }

    [Fact]
    public void ThrowsWhenUsedWithEmptyBuilder() {
        var builder = Host.CreateEmptyApplicationBuilder(new());

        var act = () => builder.WithJson5AppSettings();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The ConfigurationManager was initialized in a non-default way*");
    }

    [Fact]
    public void ThrowsWhenUsedWithCustomBuilder() {
        var builder = Host.CreateEmptyApplicationBuilder(new());
        builder.Configuration.AddJsonFile("config.json", optional: true);
        builder.Configuration.AddJsonFile("dev-config.json", optional: true);

        var act = () => builder.WithJson5AppSettings();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The ConfigurationManager was initialized in a non-default way*");
    }
}
