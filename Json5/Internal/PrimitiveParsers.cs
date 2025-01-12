#pragma warning disable IDE0065 // Misplaced using directive

using Microsoft.FSharp.Core;

namespace Json5.Internal;

using FParsec.CSharp;

using static FParsec.CSharp.CharParsersCS;
using static FParsec.CSharp.PrimitivesCS;

using StringP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<string>>;

public static class PrimitiveParsers {
    public static StringP Json5Null { get; set; } = StringP("null").Lbl("null");

    public static StringP Json5Bool { get; set; } = Choice(StringP("true"), StringP("false")).Lbl("bool");
}
