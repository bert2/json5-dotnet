namespace Json5.Tests.Helpers;

using FluentAssertions;
using FluentAssertions.Collections;

using System.Text.Json.Nodes;

public class JsonArrayAssertions(IEnumerable<JsonNode?> actual) : GenericCollectionAssertions<JsonNode?>(actual) {
    public new AndConstraint<GenericCollectionAssertions<JsonNode?>> Equal(params JsonNode?[] elements)
        => Equal(elements, JsonNode.DeepEquals);
}
