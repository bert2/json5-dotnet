namespace Json5;

using FParsec.CSharp;

using System.Text.Json.Nodes;

using static Internal.Json5Parser;

public static class Parser {
    public static JsonNode? Parse(string json5) => JsonNode.Parse(Translate(json5));

    public static JsonNode? Parse(Stream json5) => JsonNode.Parse(Translate(json5));

    public static string Translate(string json5) => Json5.RunOnString(json5).GetResult();

    public static string Translate(Stream json5) => Json5.RunOnStream(json5).GetResult();

}
