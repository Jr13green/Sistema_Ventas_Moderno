using FluentAssertions;
using SistemaVentas.Services;
using SistemaVentas.Tests.Fixtures;

namespace SistemaVentas.Tests.IntegrationTests;

public class CajaIntegrationTests
{
    [Fact]
    public async Task InicializarCaja_DebeCargarSaldo()
    {
        var vm = new TestCajaViewModel(new CajaService(), new AuditoriaService());

        await vm.InicializarAsync();

        vm.SaldoActual.Should().Be(0m);
    }

    [Fact]
    public async Task AgregarAjuste_FlujoBasico_DebeLimpiarCampos()
    {
        var vm = new TestCajaViewModel(new CajaService(), new AuditoriaService())
        {
            MontoAjuste = 25m,
            MotivoAjuste = "Ingreso manual",
            TipoAjuste = "Ingreso"
        };

        vm.AgregarAjusteCommand.Execute(null);
        await Task.Delay(40);

        vm.MontoAjuste.Should().Be(0m);
        vm.MotivoAjuste.Should().BeEmpty();
    }

    [Fact]
    public async Task CargarMovimientos_FlujoBasico_DebeFinalizarSinCarga()
    {
        var vm = new TestCajaViewModel(new CajaService(), new AuditoriaService());

        vm.CargarMovimientosCommand.Execute(null);
        await Task.Delay(30);

        vm.Cargando.Should().BeFalse();
    }
}
