namespace Json5.Tests;

using FluentAssertions;
using FluentAssertions.Execution;

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text.Json.Nodes;

public static class AssertionExtensions {
    public static JsonNodeAssertions Should(this JsonNode? actualValue) => new(actualValue!);
}

public record JsonNodeAssertions(JsonNode Subject) {
    private const string identifier = nameof(JsonNode);

    public AndConstraint<JsonNodeAssertions> BeNull(string because = "", params object[] becauseArgs) {
        Execute.Assertion
            .ForCondition(Subject is null)
            .BecauseOf(because, becauseArgs)
            .WithDefaultIdentifier(identifier)
            .FailWith("Expected {context} to be <null>{reason}, but found {0}.", Subject?.ToString());

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

    public AndConstraint<JsonNodeAssertions> Be(JsonNode? expected, string because = "", params object[] becauseArgs) {
        var (actualJson, expectedJson) = (Subject?.ToString(), expected?.ToString());
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(actualJson == expectedJson)
            .WithDefaultIdentifier(identifier)
            .FailWith("Expected {context} to be {0}{reason}, but found {1}.", expectedJson, actualJson);

        return new AndConstraint<JsonNodeAssertions>(this);
    }

    public AndConstraint<JsonNodeAssertions> BeValue<T>(T expected, string because = "", params object[] becauseArgs)
        where T: IEqualityOperators<T, T, bool> {
        var actualValue = Subject.GetValue<T>();
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(actualValue == expected)
            .WithDefaultIdentifier(identifier)
            .FailWith("Expected {context} to be {0}{reason}, but found {1}.", expected, actualValue);

        return new AndConstraint<JsonNodeAssertions>(this);
    }

    public AndConstraint<JsonNodeAssertions> BeJson(
        [StringSyntax("Json")] string expected,
        string because = "",
        params object[] becauseArgs)
        => Be(JsonNode.Parse(expected), because, becauseArgs);
}
