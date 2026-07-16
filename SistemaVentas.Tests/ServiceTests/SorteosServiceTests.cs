using FluentAssertions;
using SistemaVentas.Services;

namespace SistemaVentas.Tests.ServiceTests;

public class SorteosServiceTests
{
    private readonly SorteosService _service = new();

    [Fact]
    public async Task CrearSorteosDelDiaAsync_NoDebeLanzarExcepcion()
    {
        var act = async () => await _service.CrearSorteosDelDiaAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ActualizarEstadosSorteosAsync_NoDebeLanzarExcepcion()
    {
        var act = async () => await _service.ActualizarEstadosSorteosAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ObtenerSorteosDiarioAsync_DebeRetornarDosSorteos()
    {
        var result = await _service.ObtenerSorteosDiarioAsync(DateTime.Today);
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ObtenerSorteosDiarioAsync_DebeRetornarNombresEsperados()
    {
        var result = await _service.ObtenerSorteosDiarioAsync(DateTime.Today);
        result.Select(x => x.Nombre).Should().Contain(["Matutino", "Vespertino"]);
    }

    [Fact]
    public async Task ObtenerSorteosDiarioAsync_DebeUsarFechaSolicitada()
    {
        var fecha = new DateTime(2026, 1, 10);
        var result = await _service.ObtenerSorteosDiarioAsync(fecha);
        result.Should().OnlyContain(x => x.Fecha == fecha);
    }

    [Fact]
    public async Task ObtenerSorteosDiarioAsync_EstadoInicialAbierto()
    {
        var result = await _service.ObtenerSorteosDiarioAsync(DateTime.Today);
        result.Should().OnlyContain(x => x.Estado == "Abierto");
    }
}
