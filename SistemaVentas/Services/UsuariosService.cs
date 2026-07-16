using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SistemaVentas.Models;

namespace SistemaVentas.Services
{
    /// <summary>
    /// Servicio para gestionar operaciones de usuarios para la capa MVVM.
    /// </summary>
    public class UsuariosService
    {
        private readonly BaseDatosService _baseDatos;
        private static readonly List<Usuario> _usuarios = new();
        private static long _nextId = 1;

        public UsuariosService(BaseDatosService baseDatos)
        {
            _baseDatos = baseDatos;
        }

        public virtual Task<Usuario> AutenticarAsync(string nombreUsuario, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(contrasena))
                return Task.FromResult<Usuario>(null);

            var usuario = _usuarios.FirstOrDefault(u =>
                u.Activo &&
                string.Equals(u.NombreUsuario, nombreUsuario, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(usuario);
        }

        public virtual Task<(bool exito, string mensaje, long usuarioId)> CrearUsuarioAsync(Usuario usuario, string contrasena)
        {
            if (usuario == null || string.IsNullOrWhiteSpace(usuario.NombreUsuario) || string.IsNullOrWhiteSpace(contrasena))
                return Task.FromResult((false, "Datos incompletos", 0L));

            if (_usuarios.Any(u => string.Equals(u.NombreUsuario, usuario.NombreUsuario, StringComparison.OrdinalIgnoreCase)))
                return Task.FromResult((false, "El usuario ya existe", 0L));

            usuario.Id = _nextId++;
            usuario.Activo = true;
            _usuarios.Add(usuario);

            return Task.FromResult((true, "Usuario creado exitosamente", usuario.Id));
        }

        public virtual Task<List<Usuario>> ObtenerVendedoresActivosAsync()
        {
            var usuarios = _usuarios
                .Where(u => u.Activo && string.Equals(u.Rol, "Vendedor", StringComparison.OrdinalIgnoreCase))
                .OrderBy(u => u.NombreCompleto)
                .ToList();

            return Task.FromResult(usuarios);
        }

        public virtual Task<bool> CambiarEstadoUsuarioAsync(long usuarioId, bool activo)
        {
            var usuario = _usuarios.FirstOrDefault(u => u.Id == usuarioId);
            if (usuario == null)
                return Task.FromResult(false);

            usuario.Activo = activo;
            return Task.FromResult(true);
        }

        public virtual Task<Usuario> ObtenerUsuarioPorIdAsync(long usuarioId)
        {
            var usuario = _usuarios.FirstOrDefault(u => u.Id == usuarioId);
            return Task.FromResult(usuario);
        }

        /// <summary>
        /// Sobrecarga MVVM para crear usuario desde formulario.
        /// </summary>
        public virtual async Task<Usuario> CrearUsuarioAsync(string nombreCompleto, string numero, string rol)
        {
            string usuarioBase = (nombreCompleto ?? string.Empty).Trim().ToLower().Replace(" ", ".");
            if (string.IsNullOrWhiteSpace(usuarioBase))
                usuarioBase = "usuario";

            var nuevoUsuario = new Usuario
            {
                NombreCompleto = nombreCompleto ?? string.Empty,
                NombreUsuario = $"{usuarioBase}.{DateTime.Now:HHmmss}",
                Numero = numero ?? string.Empty,
                Rol = string.IsNullOrWhiteSpace(rol) ? "Vendedor" : rol,
                Activo = true
            };

            var (exito, _, usuarioId) = await CrearUsuarioAsync(nuevoUsuario, "Temporal123!");
            if (!exito)
                return null;

            nuevoUsuario.Id = usuarioId;
            return nuevoUsuario;
        }

        /// <summary>
        /// Elimina (desactiva) un usuario.
        /// </summary>
        public virtual Task<bool> EliminarUsuarioAsync(long usuarioId)
        {
            return CambiarEstadoUsuarioAsync(usuarioId, false);
        }
    }
}
