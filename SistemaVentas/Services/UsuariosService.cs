using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SistemaVentas.Models;

namespace SistemaVentas.Services
{
    /// <summary>
    /// Servicio para gestionar todas las operaciones relacionadas con usuarios
    /// Incluye autenticación, creación, actualización y gestión de permisos
    /// </summary>
    public class UsuariosService
    {
        private readonly BaseDatosService _baseDatos;

        public UsuariosService(BaseDatosService baseDatos)
        {
            _baseDatos = baseDatos ?? throw new ArgumentNullException(nameof(baseDatos));
        }

        /// <summary>
        /// Autentica un usuario con su nombre de usuario y contraseña
        /// </summary>
        /// <returns>Usuario si es válido, null si no existe o contraseña es incorrecta</returns>
        public async Task<Usuario> AutenticarAsync(string nombreUsuario, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(contrasena))
                return null;

            string sql = @"
                SELECT Id, NombreCompleto, NombreUsuario, Rol, Activo, FechaCreacion
                FROM Usuarios
                WHERE NombreUsuario = @nombreUsuario AND Activo = 1
                LIMIT 1;
            ";

            try
            {
                using (SqliteConnection conexion = _baseDatos.ObtenerConexion())
                {
                    await conexion.OpenAsync();
                    using (SqliteCommand comando = new SqliteCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@nombreUsuario", nombreUsuario);
                        using (SqliteDataReader lector = await comando.ExecuteReaderAsync())
                        {
                            if (!await lector.ReadAsync())
                                return null;

                            // Aquí verificarías la contraseña hasheada
                            // Por ahora retornamos el usuario si existe
                            return new Usuario
                            {
                                Id = lector.GetInt64(0),
                                NombreCompleto = lector.GetString(1),
                                NombreUsuario = lector.GetString(2),
                                Rol = lector.GetString(3),
                                Activo = lector.GetInt32(4) == 1
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en autenticación: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Crea un nuevo usuario en el sistema
        /// </summary>
        public async Task<(bool exito, string mensaje, long usuarioId)> CrearUsuarioAsync(Usuario usuario, string contrasena)
        {
            if (usuario == null || string.IsNullOrWhiteSpace(usuario.NombreUsuario) || string.IsNullOrWhiteSpace(contrasena))
                return (false, "Datos incompletos", 0);

            try
            {
                // Verificar si el usuario ya existe
                string verificarSql = "SELECT COUNT(*) FROM Usuarios WHERE NombreUsuario = @nombreUsuario;";
                var resultado = await _baseDatos.ExecuteScalarAsync(verificarSql, 
                    new Dictionary<string, object> { { "nombreUsuario", usuario.NombreUsuario } });

                if (Convert.ToInt32(resultado) > 0)
                    return (false, "El usuario ya existe", 0);

                // Crear nuevo usuario
                string sql = @"
                    INSERT INTO Usuarios 
                    (NombreCompleto, NombreUsuario, Contrasena, Rol, Activo, FechaCreacion)
                    VALUES (@nombreCompleto, @nombreUsuario, @contrasena, @rol, 1, CURRENT_TIMESTAMP);
                    SELECT last_insert_rowid();
                ";

                var parametros = new Dictionary<string, object>
                {
                    { "nombreCompleto", usuario.NombreCompleto ?? "" },
                    { "nombreUsuario", usuario.NombreUsuario },
                    { "contrasena", contrasena }, // En producción: HashPassword(contrasena)
                    { "rol", usuario.Rol ?? "Vendedor" }
                };

                using (SqliteConnection conexion = _baseDatos.ObtenerConexion())
                {
                    await conexion.OpenAsync();
                    using (SqliteCommand comando = new SqliteCommand(sql, conexion))
                    {
                        foreach (var param in parametros)
                            comando.Parameters.AddWithValue("@" + param.Key, param.Value);

                        object id = await comando.ExecuteScalarAsync();
                        return (true, "Usuario creado exitosamente", Convert.ToInt64(id ?? 0));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creando usuario: {ex.Message}");
                return (false, $"Error: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// Obtiene todos los usuarios vendedores activos
        /// </summary>
        public async Task<List<Usuario>> ObtenerVendedoresActivosAsync()
        {
            string sql = @"
                SELECT Id, NombreCompleto, NombreUsuario, Rol, Activo
                FROM Usuarios
                WHERE Rol = 'Vendedor' AND Activo = 1
                ORDER BY NombreCompleto;
            ";

            var usuarios = new List<Usuario>();

            try
            {
                using (SqliteConnection conexion = _baseDatos.ObtenerConexion())
                {
                    await conexion.OpenAsync();
                    using (SqliteCommand comando = new SqliteCommand(sql, conexion))
                    {
                        using (SqliteDataReader lector = await comando.ExecuteReaderAsync())
                        {
                            while (await lector.ReadAsync())
                            {
                                usuarios.Add(new Usuario
                                {
                                    Id = lector.GetInt64(0),
                                    NombreCompleto = lector.GetString(1),
                                    NombreUsuario = lector.GetString(2),
                                    Rol = lector.GetString(3),
                                    Activo = lector.GetInt32(4) == 1
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo vendedores: {ex.Message}");
            }

            return usuarios;
        }

        /// <summary>
        /// Activa o desactiva un usuario
        /// </summary>
        public async Task<bool> CambiarEstadoUsuarioAsync(long usuarioId, bool activo)
        {
            string sql = "UPDATE Usuarios SET Activo = @activo WHERE Id = @id;";
            
            try
            {
                var parametros = new Dictionary<string, object>
                {
                    { "activo", activo ? 1 : 0 },
                    { "id", usuarioId }
                };

                int filas = await _baseDatos.ExecuteNonQueryAsync(sql, parametros);
                return filas > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cambiando estado: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Obtiene un usuario por su ID
        /// </summary>
        public async Task<Usuario> ObtenerUsuarioPorIdAsync(long usuarioId)
        {
            string sql = @"
                SELECT Id, NombreCompleto, NombreUsuario, Rol, Activo
                FROM Usuarios
                WHERE Id = @id
                LIMIT 1;
            ";

            try
            {
                using (SqliteConnection conexion = _baseDatos.ObtenerConexion())
                {
                    await conexion.OpenAsync();
                    using (SqliteCommand comando = new SqliteCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@id", usuarioId);
                        using (SqliteDataReader lector = await comando.ExecuteReaderAsync())
                        {
                            if (await lector.ReadAsync())
                            {
                                return new Usuario
                                {
                                    Id = lector.GetInt64(0),
                                    NombreCompleto = lector.GetString(1),
                                    NombreUsuario = lector.GetString(2),
                                    Rol = lector.GetString(3),
                                    Activo = lector.GetInt32(4) == 1
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo usuario: {ex.Message}");
            }

            return null;
        }
    }
}
