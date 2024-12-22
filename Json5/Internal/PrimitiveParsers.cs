#pragma warning disable IDE0065 // Misplaced using directive

using Microsoft.FSharp.Core;

using System.Text.Json.Nodes;

namespace Json5.Internal;

using FParsec.CSharp;

using System.Text.Json.Nodes;

using static FParsec.CSharp.CharParsersCS;
using static FParsec.CSharp.PrimitivesCS;

using JsonNodeP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<JsonNode?>>;

public static class PrimitiveParsers {
    public static JsonNodeP Json5Null { get; set; } = StringP<JsonNode?>("null", null).Lbl("null");

    public static JsonNodeP Json5Bool { get; set; } =
        Choice(
            StringP("true", true),
            StringP("false", false))
        .Map(x => (JsonNode?)x) // deferred conversion ensures a new JsonNode is created every time
        .Lbl("bool");
}
