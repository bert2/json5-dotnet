#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests;

using FluentAssertions;

using System;

using static FluentAssertions.FluentActions;

public class ParserErrors {
    [Fact]
    void GeneralError() =>
        Invoking(() => Parser.Parse("foo"))
        .Should().Throw<Exception>().WithMessage(
            """
            Error in Ln: 1 Col: 1
            foo
            ^
            Expecting: array, bool, null, number, object or string
            
            """);
}
