namespace Json5.Tests.IntegrationTests;

using FluentAssertions;

using Json5.Tests.Helpers;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.Json5;

public class WebApplicationBuilderTests {
    [Fact]
    public void ReplacesJsonWithJson5() {
        var builder = WebApplication.CreateBuilder();

        builder.WithJson5AppSettings();

        builder.Configuration.Sources.OfType<JsonConfigurationSource>().Should().BeEmpty();
        builder.Configuration.Sources.OfType<Json5ConfigSource>().Should().SatisfyRespectively(
            s => s.Path.Should().Be("appsettings.json5"),
            s => s.Path.Should().Be("appsettings.Production.json5"));
    }

    [Fact]
    public void PreservesOrderOfSourcesWithHigherPriority() {
        var builder = WebApplication.CreateBuilder(["--arg", "value"]);

        builder.WithJson5AppSettings();

        builder.Configuration.Sources.TakeLast(5).Should().SatisfyRespectively(
            s => s.Should().BeOfType<Json5ConfigSource>(),
            s => s.Should().BeOfType<Json5ConfigSource>(),
            s => s.Should().BeOfType<EnvironmentVariablesConfigurationSource>(),
            s => s.Should().BeOfType<CommandLineConfigurationSource>(),
            s => s.Should().BeOfType<ChainedConfigurationSource>());
    }

    [Fact]
    public void AddsEnvironmentSpecificJson5() {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Test" });

        builder.WithJson5AppSettings();

        builder.Configuration.Sources.OfType<Json5ConfigSource>().Should().SatisfyRespectively(
            s => s.Path.Should().Be("appsettings.json5"),
            s => s.Path.Should().Be("appsettings.Test.json5"));
    }

    [Fact]
    public void ThrowsWhenUsedWithEmptyBuilder() {
        var builder = WebApplication.CreateEmptyBuilder(new());

        var act = () => builder.WithJson5AppSettings();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The ConfigurationManager was initialized in a non-default way*");
    }

    [Fact]
    public void ThrowsWhenUsedWithCustomBuilder() {
        var builder = WebApplication.CreateEmptyBuilder(new());
        builder.Configuration.AddJsonFile("config.json", optional: true);
        builder.Configuration.AddJsonFile("dev-config.json", optional: true);

        var act = () => builder.WithJson5AppSettings();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The ConfigurationManager was initialized in a non-default way*");
    }
}
