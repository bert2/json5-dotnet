#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace Microsoft.Extensions.Configuration.Json5;

/// <summary>
/// Represents a JSON5 file as an <see cref="IConfigurationSource"/>.
/// </summary>
public class Json5ConfigSource : FileConfigurationSource {
    /// <summary>
    /// Builds the <see cref="Json5ConfigProvider"/> for this source.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
    /// <returns>A <see cref="Json5ConfigProvider"/> instance.</returns>
    public override IConfigurationProvider Build(IConfigurationBuilder builder) {
        EnsureDefaults(builder);
        return new Json5ConfigProvider(this);
    }
}