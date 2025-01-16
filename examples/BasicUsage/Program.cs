using System.Text.Json;

var json5 =
    """
    {
        description: 'This is a basic example showing how to translate JSON5 to JSON, \
                      how to parse JSON5 as a `System.Text.Json.Nodes.JsonNode`, \
                      and how to deserialize JSON5 to a C# type.',
        nums: [.1, -Infinity, NaN, 2.e-3, 0b101, 0xBEEF],
    }
    """;

Console.WriteLine(
    $"""
    JSON5:

    {json5}

    Translated to JSON:

    {Json5.Parser.Translate(json5)}

    Parsed as JsonNode:

    {Json5.Parser.Parse(json5)}

    """);

// `JsonSerializerOptions.Web` makes property names case-insensitive and allows the
// floating point literals "NaN" and "Infinity".
Console.WriteLine(
    $"""
    Deserialized as record:

    {Json5.Parser.Parse(json5).Deserialize<Foo>(JsonSerializerOptions.Web)}
    """);

public record Foo(string Description, double[] Nums) {
    public override string ToString()
        => $"Foo {{ Description = {Description[..15]}..., Nums = [{string.Join(", ", Nums)}] }}";
}
