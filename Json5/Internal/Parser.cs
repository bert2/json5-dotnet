#pragma warning disable IDE0065 // Misplaced using directive

using Microsoft.FSharp.Core;

namespace Json5.Internal;

using FParsec.CSharp;

using static Common;
using static FParsec.CSharp.CharParsersCS;

using StringP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<string>>;

/// <summary>Implements a JSON5 parser.</summary>
public static class Parser {
    /// <summary>
    /// <para>The main entry point of the JSON5 parser.</para>
    /// <para>
    /// The parser outputs a JSON string equivalent of the JSON5 input string,
    /// but without any whitespace.
    /// </para>
    /// </summary>
    public static StringP Json5 { get; set; } = WSC.And(Json5Value).And(EOF);
}
