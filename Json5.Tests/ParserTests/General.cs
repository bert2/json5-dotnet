#pragma warning disable IDE0051 // Remove unused private members

namespace Json5.Tests.ParserTests;

using FluentAssertions;

using Helpers;

using System;
using System.Text.Json.Serialization;
using System.Text.Json;

using static FluentAssertions.FluentActions;

public class General {
    // required to deserialize "Infinity" and "NaN"
    static readonly JsonSerializerOptions opts = new() {
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    [Fact]
    void Example() =>
        Parser.Parse(
            """
            {    
                foo: 'bar',
                while: true,

                this: 'is a \
            multi-line string',

                // this is an inline comment
                here: 'is another', // inline comment

                /* this is a block comment
                   that continues on another line */

                hex: 0xDEADbeef,
                half: .5,
                delta: +10,
                to: Infinity,   // and beyond!

                finally: 'a trailing comma',
                oh: [
                    "we shouldn't forget",
                    'arrays can have',
                    'trailing commas too',
                ],
            }
            """)
        .Should().Be(new {
            foo = "bar",
            @while = true,
            @this = "is a multi-line string",
            here = "is another",
            hex = 0xDEADbeef,
            half = .5,
            delta = +10,
            to = double.PositiveInfinity,
            @finally = "a trailing comma",
            oh = new[] { "we shouldn't forget", "arrays can have", "trailing commas too" }
        }, opts);

    [Fact]
    void Invalid() =>
        Invoking(() => Parser.Parse("foo"))
        .Should().Throw<Exception>().WithMessage(
            """
            Error in Ln: 1 Col: 1
            foo
            ^
            Expecting: array, bool, null, number, object or string
            """);

    [Fact]
    void Empty() =>
        Invoking(() => Parser.Parse(""))
        .Should().Throw<Exception>().WithMessage(
            """
            Error in Ln: 1 Col: 1
            Note: The error occurred at the end of the input stream.
            Expecting: array, bool, null, number, object or string
            """);
}
