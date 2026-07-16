using FluentAssertions;
using SistemaVentas.Converters;
using System.Globalization;
using System.Windows;

namespace SistemaVentas.Tests.ConverterTests;

public class BoolToVisibilityConverterTests
{
    private readonly BoolToVisibilityConverter _converter = new();

    [Fact]
    public void Convert_True_DebeSerVisible()
    {
        var result = _converter.Convert(true, typeof(Visibility), null, CultureInfo.InvariantCulture);
        result.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void Convert_False_DebeSerCollapsed()
    {
        var result = _converter.Convert(false, typeof(Visibility), null, CultureInfo.InvariantCulture);
        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void ConvertBack_Visible_DebeSerTrue()
    {
        var result = _converter.ConvertBack(Visibility.Visible, typeof(bool), null, CultureInfo.InvariantCulture);
        result.Should().Be(true);
    }
}
