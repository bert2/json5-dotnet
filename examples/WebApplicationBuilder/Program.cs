// This example demonstrates how to integrate your JSON5 appsettings
// with the default web application builder.

var builder = WebApplication.CreateBuilder(args);
// Replaces the default appsettings.json & appsettings.*.json with
// your appsettings.json5 & appsettings.*.json5.
builder.WithJson5AppSettings();
var app = builder.Build();

app.MapGet("/config", (IConfiguration cfg) => cfg.Get<Config>());

app.Run();

// appsettings.json5 has four values:
// a: will be overwritten by a command line argument (see launch settings)
// b: will be overwritten by an environment variable (see launch settings)
// c: will be overwritten by appsettings.Development.json5
// d: will not be overwritten
public record Config(string A, string B, string C, string D);
