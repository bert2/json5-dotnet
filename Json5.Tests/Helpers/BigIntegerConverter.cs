namespace Json5.Tests.ConfigurationTests;

using System.ComponentModel;
using System.Globalization;
using System.Numerics;

public partial class ConfigurationBuilderTests {
    public class BigIntegerConverter : TypeConverter {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
            => sourceType == typeof(string);
        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
            => false;
        public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
            => BigInteger.Parse((string)value, culture);
        public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
            => throw new NotSupportedException();
    }
}
