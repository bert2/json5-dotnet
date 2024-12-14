namespace Json5;

using FParsec.CSharp;

using Parsing;

using System.Text.Json.Nodes;

public static class Json5 {
    public static JsonNode? Parse(string json) => Json5Parser.Parse(json).GetResult();
}
