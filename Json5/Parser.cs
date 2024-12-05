#pragma warning disable IDE0065 // Misplaced using directive
#pragma warning disable IDE0130 // Namespace does not match folder structure

using FParsec;
using FParsec.CSharp;

using Microsoft.FSharp.Core;

using System.Text.Json.Nodes;

using static FParsec.CSharp.CharParsersCS;
using static FParsec.CSharp.PrimitivesCS;

namespace Json5.Parsing;

using NodeP = FSharpFunc<CharStream<Unit>, Reply<JsonNode?>>;

public static class Parser {
    public static JsonNode? Parse(string s) => json5.Run(s).GetResult();

    private static readonly NodeP jnull = StringCI<JsonNode?>("null", null).Lbl("null");

    private static readonly NodeP jtrue = StringCI<JsonNode?>("true", JsonValue.Create(true)).Lbl("true");

    private static readonly NodeP jfalse = StringCI<JsonNode?>("false", JsonValue.Create(false)).Lbl("false");

    private static readonly NodeP json5 = Choice(
        jnull,
        jtrue,
        jfalse);
}
