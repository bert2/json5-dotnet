namespace Json5;

using FParsec.CSharp;

using System.Text.Json.Nodes;

using static Internal.Json5Parser;

public static class Parser {
    public static JsonNode? Parse(string json5, string? name = null)
        => JsonNode.Parse(Translate(json5, name));

    public static JsonNode? Parse(Stream json5, string? name = null)
        => JsonNode.Parse(Translate(json5, name));

    public static string Translate(string json5, string? name = null)
        => Json5.RunOnString(json5, streamName: name).GetResult();

    public static string Translate(Stream json5, string? name = null)
        => Json5.RunOnStream(json5, streamName: name).GetResult();
}
