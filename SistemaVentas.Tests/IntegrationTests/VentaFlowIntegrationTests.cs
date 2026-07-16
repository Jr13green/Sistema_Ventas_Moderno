using FluentAssertions;
using SistemaVentas.Models;
using SistemaVentas.Services;
using SistemaVentas.Tests.Fixtures;

namespace SistemaVentas.Tests.IntegrationTests;

public class VentaFlowIntegrationTests
{
    [Fact]
    public async Task InicializarVentaViewModel_DebeCargarSorteos()
    {
        var vm = new TestVentaViewModel(new VentasService(), new SorteosService(), 1);

        await vm.InicializarAsync();

        vm.SorteosDia.Should().HaveCount(2);
    }

    [Fact]
    public void AgregarJugadaYTotal_DebeAcumular()
    {
        var vm = new TestVentaViewModel(new VentasService(), new SorteosService(), 1)
        {
            NumeroIngresado = "7",
            MontoIngresado = 40m,
            SorteoSeleccionado = new SorteoDiario { Id = 1, Nombre = "Matutino" }
        };

        vm.AgregarJugadaCommand.Execute(null);

        vm.JugadasAgregadas.Should().HaveCount(1);
        vm.TotalVenta.Should().Be(40m);
    }

    [Fact]
    public async Task GuardarVenta_FlujoCompleto_DebeLimpiarJugadas()
    {
        var vm = new TestVentaViewModel(new VentasService(), new SorteosService(), 1)
        {
            NumeroIngresado = "17",
            MontoIngresado = 20m,
            SorteoSeleccionado = new SorteoDiario { Id = 1, Nombre = "Matutino" }
        };

        vm.AgregarJugadaCommand.Execute(null);
        vm.GuardarVentaCommand.Execute(null);
        await Task.Delay(40);

        vm.JugadasAgregadas.Should().BeEmpty();
    }
}
