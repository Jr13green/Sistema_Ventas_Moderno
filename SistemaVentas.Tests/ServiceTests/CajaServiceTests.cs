using FluentAssertions;
using SistemaVentas.Services;

namespace SistemaVentas.Tests.ServiceTests;

public class CajaServiceTests
{
    private readonly CajaService _service = new();

    [Fact]
    public async Task ObtenerSaldoCajaAsync_DebeRetornarCero()
    {
        var saldo = await _service.ObtenerSaldoCajaAsync(DateTime.Today);
        saldo.Should().Be(0m);
    }

    [Fact]
    public async Task RegistrarAjusteAsync_NoDebeLanzarExcepcion()
    {
        var act = async () => await _service.RegistrarAjusteAsync("Ingreso", 10m, "Prueba");
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ObtenerResumenCajaPorFechaAsync_DebeRetornarCeros()
    {
        var result = await _service.ObtenerResumenCajaPorFechaAsync(DateTime.Today);
        result.totalIngresos.Should().Be(0m);
        result.totalEgresos.Should().Be(0m);
        result.saldoFinal.Should().Be(0m);
    }

    [Fact]
    public async Task RegistrarAjusteAsync_TipoEgreso_NoDebeLanzarExcepcion()
    {
        var act = async () => await _service.RegistrarAjusteAsync("Egreso", 5m, "Pago");
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ObtenerSaldoCajaAsync_CualquierFecha_SiempreCero()
    {
        var saldo = await _service.ObtenerSaldoCajaAsync(new DateTime(2024, 5, 1));
        saldo.Should().Be(0m);
    }

    [Fact]
    public async Task ObtenerResumenCajaPorFechaAsync_CualquierFecha_SiempreCeros()
    {
        var result = await _service.ObtenerResumenCajaPorFechaAsync(new DateTime(2024, 5, 1));
        result.Should().Be((0m, 0m, 0m));
    }
}
