using FluentAssertions;
using SistemaVentas.Services;
using SistemaVentas.Tests.Fixtures;

namespace SistemaVentas.Tests.IntegrationTests;

public class DashboardIntegrationTests
{
    [Fact]
    public async Task InicializarDashboard_FlujoBasico_DebeCompletar()
    {
        var vm = new TestDashboardViewModel(
            new VentasService(),
            new SorteosService(),
            new CajaService(),
            new ConfiguracionService(),
            new ReportesService(),
            new NotificacionesService());

        await vm.InicializarAsync();

        vm.EstadoSistema.Should().Be("Sistema listo");
        vm.SorteosDia.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task ActualizarDashboard_FlujoBasico_DebeActualizarMetricas()
    {
        var vm = new TestDashboardViewModel(
            new VentasService(),
            new SorteosService(),
            new CajaService(),
            new ConfiguracionService(),
            new ReportesService(),
            new NotificacionesService());

        await vm.ActualizarDatosAsync();

        vm.TotalVentasHoy.Should().Be(0m);
        vm.SaldoCaja.Should().Be(0m);
    }

    [Fact]
    public void CrearVentaCommand_FlujoBasico_DebeEjecutarSinError()
    {
        var vm = new TestDashboardViewModel(
            new VentasService(),
            new SorteosService(),
            new CajaService(),
            new ConfiguracionService(),
            new ReportesService(),
            new NotificacionesService());

        vm.CrearVentaCommand.Execute(null);

        vm.UltimoTitulo.Should().Be("Información");
    }

    [Fact]
    public async Task GenerarReporteCommand_FlujoBasico_DebeMostrarMensaje()
    {
        var vm = new TestDashboardViewModel(
            new VentasService(),
            new SorteosService(),
            new CajaService(),
            new ConfiguracionService(),
            new ReportesService(),
            new NotificacionesService());

        vm.GenerarReporteCommand.Execute(null);
        await Task.Delay(30);

        vm.UltimoTitulo.Should().Be("Reporte");
    }
}
