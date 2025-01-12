#pragma warning disable IDE0065 // Misplaced using directive

using Microsoft.FSharp.Core;

using System.Text.Json.Nodes;

namespace Json5.Internal;

using FParsec.CSharp;

using static CommonParsers;
using static FParsec.CSharp.CharParsersCS;

using JsonNodeP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<JsonNode?>>;
using StringP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<string>>;

public static class Json5Parser {
    public static JsonNodeP Json5 { get; set; } = WSC.And(Json5Value).And(EOF);
    public static StringP Json5S { get; set; } = WSC.And(Json5ValueS).And(EOF);
}
