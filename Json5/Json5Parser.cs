#pragma warning disable IDE0065 // Misplaced using directive

using Microsoft.FSharp.Core;

using System.Text.Json.Nodes;

namespace Json5.Parsing;

using FParsec.CSharp;

using static FParsec.CSharp.CharParsersCS;
using static FParsec.CSharp.PrimitivesCS;

using Chars = FParsec.CharStream<Unit>;
using UnitR = FParsec.Reply<Unit>;
using CharR = FParsec.Reply<char>;
using StringR = FParsec.Reply<string>;
using UnitP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<Unit>>;
using CharP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<char>>;
using StringP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<string>>;
using JsonNodeP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<JsonNode?>>;
using ParserResult = FParsec.CharParsers.ParserResult<JsonNode?, Unit>;

public static partial class Json5Parser {
    public static ParserResult Parse(string json) => json5.Run(json);

    private const string nonBreakingWhitespace = " \t\v\f\u00A0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A\u202F\u205F\u3000\uFEFF";

    private const string breakingWhitespace = "\n\u0085\u2028\u2029"; // \r and \r\n are normalized to \n by FParsec

    private const string whitespace = nonBreakingWhitespace + breakingWhitespace;

    private static readonly UnitP ws1 = Choice(UnicodeSpaces1, Skip('\uFEFF'));

    private static readonly UnitP wsc = Purify(SkipMany(Choice(
        ws1,
        Skip("//").AndR(SkipRestOfLine(skipNewline: true)),
        Skip("/*").AndR(SkipCharsTillString("*/", maxCount: int.MaxValue, skipString: true)))));

    private static readonly JsonNodeP jnull = StringP<JsonNode?>("null", null).Lbl("null");

    private static readonly JsonNodeP jbool =
        Choice(
            StringP("true", true),
            StringP("false", false))
        .Map(x => (JsonNode?)x) // deferred conversion ensures a new JsonNode is created every time
        .Lbl("bool");

    private static readonly JsonNodeP infinitySymbol =
        Choice(
            CharP('∞', double.PositiveInfinity),
            StringP("+∞", double.PositiveInfinity),
            StringP("-∞", double.NegativeInfinity))
        .Map(x => (JsonNode?)x) // deferred conversion ensures a new JsonNode is created every time
        .Lbl("number");

    private static readonly StringP @string =
        AnyOf("'\"")
        .And(StringContent)
        .Lbl("string");

    private static readonly JsonNodeP jstring = @string.Map(s => (JsonNode?)s);

    private static readonly JsonNodeP jnumber = NumberLiteral(numLiteralOpts, label: "number").Map(ParseNumLiteral);

    private static readonly JsonNodeP jarray =
        Between(
            Skip('[').AndR(wsc),
            Many(Rec(() => jvalue), sep: Skip(',').AndR(wsc), canEndWithSep: true),
            Skip(']'))
        .Map(elems => (JsonNode?)new JsonArray([.. elems]))
        .Lbl("array");

    private static readonly JsonNodeP jobject =
        Between(Skip('{').AndR(wsc), jobjProps, Skip('}'))
        .Map(props => (JsonNode?)new JsonObject(props))
        .Lbl("object");

    private static readonly JsonNodeP jvalue =
        Choice(
            jarray,
            jobject,
            jnull,
            jbool,
            infinitySymbol,
            jstring,
            jnumber)
        .And(wsc);

    private static readonly JsonNodeP json5 = wsc.And(jvalue).And(EOF);
}
