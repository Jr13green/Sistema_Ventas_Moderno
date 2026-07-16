using FluentAssertions;
using SistemaVentas.Commands;

namespace SistemaVentas.Tests.CommandTests;

public class AsyncRelayCommandTests
{
    [Fact]
    public void Constructor_NullExecute_DebeLanzarExcepcion()
    {
        Action act = () => _ = new AsyncRelayCommand(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CanExecute_CuandoNoEjecuta_DebeSerTrue()
    {
        var command = new AsyncRelayCommand(() => Task.CompletedTask);
        command.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task Execute_DebeEjecutarFuncionAsync()
    {
        var ejecutado = false;
        var command = new AsyncRelayCommand(async () =>
        {
            await Task.Delay(5);
            ejecutado = true;
        });

        command.Execute(null);
        await Task.Delay(30);

        ejecutado.Should().BeTrue();
    }

    [Fact]
    public async Task CanExecute_MientrasEjecuta_DebeSerFalse()
    {
        using var gate = new SemaphoreSlim(0, 1);
        var command = new AsyncRelayCommand(async () => await gate.WaitAsync());

        command.Execute(null);
        await Task.Delay(20);

        command.CanExecute(null).Should().BeFalse();
        gate.Release();
    }
}
