using FluentAssertions;
using SistemaVentas.Converters;
using System.Globalization;

namespace SistemaVentas.Tests.ConverterTests;

public class DecimalToStringConverterTests
{
    private readonly DecimalToStringConverter _converter = new();

    [Fact]
    public void Convert_Decimal_DebeFormatearConDosDecimales()
    {
        var result = _converter.Convert(123.4m, typeof(string), null, CultureInfo.InvariantCulture);
        result.Should().Be("123.40");
    }

    [Fact]
    public void ConvertBack_NumeroValido_DebeRetornarDecimal()
    {
        var result = _converter.ConvertBack("50.75", typeof(decimal), null, CultureInfo.InvariantCulture);
        result.Should().Be(50.75m);
    }

    [Fact]
    public void ConvertBack_NumeroInvalido_DebeRetornarCero()
    {
        var result = _converter.ConvertBack("abc", typeof(decimal), null, CultureInfo.InvariantCulture);
        result.Should().Be(0m);
    }
}
