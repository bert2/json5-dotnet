namespace Json5;

using FParsec.CSharp;

using System.Text.Json.Nodes;

using static Internal.Json5Parser;

public static class Parser {
    public static JsonNode? Parse(string json5) => Json5.Run(json5).GetResult();
}
