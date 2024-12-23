#pragma warning disable IDE0065 // Misplaced using directive

using Microsoft.FSharp.Core;

using System.Text.Json.Nodes;

namespace Json5.Internal;

using FParsec;
using FParsec.CSharp;

using System.Text;
using System.Text.RegularExpressions;

using static CommonParsers;
using static FParsec.CSharp.CharParsersCS;
using static FParsec.CSharp.PrimitivesCS;
using static StringParser;

using Chars = FParsec.CharStream<Unit>;
using JsonNodeP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<JsonNode?>>;

public static partial class ObjectParser {
    public static JsonNodeP Json5Object { get; set; }

    static ObjectParser() {
        var escapeSequence =
            Skip('\\').AndR(Skip('u')) // compared to Skip(@"\u") this produces better errors (e.g. for @"foo\bar")
            .And(HexEncodedUnicode)
            .Lbl_("unicode escape sequence");

        var identifierChar = NoneOf($"\\:{WhitespaceChars}").Lbl_("identifier character");

        var identifier1stPass = Many1Strings(Choice(escapeSequence, Many1Chars(identifierChar)));

        var identifier2ndPass = CharParsers
            .identifier<Unit>(new(
                isAsciiIdStart: FSharpFunc.From((char c) => c is '$' or '_' || char.IsLetter(c)),
                isAsciiIdContinue: FSharpFunc.From((char c) => c is '$' or '_' || char.IsLetterOrDigit(c)),
                normalization: NormalizationForm.FormC,
                normalizeBeforeValidation: false,
                allowJoinControlChars: true,
                preCheckStart: FSharpFunc.From((char c) => c != ':' && !WhitespaceChars.Contains(c)),
                preCheckContinue: FSharpFunc.From((char c) => c != ':' && !WhitespaceChars.Contains(c)),
                allowAllNonAsciiCharsInPreCheck: false,
                label: null,
                invalidCharMessage: null))
            .And(EOF);

        // Parsing identifiers is a two step process.
        // 1st pass: accept basically anything while also replacing unicode escape sequences.
        // 2nd pass: use FParsec's identifier parser on the result of the 1st pass for validation and unicode magic.
        var identifier = FSharpFunc.From((Chars chars1) => {
            var startIdx = chars1.Index;
            var reply1 = identifier1stPass.Invoke(chars1);
            if (reply1.Status != ReplyStatus.Ok) return reply1;

            var chars2 = new Chars(reply1.Result, 0, reply1.Result.Length);
            var reply2 = identifier2ndPass.Invoke(chars2);
            if (reply2.Status == ReplyStatus.Ok) return reply2;

            // Rewind original stream to the position where identifer parsing started.
            chars1.Seek(startIdx);
            // Now skip ahead to where the validation error occured while correctly identifying escape sequences.
            chars1.SkipEscaped(chars2.Index);

            var err = chars1.MatchUnicodeEscape(out _)
                ? new(new ErrorMessage.Message(
                    "The unicode escape sequence starting at the indicated position was replaced with the invalid character "
                    + $"'{chars2.Peek()}'. The full identifier string after replacing all escape sequences was: '{reply1.Result}'."))
                : reply2.Error;

        return new Reply<string>(reply2.Status, err);
        }).Lbl_("identifier");

        var memberName = Choice(identifier, RawString);

        var property =
            memberName.And(WSC)
            .And(Skip(':')).And(WSC)
            .And(Rec(() => Json5Value))
            .Map(KeyValuePair.Create);

        var properties = Many(property, sep: Skip(',').AndR(WSC), canEndWithSep: true);

        Json5Object =
            Between(Skip('{').AndR(WSC), properties, Skip('}'))
            .Map(props => (JsonNode?)new JsonObject(props))
            .Lbl("object");
    }

    public static void SkipEscaped(this Chars chars, long utf16Offset) {
        if (utf16Offset < 0) throw new ArgumentOutOfRangeException(nameof(utf16Offset), "Must be positive.");

        for(; utf16Offset > 0; utf16Offset--)
            chars.Skip(chars.MatchUnicodeEscape(out var len) ? len : 1);
    }

    public static bool MatchUnicodeEscape(this Chars chars, out int length) {
        if (chars.Match(Utf16Escape()).Success) {
            length = 6;
            return true;
        } else if (chars.Match(Utf32Escape()) is { Success: true, Length: var len }) {
            length = len;
            return true;
        } else {
            length = 0;
            return false;
        }
    }

    [GeneratedRegex(@"^\\u[\da-fA-F]{4}")]
    private static partial Regex Utf16Escape();

    [GeneratedRegex(@"^\\u\{[\da-fA-F]+\}")]
    private static partial Regex Utf32Escape();
}
