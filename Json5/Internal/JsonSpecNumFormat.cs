namespace Json5.Internal;

using static FParsec.CharParsers;
using static FParsec.CharParsers.NumberLiteralResultFlags;
using static MoreLinq.Extensions.WindowLeftExtension;

// Transforms a parsed number literal so it obeys the JSON spec (https://www.json.org/img/number.png).
public static class JsonSpecNumFormat {
    public static NumberLiteral Normalize(this NumberLiteral nl) => nl
        .TrimLeadingPlus()
        .TrimExcessLeadingZeros()
        .FillIntegerPart()
        .FillFraction();

    public static NumberLiteral TrimLeadingPlus(this NumberLiteral nl) => nl.HasPlusSign
        ? nl.Clone(nl.String[1..], toggle: HasPlusSign)
        : nl;

    public static NumberLiteral TrimExcessLeadingZeros(this NumberLiteral nl) {
        var (sign, literal) = nl.SplitSign();
        var trimmed = new string(literal
            .WindowLeft(2)
            .SkipWhile(w => w is ['0', not '.'])
            .Select(w => w[0])
            .PrependIfNotNull(sign)
            .ToArray());
        return nl.Clone(trimmed);
    }

    public static NumberLiteral FillIntegerPart(this NumberLiteral nl) {
        if (!nl.HasFraction || nl.HasIntegerPart) return nl;

        var (sign, literal) = nl.SplitSign();
        var filled = $"{sign}0{literal}";
        return nl.Clone(filled, toggle: HasIntegerPart);
    }

    public static NumberLiteral FillFraction(this NumberLiteral nl) {
        if (!nl.HasFraction) return nl;
        if (nl.String.EndsWith('.')) return nl.Clone(nl.String + '0');
        if (!nl.HasExponent) return nl;

        var e = nl.String.IndexOf('e', StringComparison.OrdinalIgnoreCase);
        return nl.String[e - 1] == '.'
            ? nl.Clone(nl.String[..e] + '0' + nl.String[e..])
            : nl;
    }

    public static (char? Sign, string Literal) SplitSign(this NumberLiteral nl)
        => nl.HasPlusSign || nl.HasMinusSign
            ? (nl.String[0], nl.String[1..])
            : (null, nl.String);

    public static NumberLiteral Clone(
        this NumberLiteral nl,
        string newLiteral,
        NumberLiteralResultFlags toggle = None)
        => new(
            newLiteral,
            nl.Info ^ toggle,
            nl.SuffixChar1,
            nl.SuffixChar2,
            nl.SuffixChar3,
            nl.SuffixChar4);

    private static IEnumerable<T> PrependIfNotNull<T>(this IEnumerable<T> source, T? item)
        where T : struct
        => item == null ? source : source.Prepend(item.Value);
}
