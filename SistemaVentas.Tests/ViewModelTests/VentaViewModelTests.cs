using FluentAssertions;
using Moq;
using SistemaVentas.Models;
using SistemaVentas.Tests.Fixtures;
using SistemaVentas.ViewModels;

namespace SistemaVentas.Tests.ViewModelTests;

public class VentaViewModelTests
{
    [Fact]
    public void AgregarJugada_ValidaDatosConCanExecute()
    {
        var vm = new TestVentaViewModel(new Mock<SistemaVentas.Services.VentasService>().Object, new Mock<SistemaVentas.Services.SorteosService>().Object, 1)
        {
            NumeroIngresado = "",
            MontoIngresado = 0,
            SorteoSeleccionado = null
        };

        vm.AgregarJugadaCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void EliminarJugada_RemueveDeColeccion()
    {
        var vm = new TestVentaViewModel(new Mock<SistemaVentas.Services.VentasService>().Object, new Mock<SistemaVentas.Services.SorteosService>().Object, 1)
        {
            NumeroIngresado = "10",
            MontoIngresado = 15m,
            SorteoSeleccionado = new SorteoDiario { Id = 1, Nombre = "M" }
        };

        vm.AgregarJugadaCommand.Execute(null);
        var jugada = vm.JugadasAgregadas.First();
        vm.EliminarJugadaCommand.Execute(jugada);

        vm.JugadasAgregadas.Should().BeEmpty();
    }

    [Fact]
    public void TotalVenta_CalculaCorrectamente()
    {
        var vm = new TestVentaViewModel(new Mock<SistemaVentas.Services.VentasService>().Object, new Mock<SistemaVentas.Services.SorteosService>().Object, 1);

        vm.JugadasAgregadas.Add(new DetalleVentaTemporal { Monto = 10m, Numero = "10", Sorteo = "A", SorteoDiarioId = 1 });
        vm.JugadasAgregadas.Add(new DetalleVentaTemporal { Monto = 20m, Numero = "20", Sorteo = "B", SorteoDiarioId = 2 });

        vm.TotalVenta.Should().Be(30m);
    }

    [Fact]
    public async Task GuardarVentaAsync_PersisteDatos()
    {
        var ventasMock = new Mock<SistemaVentas.Services.VentasService>();
        ventasMock.Setup(x => x.CrearVentaAsync(It.IsAny<long>(), It.IsAny<List<(long, string, decimal)>>()))
            .ReturnsAsync((true, "ok", 100));

        var vm = new TestVentaViewModel(ventasMock.Object, new Mock<SistemaVentas.Services.SorteosService>().Object, 9)
        {
            NumeroIngresado = "10",
            MontoIngresado = 25m,
            SorteoSeleccionado = new SorteoDiario { Id = 1, Nombre = "M" }
        };

        vm.AgregarJugadaCommand.Execute(null);
        vm.GuardarVentaCommand.Execute(null);
        await Task.Delay(30);

        ventasMock.Verify(x => x.CrearVentaAsync(9, It.IsAny<List<(long, string, decimal)>>()), Times.Once);
    }
}
