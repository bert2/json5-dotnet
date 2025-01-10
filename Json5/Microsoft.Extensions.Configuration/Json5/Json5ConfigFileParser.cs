#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace Microsoft.Extensions.Configuration.Json5;

using global::Json5;

using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;

public static class Json5ConfigFileParser {
    public static readonly CultureInfo invCult = CultureInfo.InvariantCulture;

    public static Dictionary<string, string?> Parse(Stream input) {
        var json5 = Parser.Parse(input);
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
            // Annoying special case for BigIntegers, because they are not created as JSON value primitives
            JsonValueKind.Object when node is not JsonObject => [Pair(path, node.GetValue<BigInteger>().ToString(invCult))],
            JsonValueKind.Object => node.AsObject().ToConfigValues(path).DefaultIfEmpty(Pair(path, null)),
            JsonValueKind.Array => node.AsArray().ToConfigValues(path).DefaultIfEmpty(Pair(path, null)),
            JsonValueKind.Null or null => [Pair(path, null)],
            JsonValueKind.True => [Pair(path, true.ToString(invCult))],
            JsonValueKind.False => [Pair(path, false.ToString(invCult))],
            // We write doubles ourselves because JsonNode.ToString() can't write double.NaN/PositiveInfinity/NegativeInfinity
            // and JsonNode.ToJsonString() would write them with quotes.
            JsonValueKind.Number when node.IsDouble(out var v) => [Pair(path, v.ToString(invCult))],
            JsonValueKind.Number or JsonValueKind.String => [Pair(path, node.ToString())],
            _ => throw new FormatException($"Unsupported JSON5 {node.GetKindString()} value was found.")
        };

    private static string Concat(string path, string key) => path.Length > 0
        ? path + ConfigurationPath.KeyDelimiter + key
        : key;

    private static bool IsDouble(this JsonNode node, out double d) => node.AsValue().TryGetValue(out d);

    private static KeyValuePair<string, string?> Pair(string path, string? value)
        => KeyValuePair.Create(path, value);

    private static string GetKindString(this JsonNode? node) => node?.GetValueKind().ToString() ?? "null";
}
