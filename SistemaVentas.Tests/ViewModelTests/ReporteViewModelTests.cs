using FluentAssertions;
using Moq;
using SistemaVentas.Services;
using SistemaVentas.Tests.Fixtures;

namespace SistemaVentas.Tests.ViewModelTests;

public class ReporteViewModelTests
{
    [Fact]
    public async Task GenerarReporteAsync_CargaMetricas()
    {
        var reportes = new Mock<ReportesService>();
        reportes.Setup(x => x.ObtenerResumenPeriodoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync((100m, 20m, 80m, 5));
        reportes.Setup(x => x.ObtenerTopVendedoresAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 10))
            .ReturnsAsync([("Ana", 100m, 2)]);

        var vm = new TestReporteViewModel(reportes.Object, new ConfiguracionService(), Path.GetTempFileName());
        vm.GenerarReporteCommand.Execute(null);
        await Task.Delay(30);

        vm.TotalVentas.Should().Be(100m);
        vm.TotalPremios.Should().Be(20m);
        vm.Ganancia.Should().Be(80m);
        vm.TotalTransacciones.Should().Be(5);
    }

    [Fact]
    public async Task ExportarCSVAsync_GeneraArchivo()
    {
        var reportes = new Mock<ReportesService>();
        reportes.Setup(x => x.ExportarVentasCSVAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync("Fecha,Venta,Premio,Ganancia");

        var ruta = Path.Combine(Path.GetTempPath(), $"reporte-{Guid.NewGuid():N}.csv");
        var vm = new TestReporteViewModel(reportes.Object, new ConfiguracionService(), ruta)
        {
            TotalVentas = 1m
        };

        vm.ExportarCSVCommand.Execute(null);
        await Task.Delay(30);

        File.Exists(ruta).Should().BeTrue();
        File.ReadAllText(ruta).Should().Contain("Ganancia");
    }

    [Fact]
    public void FechaInicioFechaFin_PermitenFiltrar()
    {
        var vm = new TestReporteViewModel(new ReportesService(), new ConfiguracionService(), Path.GetTempFileName());
        var inicio = new DateTime(2026, 1, 1);
        var fin = new DateTime(2026, 1, 31);

        vm.FechaInicio = inicio;
        vm.FechaFin = fin;

        vm.FechaInicio.Should().Be(inicio);
        vm.FechaFin.Should().Be(fin);
    }
}
