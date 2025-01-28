#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace Microsoft.Extensions.Configuration.Json5;

/// <summary>Represents a JSON5 stream as an <see cref="IConfigurationSource"/>.</summary>
public class Json5StreamConfigSource : StreamConfigurationSource {
    /// <summary>Builds the <see cref="Json5StreamConfigProvider"/> for this source.</summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
    /// <returns>An <see cref="Json5StreamConfigProvider"/></returns>
    public override IConfigurationProvider Build(IConfigurationBuilder builder)
        => new Json5StreamConfigProvider(this);
}