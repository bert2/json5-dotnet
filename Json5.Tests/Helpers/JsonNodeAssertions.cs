namespace Json5.Tests.Helpers;

using FluentAssertions;
using FluentAssertions.Execution;

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

public record JsonNodeAssertions(JsonNode Subject) {
    private const string identifier = nameof(JsonNode);
    private static JsonSerializerOptions Opts => new() { NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals };

    public AndConstraint<JsonNodeAssertions> BeNull(string because = "", params object[] becauseArgs) {
        Execute.Assertion
            .ForCondition(Subject is null)
            .BecauseOf(because, becauseArgs)
            .WithDefaultIdentifier(identifier)
            .FailWith("Expected {context} to be <null>{reason}, but found {0}.", Subject?.ToJsonString(Opts));

        return new AndConstraint<JsonNodeAssertions>(this);
    }

    public AndConstraint<JsonNodeAssertions> BeNaN(string because = "", params object[] becauseArgs) {
        Execute.Assertion
            .ForCondition(double.IsNaN(Subject.GetValue<double>()))
            .BecauseOf(because, becauseArgs)
            .WithDefaultIdentifier(identifier)
            .FailWith(
                "Expected {context} to be <null>{reason}, but found {0}.",
                Subject?.ToJsonString(Opts));

        return new AndConstraint<JsonNodeAssertions>(this);
    }

    public AndConstraint<JsonNodeAssertions> Be(JsonNode? expected, string because = "", params object[] becauseArgs) {
        var (actualJson, expectedJson) = (Subject?.ToJsonString(Opts), expected?.ToJsonString(Opts));
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(actualJson == expectedJson)
            .WithDefaultIdentifier(identifier)
            .FailWith("Expected {context} to be {0}{reason}, but found {1}.", expectedJson, actualJson);

        return new AndConstraint<JsonNodeAssertions>(this);
    }

    public AndConstraint<JsonNodeAssertions> BeValue<T>(T expected, string because = "", params object[] becauseArgs)
        where T : IEquatable<T> {
        var actual = Subject.GetValue<T>();
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(actual.Equals(expected))
            .WithDefaultIdentifier(identifier)
            .FailWith("Expected {context} to be {0}{reason}, but found {1}.", expected, actual);

        return new AndConstraint<JsonNodeAssertions>(this);
    }

    public AndConstraint<JsonArrayAssertions> BeArray() => new(new JsonArrayAssertions(Subject.AsArray()));
}
