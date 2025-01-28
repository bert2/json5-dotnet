#pragma warning disable IDE0065 // Misplaced using directive

using Microsoft.FSharp.Core;

namespace Json5.Internal;

using FParsec.CSharp;

using static Common;
using static FParsec.CSharp.CharParsersCS;
using static FParsec.CSharp.PrimitivesCS;

using StringP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<string>>;

/// <summary>Implements JSON5 array parsing.</summary>
public static class ArrayParser {
    /// <summary>
    /// <para>Parses a JSON5 array and translates it to JSON.</para>
    /// <para>
    /// JSON5 arrays are just like JSON arrays, except that trailing commas are allowed.
    /// </para>
    /// </summary>
    public static StringP Json5Array { get; set; }

    static ArrayParser() {
        var items = Many(Rec(() => Json5Value), sep: Skip(',').AndR(WSC), canEndWithSep: true);

        Json5Array =
            Between(Skip('[').AndR(WSC), items, Skip(']'))
            .Map(xs => '[' + string.Join(',', xs) + ']')
            .Lbl("array");
    }
}
