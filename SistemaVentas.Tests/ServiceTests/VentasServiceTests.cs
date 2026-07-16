using FluentAssertions;
using SistemaVentas.Services;

namespace SistemaVentas.Tests.ServiceTests;

public class VentasServiceTests
{
    private readonly VentasService _service = new();

    [Fact]
    public async Task CrearVentaAsync_SinJugadas_DebeFallar()
    {
        var result = await _service.CrearVentaAsync(1, []);
        result.exito.Should().BeFalse();
    }

    [Fact]
    public async Task CrearVentaAsync_ConJugadas_DebeSerExito()
    {
        var result = await _service.CrearVentaAsync(1, [(1, "10", 25m)]);
        result.exito.Should().BeTrue();
    }

    [Fact]
    public async Task CrearVentaAsync_ConJugadas_DebeRetornarMensajeExito()
    {
        var result = await _service.CrearVentaAsync(1, [(1, "10", 25m)]);
        result.mensaje.Should().Be("Venta creada correctamente");
    }

    [Fact]
    public async Task CrearVentaAsync_ConJugadas_DebeGenerarVentaId()
    {
        var result = await _service.CrearVentaAsync(1, [(1, "10", 25m)]);
        result.ventaId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CrearVentaAsync_UsuarioCualquiera_DebeSeguirExito()
    {
        var result = await _service.CrearVentaAsync(999, [(2, "88", 10m)]);
        result.exito.Should().BeTrue();
    }

    [Fact]
    public async Task CrearVentaAsync_NullJugadas_DebeFallar()
    {
        var result = await _service.CrearVentaAsync(1, null!);
        result.exito.Should().BeFalse();
    }

    [Fact]
    public async Task ObtenerTotalVentasActivasPorFechaAsync_DebeRetornarCeroPorDefecto()
    {
        var total = await _service.ObtenerTotalVentasActivasPorFechaAsync(DateTime.Today);
        total.Should().Be(0m);
    }

    [Fact]
    public async Task CrearVentaAsync_CuandoFalla_DebeMensajeEsperado()
    {
        var result = await _service.CrearVentaAsync(1, []);
        result.mensaje.Should().Be("Debe ingresar al menos una jugada");
    }
}
