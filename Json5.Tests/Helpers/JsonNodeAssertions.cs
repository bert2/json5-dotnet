namespace Json5.Tests.Helpers;

using FluentAssertions;
using FluentAssertions.Execution;

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

public record JsonNodeAssertions(JsonNode Subject) {
    private const string identifier = nameof(JsonNode);
    private static readonly JsonSerializerOptions opts = new() { NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals };

    public AndConstraint<JsonNodeAssertions> BeNull(string because = "", params object[] becauseArgs) {
        Execute.Assertion
            .ForCondition(Subject is null)
            .BecauseOf(because, becauseArgs)
            .WithDefaultIdentifier(identifier)
            .FailWith("Expected {context} at path {1} to be <null>{reason}, but found {0}.", Subject?.ToJsonString(opts), Subject?.GetPath());

        return new AndConstraint<JsonNodeAssertions>(this);
    }

    public AndConstraint<JsonNodeAssertions> NotBeNull(string because = "", params object[] becauseArgs) {
        Execute.Assertion
            .ForCondition(Subject is not null)
            .BecauseOf(because, becauseArgs)
            .WithDefaultIdentifier(identifier)
            .FailWith("Expected {context} not to be <null>{reason}.");

        return new AndConstraint<JsonNodeAssertions>(this);
    }

    public AndConstraint<JsonNodeAssertions> BeEmptyObject() {
        Subject.Should().HaveValueKind(JsonValueKind.Object);
        var props = Subject.AsObject().Count;

        Execute.Assertion
            .ForCondition(props == 0)
            .WithDefaultIdentifier(identifier)
            .FailWith("Expected {context} at path {1} to be an emty object, but found {0} properties.", props, Subject.GetPath());

        return new AndConstraint<JsonNodeAssertions>(this);
    }

    public AndConstraint<JsonNodeAssertions> Be(JsonObject expected) => Be((JsonNode)expected);

    public AndConstraint<JsonNodeAssertions> Be(JsonArray expected) => Be((JsonNode)expected);

    public AndConstraint<JsonNodeAssertions> Be(JsonNode? expected) {
        Execute.Assertion
            .ForCondition(JsonNode.DeepEquals(Subject, expected))
            .WithDefaultIdentifier(identifier)
            .FailWith("Expected {context} to match {0}, but found {1}.", expected, Subject);
        return new(new JsonNodeAssertions(this));
    }

    public AndConstraint<JsonNodeAssertions> Be<T>(T expected, string because = "", params object[] becauseArgs) {
        var actual = Subject.Deserialize<T>(opts);
        actual.Should().BeEquivalentTo(
            expected,
            opts => opts.WithAutoConversion().ThrowingOnMissingMembers(),
            because,
            becauseArgs);
        return new(new JsonNodeAssertions(this));
    }

    public AndConstraint<JsonNodeAssertions> HaveValueKind(JsonValueKind expected) {
        var actual = Subject?.GetValueKind();
        Execute.Assertion
            .ForCondition(actual == expected)
            .WithDefaultIdentifier(identifier)
            .FailWith("Expected {context} at path {2} to be a {0}, but found {1}.", expected, actual, Subject?.GetPath());

        return new AndConstraint<JsonNodeAssertions>(this);
    }
}
