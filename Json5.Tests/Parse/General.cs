namespace Json5.Tests.Parse;

using FluentAssertions;

using Helpers;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using static FluentAssertions.FluentActions;
using static Json5.Json5Parser;

public class General {
    // required to deserialize "Infinity" and "NaN"
    static readonly JsonSerializerOptions opts = new() {
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    [Fact]
    public void Example() =>
        Parse(
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
    public void Invalid() =>
        Invoking(() => Parse("foo"))
        .Should().Throw<Exception>().WithMessage(
            """
            Error in Ln: 1 Col: 1
            foo
            ^
            Expecting: array, bool, null, number, object or string
            """);

    [Fact]
    public void Empty() =>
        Invoking(() => Parse(""))
        .Should().Throw<Exception>().WithMessage(
            """
            Error in Ln: 1 Col: 1
            Note: The error occurred at the end of the input stream.
            Expecting: array, bool, null, number, object or string
            """);
}
