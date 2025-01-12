#pragma warning disable IDE0065 // Misplaced using directive

using Microsoft.FSharp.Core;

using System.Text.Json.Nodes;

namespace Json5.Internal;

using FParsec;
using FParsec.CSharp;

using System.Text;

using static CommonParsers;
using static FParsec.CSharp.CharParsersCS;
using static FParsec.CSharp.PrimitivesCS;
using static FParsec.IdentifierValidator;
using static StringParser;

using Chars = FParsec.CharStream<Unit>;
using JsonNodeP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<JsonNode?>>;
using StringP = FSharpFunc<FParsec.CharStream<Unit>, FParsec.Reply<string>>;

public static class ObjectParser {
    public static JsonNodeP Json5Object { get; set; }

    public static StringP Json5ObjectS { get; set; } = Fail<string>("nope").Lbl("object");

    static ObjectParser() {
        var escapeSequence = Skip("\\u").And(HexEncodedUnicode).Lbl_("unicode escape sequence");

        var identifierChar = NoneOf($"\\:}},/{WhitespaceChars}").Lbl_("identifier character");

        var identifier1stPass = Many1Strings(Choice(escapeSequence, Many1Chars(identifierChar)));

        var identifier2ndPass = CharParsers
            .identifier<Unit>(new(
                isAsciiIdStart: FSharpFunc.From((char c) => c is '$' or '_' || IsXIdStartOrSurrogate(c)),
                isAsciiIdContinue: FSharpFunc.From((char c) => c is '$' || IsXIdContinueOrJoinControlOrSurrogate(c)),
                normalization: NormalizationForm.FormC,
                normalizeBeforeValidation: false,
                allowJoinControlChars: true,
                // Accept anything in pre-check, because 1st pass already made sure there
                // are no raw/unencoded whitespace or object literal tokens in the input.
                preCheckStart: FSharpFunc.From((char _) => true),
                preCheckContinue: FSharpFunc.From((char _) => true),
                allowAllNonAsciiCharsInPreCheck: false,
                label: "identifier",
                invalidCharMessage: "The identifier contains an invalid character at the indicated position."))
            .And(EOF);

        // Parsing identifiers is a two step process.
        // 1st pass: accept basically anything while also replacing unicode escape sequences.
        // 2nd pass: use FParsec's identifier parser on the result of the 1st pass for validation and unicode magic.
        var identifier = FSharpFunc.From((Chars chars) => {
            var identStartIdx = chars.Index;
            var pass1 = identifier1stPass.Invoke(chars);
            if (pass1.Status != ReplyStatus.Ok) return pass1;

            using var preparsedIdent = new Chars(pass1.Result, 0, pass1.Result.Length);
            var pass2 = identifier2ndPass.Invoke(preparsedIdent);
            if (pass2.Status == ReplyStatus.Ok) return pass2;

            // Parsing failed: rewind main CharStream to the position where identifer parsing started.
            chars.Seek(identStartIdx);
            // Now skip ahead to where the validation error occured while correctly identifying escape sequences.
            chars.SkipEscaped(preparsedIdent.Index);

            // Generate helpful error when error occured at an escape sequence.
            var err = chars.MatchUnicodeEscape(out _)
                ? new(new ErrorMessage.Message(
                    "The unicode escape sequence starting at the indicated position was replaced with the invalid character "
                    + $"'{preparsedIdent.Peek()}'. The full identifier string after replacing all escape sequences was: '{pass1.Result}'."))
                : pass2.Error;

            return new Reply<string>(pass2.Status, err);
        }).Lbl("identifier");

        var memberName = Choice(RawString, identifier);

        var property =
            memberName.And(WSC)
            .And(Skip(':')).And(WSC)
            .And(Rec(() => Json5Value))
            .Map(KeyValuePair.Create)
            .Lbl("property");

        var properties = Many(property, sep: Skip(',').AndR(WSC), canEndWithSep: true);

        Json5Object =
            Between(Skip('{').AndR(WSC), properties, Skip('}'))
            .Map(props => (JsonNode?)new JsonObject(props))
            .Lbl("object");

        var propertyS =
            memberName.And(WSC)
            .And(Skip(':')).And(WSC)
            .And(Rec(() => Json5ValueS))
            .Map((k, v) => '"' + k + '"' + ':' + v)
            .Lbl("property");

        var propertiesS = Many(propertyS, sep: Skip(',').AndR(WSC), canEndWithSep: true);

        Json5ObjectS =
            Between(Skip('{').AndR(WSC), propertiesS, Skip('}'))
            .Map(props => '{' + string.Join(',', props) + '}')
            .Lbl("object");
    }
}
