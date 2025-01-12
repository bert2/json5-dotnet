﻿namespace Json5;

using FParsec.CSharp;

using System.Text.Json.Nodes;

using static Internal.Json5Parser;

public static class Parser {
    public static JsonNode? Parse(string json5) => Json5.RunOnString(json5).GetResult();

    public static JsonNode? Parse(Stream json5) => Json5.RunOnStream(json5).GetResult();

    public static string Translate(string json5) => Json5S.RunOnString(json5).GetResult();

    public static JsonNode? Parse2(string json5) => JsonNode.Parse(Translate(json5));
}
