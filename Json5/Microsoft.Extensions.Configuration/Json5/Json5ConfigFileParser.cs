#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace Microsoft.Extensions.Configuration.Json5;

using global::Json5;

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

public static class Json5ConfigFileParser {
    public static Dictionary<string, string?> Parse(Stream input, string? path = null) {
        var json5 = Json5Parser.Parse(input, path);
        return json5?.GetValueKind() == JsonValueKind.Object
            ? new(json5.AsObject().ToConfigValues(path: ""), StringComparer.OrdinalIgnoreCase)
            : throw new FormatException($"Top-level JSON5 element must be an object. Instead, {json5.GetKindString()} was found.");
    }

    private static IEnumerable<KeyValuePair<string, string?>> ToConfigValues(this JsonObject obj, string path)
        => obj.SelectMany(x => x.Value.ToConfigValues(Concat(path, x.Key)));

    private static IEnumerable<KeyValuePair<string, string?>> ToConfigValues(this JsonArray arr, string path)
        => arr.SelectMany((x, i) => x.ToConfigValues(Concat(path, i.ToString())));

    private static IEnumerable<KeyValuePair<string, string?>> ToConfigValues(this JsonNode? node, string path)
        => node?.GetValueKind() switch {
            JsonValueKind.Object => node.AsObject().ToConfigValues(path).DefaultIfEmpty(Pair(path, null)),
            JsonValueKind.Array => node.AsArray().ToConfigValues(path).DefaultIfEmpty(Pair(path, null)),
            _ => [Pair(path, node?.ToString())]
        };

    private static string Concat(string path, string key) => path.Length > 0
        ? path + ConfigurationPath.KeyDelimiter + key
        : key;

    private static KeyValuePair<string, string?> Pair(string path, string? value)
        => KeyValuePair.Create(path, value);

    private static string GetKindString(this JsonNode? node) => node?.GetValueKind().ToString() ?? "null";
}
