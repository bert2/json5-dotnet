namespace Json5.Tests.Helpers;

using System;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

public class BigIntegerJsonConverter : JsonConverter<BigInteger> {
    public override BigInteger Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => BigInteger.Parse(Encoding.UTF8.GetString(reader.ValueSpan), CultureInfo.InvariantCulture);

    public override void Write(Utf8JsonWriter writer, BigInteger value, JsonSerializerOptions options)
        => writer.WriteRawValue(value.ToString(CultureInfo.InvariantCulture));
}
