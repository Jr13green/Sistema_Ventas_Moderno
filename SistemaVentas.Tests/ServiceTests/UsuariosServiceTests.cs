using FluentAssertions;
using SistemaVentas.Models;
using SistemaVentas.Services;
using SistemaVentas.Tests.Fixtures;

namespace SistemaVentas.Tests.ServiceTests;

public class UsuariosServiceTests
{
    private readonly UsuariosService _service;

    public UsuariosServiceTests()
    {
        UsuariosServiceState.Reset();
        _service = new UsuariosService(new BaseDatosService());
    }

    [Fact]
    public async Task CrearUsuarioAsync_DatosValidos_DebeCrear()
    {
        var (exito, _, usuarioId) = await _service.CrearUsuarioAsync(
            new Usuario { NombreUsuario = "ana", NombreCompleto = "Ana", Numero = "1", Rol = "Vendedor" },
            "pwd");

        exito.Should().BeTrue();
        usuarioId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CrearUsuarioAsync_Duplicado_DebeFallar()
    {
        await _service.CrearUsuarioAsync(new Usuario { NombreUsuario = "ana", NombreCompleto = "Ana", Numero = "1", Rol = "Vendedor" }, "pwd");
        var result = await _service.CrearUsuarioAsync(new Usuario { NombreUsuario = "ana", NombreCompleto = "Ana 2", Numero = "2", Rol = "Vendedor" }, "pwd");

        result.exito.Should().BeFalse();
    }

    [Fact]
    public async Task ObtenerVendedoresActivosAsync_DebeFiltrarPorRolYActivo()
    {
        await _service.CrearUsuarioAsync(new Usuario { NombreUsuario = "ana", NombreCompleto = "Ana", Numero = "1", Rol = "Vendedor" }, "pwd");
        await _service.CrearUsuarioAsync(new Usuario { NombreUsuario = "admin", NombreCompleto = "Admin", Numero = "2", Rol = "Admin" }, "pwd");

        var vendedores = await _service.ObtenerVendedoresActivosAsync();
        vendedores.Should().ContainSingle();
    }

    [Fact]
    public async Task EliminarUsuarioAsync_DebeDesactivarUsuario()
    {
        var (_, _, usuarioId) = await _service.CrearUsuarioAsync(new Usuario { NombreUsuario = "ana", NombreCompleto = "Ana", Numero = "1", Rol = "Vendedor" }, "pwd");
        await _service.EliminarUsuarioAsync(usuarioId);

        var usuario = await _service.ObtenerUsuarioPorIdAsync(usuarioId);
        usuario!.Activo.Should().BeFalse();
    }

    [Fact]
    public async Task CrearUsuarioAsyncSobrecarga_DebeGenerarUsuario()
    {
        var usuario = await _service.CrearUsuarioAsync("Juan Perez", "99", "Vendedor");
        usuario.Should().NotBeNull();
        usuario!.NombreUsuario.Should().Contain("juan.perez");
    }
}
