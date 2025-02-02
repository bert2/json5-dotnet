namespace Json5.Internal;

using static FParsec.CharParsers;
using static FParsec.CharParsers.NumberLiteralResultFlags;

/// <summary>Provides helpers to convert JSON5 numbers to JSON numbers.</summary>
public static class JsonSpecNumFormat {
    /// <summary>
    /// <para>
    /// Transforms a parsed <see cref="NumberLiteral"/> such that it obeys the
    /// <see href="https://www.json.org/img/number.png">JSON spec</see>.
    /// </para>
    /// <para>
    /// Applies the following normalization rules:
    /// <list type="number">
    /// <item>Remove the leading plus sign if present,</item>
    /// <item>Trim superfluous leading zeros,</item>
    /// <item>Insert a <c>0</c> digit if the literal is a fraction but has no integer part,</item>
    /// <item>Insert a <c>0</c> digit if the literal is a fraction but has no fraction digits.</item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="nl">The <see cref="NumberLiteral"/>.</param>
    /// <returns>The normalized <see cref="NumberLiteral"/>.</returns>
    public static NumberLiteral Normalize(this NumberLiteral nl) => nl
        .TrimLeadingPlus()
        .TrimExcessLeadingZeros()
        .FillIntegerPart()
        .FillFraction();

    /// <summary>
    /// If the <see cref="NumberLiteral"/> has a leading plus sign then remove it.
    /// </summary>
    /// <param name="nl">The <see cref="NumberLiteral"/>.</param>
    /// <returns>
    /// A clone of the input <see cref="NumberLiteral"/> but without leading plus sign and
    /// with updated <see cref="NumberLiteral.Info"/> flags (or the original
    /// <see cref="NumberLiteral"/> if no normalization was needed).
    /// </returns>
    public static NumberLiteral TrimLeadingPlus(this NumberLiteral nl) => nl.HasPlusSign
        ? nl.Clone(nl.String[1..], toggle: HasPlusSign)
        : nl;

    /// <summary>
    /// Removes any leading zeros that are superfluous (e.g. <c>000.1</c> becomes <c>0.1</c>).
    /// </summary>
    /// <param name="nl">The <see cref="NumberLiteral"/>.</param>
    /// <returns>
    /// A clone of the input <see cref="NumberLiteral"/> but without superfluous leading zeros.
    /// </returns>
    public static NumberLiteral TrimExcessLeadingZeros(this NumberLiteral nl) {
        var (sign, literal) = nl.SplitSign();
        var trimmed = new string(literal
            .LeftPairs()
            .SkipWhile(w => w.Span is ['0', not '.'])
            .Select(w => w.Span[0])
            .PrependIfNotNull(sign)
            .ToArray());
        return nl.Clone(trimmed);
    }

    /// <summary>
    /// Inserts a <c>0</c> digit if the literal is a fraction but has no integer part (e.g.
    /// <c>.1</c> becomes <c>0.1</c>).
    /// </summary>
    /// <param name="nl">The <see cref="NumberLiteral"/>.</param>
    /// <returns>
    /// A clone of the input <see cref="NumberLiteral"/> with an integer part of <c>0</c>
    /// and with updated <see cref="NumberLiteral.Info"/> flags (or the original
    /// <see cref="NumberLiteral"/> if no normalization was needed).
    /// </returns>
    public static NumberLiteral FillIntegerPart(this NumberLiteral nl) {
        if (!nl.HasFraction || nl.HasIntegerPart) return nl;

        var (sign, literal) = nl.SplitSign();
        var filled = $"{sign}0{literal}";
        return nl.Clone(filled, toggle: HasIntegerPart);
    }

    /// <summary>
    /// Inserts a <c>0</c> digit if the literal is a fraction but has no fraction digits
    /// (e.g. <c>1.</c> becomes <c>1.0</c>).
    /// </summary>
    /// <param name="nl">The <see cref="NumberLiteral"/>.</param>
    /// <returns>
    /// A clone of the input <see cref="NumberLiteral"/> with a fraction digit of <c>0</c>
    /// (or the original <see cref="NumberLiteral"/> if no normalization was needed).
    /// </returns>
    public static NumberLiteral FillFraction(this NumberLiteral nl) {
        if (!nl.HasFraction) return nl;
        if (nl.String.EndsWith('.')) return nl.Clone(nl.String + '0');
        if (!nl.HasExponent) return nl;

        var e = nl.String.IndexOf('e', StringComparison.OrdinalIgnoreCase);
        return nl.String[e - 1] == '.'
            ? nl.Clone(nl.String[..e] + '0' + nl.String[e..])
            : nl;
    }

    /// <summary>
    /// Splits the string value of a <see cref="NumberLiteral"/> into its optional sign
    /// and the remaining literal string.
    /// </summary>
    /// <param name="nl">The <see cref="NumberLiteral"/>.</param>
    /// <returns>The optional sign character and the remaining literal string.</returns>
    public static (char? Sign, string Literal) SplitSign(this NumberLiteral nl)
        => nl.HasPlusSign || nl.HasMinusSign
            ? (nl.String[0], nl.String[1..])
            : (null, nl.String);

    /// <summary>
    /// Clones a <see cref="NumberLiteral"/> with a new string value and optionally toggles
    /// one of its <see cref="NumberLiteral.Info"/> flags.
    /// </summary>
    /// <param name="nl">The <see cref="NumberLiteral"/> to clone.</param>
    /// <param name="newLiteral">The string value of the new <see cref="NumberLiteral"/>.</param>
    /// <param name="toggle">The <see cref="NumberLiteral.Info"/> flag to toggle.</param>
    /// <returns>The cloned <see cref="NumberLiteral"/>.</returns>
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

    /// <summary>
    /// Creates a left-aligned sliding window of size 2 over the input string. Basically
    /// a fixed-size version of MoreLINQ's <c>WindowLeft(int)</c>, but only for strings.
    /// </summary>
    /// <param name="str">The input string.</param>
    /// <returns>A sequence representing each sliding window.</returns>
    private static IEnumerable<ReadOnlyMemory<char>> LeftPairs(this string str) {
        if (str.Length == 0) yield break;

        var mem = str.AsMemory();

        for (var i = 0; i < mem.Length - 1; i++)
            yield return mem[i..(i + 2)];

        yield return mem[^1..];
    }

    private static IEnumerable<T> PrependIfNotNull<T>(this IEnumerable<T> source, T? item)
        where T : struct
        => item == null ? source : source.Prepend(item.Value);
}
