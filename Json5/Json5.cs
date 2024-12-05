namespace Json5;

using Parsing;

using System.Text.Json.Nodes;

public static class Json5 {
    public static JsonNode? Parse(string json) => Parser.Parse(json);
}
