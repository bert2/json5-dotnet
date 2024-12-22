#pragma warning disable IDE0065 // Misplaced using directive

using Microsoft.FSharp.Core;

using System.Text.Json.Nodes;

namespace Json5.Internal;

using FParsec.CSharp;

using System.Text.Json.Nodes;

using static CommonParsers;
using static FParsec.CSharp.CharParsersCS;
using static FParsec.CSharp.PrimitivesCS;

using JsonNodeP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<JsonNode?>>;

public static class ArrayParser {
    public static JsonNodeP Json5Array { get; set; }

    static ArrayParser() {
        var items = Many(Rec(() => Json5Value), sep: Skip(',').AndR(WSC), canEndWithSep: true);
        Json5Array =
            Between(Skip('[').AndR(WSC), items, Skip(']'))
            .Map(xs => (JsonNode?)new JsonArray([.. xs]))
            .Lbl("array");
    }
}
