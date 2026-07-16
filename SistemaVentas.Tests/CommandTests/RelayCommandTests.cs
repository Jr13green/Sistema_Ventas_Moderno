using FluentAssertions;
using SistemaVentas.Commands;

namespace SistemaVentas.Tests.CommandTests;

public class RelayCommandTests
{
    [Fact]
    public void Constructor_NullExecute_DebeLanzarExcepcion()
    {
        Action act = () => _ = new RelayCommand(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CanExecute_SinPredicate_DebeSerTrue()
    {
        var command = new RelayCommand(() => { });
        command.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void CanExecute_ConPredicate_DebeEvaluar()
    {
        var command = new RelayCommand(() => { }, () => false);
        command.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void Execute_DebeInvocarAccion()
    {
        var ejecutado = false;
        var command = new RelayCommand(() => ejecutado = true);

        command.Execute(null);

        ejecutado.Should().BeTrue();
    }
}
