#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace Microsoft.Extensions.Configuration;

using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.Json5;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

using System;
using System.IO;

public static class Json5ConfigExtensions {
    /// <summary>
    /// Adds the JSON5 configuration provider at <paramref name="path"/> to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="path">Path relative to the base path stored in
    /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
    /// <param name="optional">Whether the file is optional.</param>
    /// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddJson5File(
        this IConfigurationBuilder builder,
        string path,
        bool optional = false,
        bool reloadOnChange = false)
        => AddJson5File(builder, provider: null, path, optional, reloadOnChange);

    /// <summary>
    /// Adds a JSON5 configuration source to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="provider">The <see cref="IFileProvider"/> to use to access the file.</param>
    /// <param name="path">Path relative to the base path stored in
    /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
    /// <param name="optional">Whether the file is optional.</param>
    /// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddJson5File(
        this IConfigurationBuilder builder,
        IFileProvider? provider,
        string path,
        bool optional = false,
        bool reloadOnChange = false) {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return builder.AddJson5File(s => {
            s.FileProvider = provider;
            s.Path = path;
            s.Optional = optional;
            s.ReloadOnChange = reloadOnChange;
            s.ResolveFileProvider();
        });
    }

    /// <summary>
    /// Adds a JSON5 configuration source to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="configureSource">Configures the source.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddJson5File(
        this IConfigurationBuilder builder,
        Action<Json5ConfigSource>? configureSource)
         => builder.Add(configureSource);

    /// <summary>
    /// Adds a JSON5 configuration source to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="stream">The <see cref="Stream"/> to read the json configuration data from.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddJson5Stream(this IConfigurationBuilder builder, Stream stream) {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Add<Json5StreamConfigSource>(s => s.Stream = stream);
    }

    /// <summary>
    /// Replaces the default JSON config sources for 'appsettings.json' and
    /// 'appsettings.[<see cref="IHostEnvironment.EnvironmentName"/>].json' with
    /// JSON5 config sources of the same name (but with the '.json5' extension).
    /// <para>
    /// This method should only be used together with 'Host.CreateApplicationBuilder()' or
    /// 'WebApplication.CreateBuilder()'.
    /// </para>
    /// </summary>
    /// <param name="builder">The default application builder.</param>
    /// <exception cref="InvalidOperationException"></exception>
    public static void WithJson5AppSettings(this IHostApplicationBuilder builder) {
        var jsonSources = builder.Configuration.Sources
            .Index().Where(x => x.Item is JsonConfigurationSource).ToArray();

        if (jsonSources.Length != 2
            || !IsMainAppSettings(jsonSources[0].Item)
            || !IsEnvAppSettings(jsonSources[1].Item)) {
            throw new InvalidOperationException(
                $"The ConfigurationManager was initialized in a non-default way. '{nameof(WithJson5AppSettings)}()' should only be used together with 'Host.CreateApplicationBuilder()' or 'WebApplication.CreateBuilder()'.");
        }

        builder.Configuration.Sources[jsonSources[0].Index] = new Json5ConfigSource {
            Path = "appsettings.json5",
            Optional = true
        };

        builder.Configuration.Sources[jsonSources[1].Index] = new Json5ConfigSource {
            Path = $"appsettings.{builder.Environment.EnvironmentName}.json5",
            Optional = true
        };

        static bool IsMainAppSettings(IConfigurationSource s)
            => ((JsonConfigurationSource)s).Path == "appsettings.json";

        bool IsEnvAppSettings(IConfigurationSource s)
            => ((JsonConfigurationSource)s).Path == $"appsettings.{builder.Environment.EnvironmentName}.json";
    }
}
