using FluentAssertions;
using Moq;
using SistemaVentas.Services;
using SistemaVentas.Tests.Fixtures;

namespace SistemaVentas.Tests.ViewModelTests;

public class CajaViewModelTests
{
    [Fact]
    public async Task AgregarAjusteAsync_RegistraMovimiento()
    {
        var caja = new Mock<CajaService>();
        var auditoria = new Mock<AuditoriaService>();
        caja.Setup(x => x.ObtenerSaldoCajaAsync(It.IsAny<DateTime>())).ReturnsAsync(50m);
        caja.Setup(x => x.ObtenerResumenCajaPorFechaAsync(It.IsAny<DateTime>())).ReturnsAsync((100m, 50m, 50m));

        var vm = new TestCajaViewModel(caja.Object, auditoria.Object)
        {
            MontoAjuste = 10m,
            MotivoAjuste = "Ajuste",
            TipoAjuste = "Ingreso"
        };

        vm.AgregarAjusteCommand.Execute(null);
        await Task.Delay(40);

        caja.Verify(x => x.RegistrarAjusteAsync("Ingreso", 10m, "Ajuste"), Times.Once);
    }

    [Fact]
    public async Task SaldoActual_ActualizaEnVivo()
    {
        var caja = new Mock<CajaService>();
        caja.Setup(x => x.ObtenerResumenCajaPorFechaAsync(It.IsAny<DateTime>())).ReturnsAsync((100m, 30m, 70m));

        var vm = new TestCajaViewModel(caja.Object, new Mock<AuditoriaService>().Object);

        vm.CargarMovimientosCommand.Execute(null);
        await Task.Delay(30);

        vm.SaldoActual.Should().Be(70m);
    }

    [Fact]
    public async Task CargarMovimientosAsync_ObtieneHistorialResumen()
    {
        var caja = new Mock<CajaService>();
        caja.Setup(x => x.ObtenerResumenCajaPorFechaAsync(It.IsAny<DateTime>())).ReturnsAsync((80m, 20m, 60m));

        var vm = new TestCajaViewModel(caja.Object, new Mock<AuditoriaService>().Object);
        await vm.InicializarAsync();

        caja.Verify(x => x.ObtenerResumenCajaPorFechaAsync(It.IsAny<DateTime>()), Times.Once);
    }
}
