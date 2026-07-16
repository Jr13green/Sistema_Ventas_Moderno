using FluentAssertions;
using SistemaVentas.Services;

namespace SistemaVentas.Tests.ServiceTests;

public class ReportesServiceTests
{
    private readonly ReportesService _service = new();

    [Fact]
    public async Task ObtenerResumenPeriodoAsync_DebeRetornarCeros()
    {
        var result = await _service.ObtenerResumenPeriodoAsync(DateTime.Today.AddDays(-1), DateTime.Today);
        result.Should().Be((0m, 0m, 0m, 0));
    }

    [Fact]
    public async Task ObtenerTopVendedoresAsync_DebeRetornarListaVacia()
    {
        var result = await _service.ObtenerTopVendedoresAsync(DateTime.Today.AddDays(-1), DateTime.Today, 10);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ExportarVentasCSVAsync_DebeRetornarEncabezado()
    {
        var csv = await _service.ExportarVentasCSVAsync(DateTime.Today.AddDays(-1), DateTime.Today);
        csv.Should().Be("Fecha,Venta,Premio,Ganancia");
    }

    [Fact]
    public async Task ExportarVentasCSVAsync_DebeIncluirColumnaGanancia()
    {
        var csv = await _service.ExportarVentasCSVAsync(DateTime.Today.AddDays(-1), DateTime.Today);
        csv.Should().Contain("Ganancia");
    }

    [Fact]
    public async Task ObtenerTopVendedoresAsync_LimiteDistinto_DebeMantenerVacio()
    {
        var result = await _service.ObtenerTopVendedoresAsync(DateTime.Today.AddDays(-1), DateTime.Today, 1);
        result.Should().BeEmpty();
    }
}
