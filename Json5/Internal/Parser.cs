#pragma warning disable IDE0065 // Misplaced using directive

using Microsoft.FSharp.Core;

namespace Json5.Internal;

using FParsec.CSharp;

using static CommonParsers;
using static FParsec.CSharp.CharParsersCS;

using StringP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<string>>;

public static class Parser {
    public static StringP Json5 { get; set; } = WSC.And(Json5Value).And(EOF);
}
