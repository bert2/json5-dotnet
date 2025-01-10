#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace Microsoft.Extensions.Configuration.Json5;

/// <summary>
/// Provides configuration key-value pairs that are obtained from a JSON5 stream.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Json5StreamConfigProvider"/> class.
/// </remarks>
/// <param name="source">The <see cref="Json5StreamConfigSource"/>.</param>
public class Json5StreamConfigProvider(Json5StreamConfigSource source) : StreamConfigurationProvider(source) {

    /// <summary>
    /// Loads JSON5 configuration key-value pairs from a stream into a provider.
    /// </summary>
    /// <param name="stream">The JSON5 <see cref="Stream"/> to load configuration data from.</param>
    public override void Load(Stream stream) {
        Data = Json5ConfigFileParser.Parse(stream);
    }
}
