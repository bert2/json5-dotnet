namespace Json5.Tests.Helpers;

using FluentAssertions;
using FluentAssertions.Execution;

using System.Reflection;
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

    public AndConstraint<JsonNodeAssertions> BeNaN(string because = "", params object[] becauseArgs) {
        Execute.Assertion
            .ForCondition(double.IsNaN(Subject.GetValue<double>()))
            .BecauseOf(because, becauseArgs)
            .WithDefaultIdentifier(identifier)
            .FailWith(
                "Expected {context} to be double.NaN{reason}, but found {0}.",
                Subject?.ToJsonString(opts));

        return new AndConstraint<JsonNodeAssertions>(this);
    }

    public AndConstraint<JsonNodeAssertions> BeJson(JsonNode? expected, string because = "", params object[] becauseArgs) {
        switch (expected?.GetValueKind()) {
            case null:
                Subject.Should().BeNull();
                break;
            case JsonValueKind.Object:
                Subject.Should().HaveValueKind(JsonValueKind.Object);
                var actualObj = Subject.AsObject();
                var expectedObj = expected.AsObject();
                AssertSameProperties(actualObj, expectedObj);
                expectedObj.ForEach(expProp => actualObj[expProp.Key].Should().BeJson(expProp.Value));
                break;
            case JsonValueKind.Array:
                Subject.Should().HaveValueKind(JsonValueKind.Array);
                var actualArr = Subject.AsArray();
                var expectedArr = expected.AsArray();
                Execute.Assertion
                    .ForCondition(actualArr.Count == expectedArr.Count)
                    .WithDefaultIdentifier(identifier)
                    .FailWith(
                        "Expected {context} at path {2} to be an array with {0} items, but found {1}.",
                        expectedArr.Count,
                        actualArr.Count,
                        Subject.GetPath());
                actualArr.Zip(expectedArr).ForEach(x => x.First.Should().BeJson(x.Second));
                break;
            default:
                Execute.Assertion
                    .BecauseOf(because, becauseArgs)
                    .ForCondition(JsonNode.DeepEquals(Subject, expected))
                    .WithDefaultIdentifier(identifier)
                    .FailWith(
                        "Expected {context} at path {2} to be {0}{reason}, but found {1}.",
                        Subject.ToJsonString(opts),
                        expected.ToJsonString(opts),
                        Subject.GetPath());
                break;
        }

        return new AndConstraint<JsonNodeAssertions>(this);
    }

    public AndConstraint<JsonNodeAssertions> Be(object? expected) {
        if (expected == null) {
            Subject.Should().BeNull();
        } else if (expected.GetType().Name.Contains("AnonymousType")) {
            Subject.Should().BeObject(expected);
        } else if (expected.GetType().IsArray) {
            Subject.Should().BeArray((object[])expected);
        } else {
            Subject.Should().NotHaveValueKind(JsonValueKind.Object);
            Subject.Should().NotHaveValueKind(JsonValueKind.Array);
            Subject.Should().BeValue(expected);
        }

        return new(new JsonNodeAssertions(this));
    }

    public AndConstraint<JsonNodeAssertions> BeOfType(Type expected, string because = "", params object[] becauseArgs) {
        var actual = Subject?.GetValue<object>().GetType();
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(Equals(actual, expected))
            .WithDefaultIdentifier(identifier)
            .FailWith("Expected type of {context} at path {2} to be {0}{reason}, but found {1}.", expected, actual, Subject?.GetPath());

        return new AndConstraint<JsonNodeAssertions>(this);
    }

    public AndConstraint<JsonNodeAssertions> BeOfType<TExpected>(string because = "", params object[] becauseArgs)
        => BeOfType(typeof(TExpected), because, becauseArgs);

    public AndConstraint<JsonNodeAssertions> BeValue<T>(T expected, string because = "", params object[] becauseArgs)
        where T : notnull {
        BeOfType(expected.GetType());

        var actual = Subject.GetValue<T>();
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(Equals(actual, expected))
            .WithDefaultIdentifier(identifier)
            .FailWith("Expected {context} at path {2} to be {0}{reason}, but found {1}.", expected, actual, Subject?.GetPath());

        return new AndConstraint<JsonNodeAssertions>(this);
    }

    public AndConstraint<JsonNodeAssertions> BeArray(params object?[] expected) {
        Subject.Should().HaveValueKind(JsonValueKind.Array);
        var actual = Subject.AsArray();

        Execute.Assertion
            .ForCondition(actual.Count == expected.Length)
            .WithDefaultIdentifier(identifier)
            .FailWith("Expected {context} at path {2} to be an array with {0} items, but found {1}.", expected.Length, actual.Count, Subject.GetPath());

        actual.Zip(expected).ForEach(x => x.First.Should().Be(x.Second));

        return new(new JsonNodeAssertions(this));
    }

    public AndConstraint<JsonNodeAssertions> BeObject(object expected) {
        Subject.Should().HaveValueKind(JsonValueKind.Object);
        var actual = Subject.AsObject();

        var expType = expected.GetType();
        if (!expType.Name.Contains("AnonymousType"))
            throw new InvalidOperationException($"Use an anonymous type to assert the JsonNode at path {Subject.GetPath()}");
        var expProps = expType.GetProperties();

        AssertSameProperties(actual, expProps);

        expProps.ForEach(expProp => actual[expProp.Name].Should().Be(expProp.GetValue(expected)));

        return new AndConstraint<JsonNodeAssertions>(this);
    }

    public AndConstraint<JsonNodeAssertions> HaveValueKind(JsonValueKind expected) {
        var actual = Subject?.GetValueKind();
        Execute.Assertion
            .ForCondition(actual == expected)
            .WithDefaultIdentifier(identifier)
            .FailWith("Expected {context} at path {2} to be a {0}, but found {1}.", expected, actual, Subject?.GetPath());

        return new AndConstraint<JsonNodeAssertions>(this);
    }

    public AndConstraint<JsonNodeAssertions> NotHaveValueKind(JsonValueKind unexpected) {
        var actual = Subject?.GetValueKind();
        Execute.Assertion
            .ForCondition(actual != unexpected)
            .WithDefaultIdentifier(identifier)
            .FailWith("Expected {context} at path {1} to not be a {0}.", unexpected, Subject?.GetPath());

        return new AndConstraint<JsonNodeAssertions>(this);
    }

    private static void AssertSameProperties(JsonObject actual, PropertyInfo[] expected) {
        var properties = expected.Select(p => p.Name);
        properties.Should().BeEquivalentTo(
            actual.Select(p => p.Key),
            "the actual JsonObject at path {0} and the expected anonymous type should have the same properties", actual.GetPath());
    }

    private static void AssertSameProperties(JsonObject actual, JsonObject expected) {
        var properties = expected.Select(p => p.Key);
        properties.Should().BeEquivalentTo(
            actual.Select(p => p.Key),
            "the actual JsonObject at path {0} and the expected JsonObject should have the same properties", actual.GetPath());
    }
}
