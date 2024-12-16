namespace Json5.Tests.Helpers;

using System.Text.Json.Nodes;

public static class AssertionExtensions {
    public static JsonNodeAssertions Should(this JsonNode? actualValue) => new(actualValue!);
}
