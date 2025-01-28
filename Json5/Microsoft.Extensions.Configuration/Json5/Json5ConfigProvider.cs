#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace Microsoft.Extensions.Configuration.Json5;

using System;
using System.IO;

/// <summary>Provides configuration key-value pairs that are obtained from a JSON5 file.</summary>
/// <remarks>Initializes a new instance of the <see cref="Json5ConfigProvider"/> class.</remarks>
/// <param name="source">The source settings.</param>
public class Json5ConfigProvider(Json5ConfigSource source) : FileConfigurationProvider(source) {
    /// <summary>Loads the JSON5 data from a stream.</summary>
    /// <param name="stream">The stream to read.</param>
    public override void Load(Stream stream) {
        try {
            Data = Json5ConfigFileParser.Parse(stream, Source.Path);
        } catch (Exception ex) {
            throw new FormatException("Could not parse the JSON5 file.", ex);
        }
    }
}
