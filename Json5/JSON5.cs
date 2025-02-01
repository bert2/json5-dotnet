namespace Json5;

using FParsec.CSharp;

using System.Text;
using System.Text.Json.Nodes;

using static Internal.Parser;

/// <summary>
/// Provides static methods to translate JSON5 to JSON or parse JSON5 as a <see cref="JsonNode"/>.
/// </summary>
public static class JSON5 {
    /// <summary>
    /// Translates a JSON5 string to JSON and then parses the result as a <see cref="JsonNode"/>.
    /// </summary>
    /// <param name="json5">The JSON5 data.</param>
    /// <param name="name">Optional name (e.g. a file path) to show in parser errors.</param>
    /// <returns>The <see cref="JsonNode"/>.</returns>
    public static JsonNode? Parse(string json5, string? name = null)
        => JsonNode.Parse(Translate(json5, name));

    /// <summary>
    /// Translates a JSON5 stream to JSON and then parses the result as a <see cref="JsonNode"/>.
    /// </summary>
    /// <param name="json5">The JSON5 data.</param>
    /// <param name="name">Optional name (e.g. a file path) to show in parser errors.</param>
    /// <param name="encoding">
    /// Optional encoding of the stream. In case no unicode byte order mark is found, the stream
    /// data is assumed to be encoded with the given encoding. <see cref="Encoding.Default"/> will
    /// be used if <paramref name="encoding"/> was not specified.
    /// </param>
    /// <returns>The <see cref="JsonNode"/>.</returns>
    public static JsonNode? Parse(Stream json5, string? name = null, Encoding? encoding = null)
        => JsonNode.Parse(Translate(json5, name, encoding));

    /// <summary>Translates a JSON5 string to JSON. Removes all whitespace during translation.</summary>
    /// <param name="json5">The JSON5 data.</param>
    /// <param name="name">Optional name (e.g. a file path) to show in parser errors.</param>
    /// <returns>The JSON string.</returns>
    public static string Translate(string json5, string? name = null)
        => Json5.RunOnString(json5, streamName: name).GetResult();

    /// <summary>Translates a JSON5 stream to JSON. Removes all whitespace during translation.</summary>
    /// <param name="json5">The JSON5 data.</param>
    /// <param name="name">Optional name (e.g. a file path) to show in parser errors.</param>
    /// <param name="encoding">
    /// Optional encoding of the stream. In case no unicode byte order mark is found, the stream
    /// data is assumed to be encoded with the given encoding. <see cref="Encoding.Default"/> will
    /// be used if <paramref name="encoding"/> was not specified.
    /// </param>
    /// <returns>The JSON string.</returns>
    public static string Translate(Stream json5, string? name = null, Encoding? encoding = null)
        => Json5.RunOnStream(json5, encoding, name).GetResult();
}
