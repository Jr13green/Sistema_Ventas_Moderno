using FluentAssertions;
using Moq;
using SistemaVentas.Models;
using SistemaVentas.Services;
using SistemaVentas.Tests.Fixtures;
using System.Windows;

namespace SistemaVentas.Tests.ViewModelTests;

public class UsuariosViewModelTests
{
    [Fact]
    public async Task CrearUsuarioAsync_ValidaDatos()
    {
        var usuarios = new Mock<UsuariosService>(new BaseDatosService());
        usuarios.Setup(x => x.CrearUsuarioAsync("Ana", "11", "Vendedor"))
            .ReturnsAsync(new Usuario { Id = 1, NombreCompleto = "Ana", NombreUsuario = "ana", Numero = "11", Rol = "Vendedor", Activo = true });
        usuarios.Setup(x => x.ObtenerVendedoresActivosAsync()).ReturnsAsync([]);

        var vm = new TestUsuariosViewModel(usuarios.Object, new Mock<AuditoriaService>().Object)
        {
            NombreNuevo = "Ana",
            NumeroNuevo = "11",
            RolSeleccionado = "Vendedor"
        };

        vm.CrearUsuarioCommand.CanExecute(null).Should().BeTrue();
        vm.CrearUsuarioCommand.Execute(null);
        await Task.Delay(40);

        usuarios.Verify(x => x.CrearUsuarioAsync("Ana", "11", "Vendedor"), Times.Once);
    }

    [Fact]
    public async Task EliminarUsuarioAsync_ConConfirmacion()
    {
        var usuarios = new Mock<UsuariosService>(new BaseDatosService());
        usuarios.Setup(x => x.EliminarUsuarioAsync(5)).ReturnsAsync(true);
        usuarios.Setup(x => x.ObtenerVendedoresActivosAsync()).ReturnsAsync([]);

        var vm = new TestUsuariosViewModel(usuarios.Object, new Mock<AuditoriaService>().Object, MessageBoxResult.Yes);
        vm.EliminarUsuarioCommand.Execute(5L);
        await Task.Delay(40);

        usuarios.Verify(x => x.EliminarUsuarioAsync(5), Times.Once);
    }

    [Fact]
    public async Task CargarUsuariosAsync_ObtieneLista()
    {
        var usuarios = new Mock<UsuariosService>(new BaseDatosService());
        usuarios.Setup(x => x.ObtenerVendedoresActivosAsync()).ReturnsAsync(TestData.UsuariosActivosVendedores());

        var vm = new TestUsuariosViewModel(usuarios.Object, new Mock<AuditoriaService>().Object);
        await vm.CargarUsuariosAsync();

        vm.UsuariosActivos.Should().HaveCount(2);
    }
}
