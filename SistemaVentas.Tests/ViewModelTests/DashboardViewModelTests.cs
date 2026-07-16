using FluentAssertions;
using Moq;
using SistemaVentas.Models;
using SistemaVentas.Tests.Fixtures;

namespace SistemaVentas.Tests.ViewModelTests;

public class DashboardViewModelTests
{
    [Fact]
    public async Task InicializarAsync_CargaDatosCorrectamente()
    {
        var mocks = new ServiceMocks();
        mocks.Configuracion.Setup(x => x.ObtenerNombreNegocio()).Returns("Mi Negocio");
        mocks.Sorteos.Setup(x => x.ObtenerSorteosDiarioAsync(It.IsAny<DateTime>())).ReturnsAsync(TestData.Sorteos());
        mocks.Notificaciones.Setup(x => x.ObtenerNotificacionesActivasAsync(5)).ReturnsAsync(TestData.Notificaciones());

        var vm = new TestDashboardViewModel(mocks.Ventas.Object, mocks.Sorteos.Object, mocks.Caja.Object, mocks.Configuracion.Object, mocks.Reportes.Object, mocks.Notificaciones.Object);

        await vm.InicializarAsync();

        vm.NombreNegocio.Should().Be("Mi Negocio");
        vm.EstadoSistema.Should().Be("Sistema listo");
        vm.SorteosDia.Should().HaveCount(2);
    }

    [Fact]
    public async Task ActualizarDatosAsync_ActualizaPropiedades()
    {
        var mocks = new ServiceMocks();
        mocks.Ventas.Setup(x => x.ObtenerTotalVentasActivasPorFechaAsync(It.IsAny<DateTime>())).ReturnsAsync(150m);
        mocks.Caja.Setup(x => x.ObtenerSaldoCajaAsync(It.IsAny<DateTime>())).ReturnsAsync(80m);
        mocks.Sorteos.Setup(x => x.ObtenerSorteosDiarioAsync(It.IsAny<DateTime>())).ReturnsAsync(TestData.Sorteos());
        mocks.Notificaciones.Setup(x => x.ObtenerConteoNoLeidasAsync()).ReturnsAsync(3);
        mocks.Notificaciones.Setup(x => x.ObtenerNotificacionesActivasAsync(5)).ReturnsAsync(TestData.Notificaciones(3));

        var vm = new TestDashboardViewModel(mocks.Ventas.Object, mocks.Sorteos.Object, mocks.Caja.Object, mocks.Configuracion.Object, mocks.Reportes.Object, mocks.Notificaciones.Object);

        await vm.ActualizarDatosAsync();

        vm.TotalVentasHoy.Should().Be(150m);
        vm.SaldoCaja.Should().Be(80m);
        vm.NotificacionesNoLeidas.Should().Be(3);
        vm.NotificacionesActivas.Should().HaveCount(3);
    }

    [Fact]
    public async Task PropertyChanged_NotificaCambios()
    {
        var mocks = new ServiceMocks();
        var vm = new TestDashboardViewModel(mocks.Ventas.Object, mocks.Sorteos.Object, mocks.Caja.Object, mocks.Configuracion.Object, mocks.Reportes.Object, mocks.Notificaciones.Object);
        string? propiedad = null;
        vm.PropertyChanged += (_, e) => propiedad = e.PropertyName;

        vm.TotalVentasHoy = 10m;

        propiedad.Should().Be(nameof(vm.TotalVentasHoy));
    }

    [Fact]
    public async Task Cargando_PrevieneOperacionesParalelas()
    {
        var mocks = new ServiceMocks();
        var vm = new TestDashboardViewModel(mocks.Ventas.Object, mocks.Sorteos.Object, mocks.Caja.Object, mocks.Configuracion.Object, mocks.Reportes.Object, mocks.Notificaciones.Object)
        {
            Cargando = true
        };

        vm.ActualizarDatosCommand.CanExecute(null).Should().BeFalse();
    }
}
