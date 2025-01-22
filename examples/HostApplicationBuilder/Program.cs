using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// This example demonstrates how to integrate your JSON5 appsettings
// with the default application builder.

var builder = Host.CreateApplicationBuilder(args);
// Replaces the default appsettings.json & appsettings.*.json with
// your appsettings.json5 & appsettings.*.json5.
builder.WithJson5AppSettings();
using var host = builder.Build();

var config = host.Services.GetRequiredService<IConfiguration>();

Console.WriteLine(config.Get<Config>());

// appsettings.json5 has four values:
// a: will be overwritten by a command line argument (see launch settings)
// b: will be overwritten by an environment variable (see launch settings)
// c: will be overwritten by appsettings.Production.json5
// d: will not be overwritten
public record Config(string A, string B, string C, string D);
