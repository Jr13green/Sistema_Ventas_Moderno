using SistemaVentas.Models;

namespace SistemaVentas.Tests.Fixtures;

public static class TestData
{
    public static List<SorteoDiario> Sorteos(DateTime? fecha = null)
    {
        var f = fecha ?? DateTime.Today;
        return
        [
            new SorteoDiario { Id = 1, Nombre = "Matutino", Fecha = f, Estado = "Abierto" },
            new SorteoDiario { Id = 2, Nombre = "Vespertino", Fecha = f, Estado = "Abierto" }
        ];
    }

    public static List<Usuario> UsuariosActivosVendedores() =>
    [
        new Usuario { Id = 1, NombreCompleto = "Ana Pérez", NombreUsuario = "ana", Numero = "1111", Rol = "Vendedor", Activo = true },
        new Usuario { Id = 2, NombreCompleto = "Luis Gómez", NombreUsuario = "luis", Numero = "2222", Rol = "Vendedor", Activo = true }
    ];

    public static List<Notificacion> Notificaciones(int cantidad = 2)
    {
        var result = new List<Notificacion>();
        for (var i = 1; i <= cantidad; i++)
        {
            result.Add(new Notificacion { Id = i, Titulo = $"Notif {i}", Mensaje = "Mensaje", Leida = false, Fecha = DateTime.Now });
        }

        return result;
    }
}
