﻿#pragma warning disable IDE0065 // Misplaced using directive

using Microsoft.FSharp.Core;

namespace Json5.Internal;

using FParsec.CSharp;

using static CommonParsers;
using static FParsec.CSharp.CharParsersCS;
using static FParsec.CSharp.PrimitivesCS;

using StringP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<string>>;

public static class ArrayParser {
    public static StringP Json5Array { get; set; }

    static ArrayParser() {
        var items = Many(Rec(() => Json5Value), sep: Skip(',').AndR(WSC), canEndWithSep: true);

        Json5Array =
            Between(Skip('[').AndR(WSC), items, Skip(']'))
            .Map(xs => '[' + string.Join(',', xs) + ']')
            .Lbl("array");
    }
}
