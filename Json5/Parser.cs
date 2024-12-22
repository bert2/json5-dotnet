namespace Json5;

using FParsec.CSharp;

using System.Text.Json.Nodes;

using static Internal.Json5Parser;

public static class Parser {
    public static JsonNode? Parse(string json) => Json5.Run(json).GetResult();
}
