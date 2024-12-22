#pragma warning disable IDE0065 // Misplaced using directive

using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

using System.Text.Json.Nodes;

namespace Json5.Parsing;

using FParsec;
using FParsec.CSharp;

using System.Text;

using static FParsec.CSharp.CharParsersCS;
using static FParsec.CSharp.PrimitivesCS;
using static System.Buffers.Binary.BinaryPrimitives;

using CharP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<char>>;
using Chars = FParsec.CharStream<Unit>;
using StringP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<string>>;
using UnitP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<Unit>>;
using JsonNodeP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<JsonNode?>>;
using JsonPropP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<KeyValuePair<string, JsonNode?>>>;
using JsonPropsP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<FSharpList<KeyValuePair<string, JsonNode?>>>>;

public static partial class Json5Parser {
    private static readonly CharP escapableChar = NoneOf("123456789");

    private static readonly UnitP nonBreakSpaces = Purify(SkipMany(AnyOf(nonBreakingWhitespace)));

    private static readonly StringP hexEncodedAscii = Array(2, Hex)
        .Map(x => new string((char)Convert.FromHexString(x)[0], 1));

    private static readonly StringP hexEncodedUnicode = Array(4, Hex)
        .Map(x => new string((char)ReadUInt16BigEndian(Convert.FromHexString(x)), 1));

    private static readonly StringP escSeqInStrContent =
        Skip('\\')
        .And(escapableChar)
        .And(c => c switch {
            'n' => Return("\n"),
            't' => Return("\t"),
            'r' => Return("\r"),
            'f' => Return("\f"),
            '0' => Return("\0"),
            'b' => Return("\b"),
            'v' => Return("\v"),
            'x' => hexEncodedAscii,
            'u' => hexEncodedUnicode,
            _ when breakingWhitespace.Contains(c) => nonBreakSpaces.Return(""),
            _ => Return(c.ToString())
        })
        .Lbl_("escape sequence");

    private static readonly StringP escSeqInIdentifier =
        Skip('\\').AndR(Skip('u')) // compared to Skip(@"\u") this produces better errors (e.g. for @"foo\bar")
        .And(hexEncodedUnicode)
        .Lbl_("unicode escape sequence");

    private static readonly CharP identifierChar = NoneOf($"\\:{whitespace}").Lbl_("identifier character");

    private static readonly StringP fstPassP = Many1Strings(Choice(escSeqInIdentifier, Many1Chars(identifierChar)));

    private static readonly StringP sndPassP = CharParsers
        .identifier<Unit>(new(
            isAsciiIdStart: FSharpFunc.From((char c) => c is '$' or '_' || char.IsLetter(c)),
            isAsciiIdContinue: FSharpFunc.From((char c) => c is '$' or '_' || char.IsLetterOrDigit(c)),
            normalization: NormalizationForm.FormC,
            normalizeBeforeValidation: false,
            allowJoinControlChars: true,
            preCheckStart: FSharpFunc.From((char c) => c != ':' && !whitespace.Contains(c)),
            preCheckContinue: FSharpFunc.From((char c) => c != ':' && !whitespace.Contains(c)),
            allowAllNonAsciiCharsInPreCheck: false,
            label: null,
            invalidCharMessage: null))
        .And(EOF);

    public static readonly StringP identifier = FSharpFunc.From((Chars chars1) => {
        var reply1 = fstPassP.Invoke(chars1);
        if (reply1.Status != ReplyStatus.Ok) return reply1;

        var inputLen = reply1.Result.Length;
        var chars2 = new Chars(reply1.Result, 0, inputLen);
        var reply2 = sndPassP.Invoke(chars2);
        if (reply2.Status == ReplyStatus.Ok) return reply2;

        // Rewind original stream to the position where the validation error occured.
        chars1.Skip(-inputLen + chars2.Index);

        var err = chars1.Peek() == chars2.Peek()
            ? reply2.Error
            : new(new ErrorMessage.Message(
                "The unicode escape sequence ending at the indicated position was replaced with the invalid character "
                + $"'{chars2.Peek()}'. The full identifier string after replacing all escape sequences was: '{reply1.Result}'."));

        return new Reply<string>(reply2.Status, err);
    }).Lbl_("identifier");

    private static readonly StringP jobjMemberName = Choice(identifier, @string);

    private static readonly JsonPropP jobjProp =
        jobjMemberName.And(wsc)
        .And(Skip(':')).And(wsc)
        .And(Rec(() => jvalue))
        .Map(KeyValuePair.Create);

    private static readonly JsonPropsP jobjProps = Many(jobjProp, sep: Skip(',').AndR(wsc), canEndWithSep: true);

    private static StringP StringContent(char quote) =>
        ManyStrings(
            ManyChars(NoneOf($"{quote}\\{breakingWhitespace}").Lbl("next string character")),
            sep: escSeqInStrContent)
        .And(Skip(quote));
}
