#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace Microsoft.Extensions.Configuration.Json5;

using System;
using System.IO;

public class Json5ConfigProvider(Json5ConfigSource source) : FileConfigurationProvider(source) {
    public override void Load(Stream stream) {
        try {
            Data = Json5ConfigFileParser.Parse(stream);
        } catch (Exception ex) {
            throw new FormatException("Could not parse the JSON5 file.", ex);
        }
    }
}
