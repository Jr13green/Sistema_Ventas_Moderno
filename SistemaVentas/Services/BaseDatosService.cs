using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SistemaVentas.Models;

namespace SistemaVentas.Services
{
    /// <summary>
    /// Servicio para gestionar conexiones y operaciones básicas de la base de datos SQLite
    /// Patrón: Singleton (una única instancia durante la ejecución)
    /// </summary>
    public class BaseDatosService
    {
        private readonly string _connectionString;
        private const string DbFileName = "SistemaVentas.db";

        public BaseDatosService()
        {
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            string dbPath = System.IO.Path.Combine(appPath, "Data", DbFileName);

            // Crear carpeta Data si no existe
            string dataFolder = System.IO.Path.Combine(appPath, "Data");
            if (!System.IO.Directory.Exists(dataFolder))
                System.IO.Directory.CreateDirectory(dataFolder);

            _connectionString = $"Data Source={dbPath};";
        }

        /// <summary>
        /// Obtiene una nueva conexión a la base de datos
        /// </summary>
        public SqliteConnection ObtenerConexion()
        {
            return new SqliteConnection(_connectionString);
        }

        /// <summary>
        /// Inicializa la base de datos con las tablas necesarias
        /// Se ejecuta una sola vez al iniciar la aplicación
        /// </summary>
        public async Task InicializarBaseDatosAsync()
        {
            using (SqliteConnection conexion = ObtenerConexion())
            {
                await conexion.OpenAsync();

                string sql = @"
                    -- Tabla de usuarios
                    CREATE TABLE IF NOT EXISTS Usuarios (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        NombreCompleto TEXT NOT NULL,
                        NombreUsuario TEXT NOT NULL UNIQUE,
                        Contrasena TEXT NOT NULL,
                        Rol TEXT NOT NULL DEFAULT 'Vendedor',
                        Activo INTEGER NOT NULL DEFAULT 1,
                        FechaCreacion TEXT NOT NULL
                    );

                    -- Tabla de sorteos
                    CREATE TABLE IF NOT EXISTS Sorteos (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Nombre TEXT NOT NULL UNIQUE,
                        Hora TEXT NOT NULL,
                        MinutosCierreAntes INTEGER NOT NULL DEFAULT 30,
                        Activo INTEGER NOT NULL DEFAULT 1,
                        FechaCreacion TEXT NOT NULL
                    );

                    -- Tabla de sorteos diarios
                    CREATE TABLE IF NOT EXISTS SorteosDiarios (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        SorteoId INTEGER NOT NULL,
                        Fecha TEXT NOT NULL,
                        Estado TEXT NOT NULL DEFAULT 'Abierto',
                        Resultado TEXT,
                        FechaResultado TEXT,
                        FOREIGN KEY (SorteoId) REFERENCES Sorteos(Id),
                        UNIQUE(SorteoId, Fecha)
                    );

                    -- Tabla de ventas
                    CREATE TABLE IF NOT EXISTS Ventas (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Codigo TEXT NOT NULL UNIQUE,
                        UsuarioId INTEGER NOT NULL,
                        Total REAL NOT NULL,
                        Estado TEXT NOT NULL DEFAULT 'Activa',
                        FechaCreacion TEXT NOT NULL,
                        FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
                    );

                    -- Tabla de detalles de ventas
                    CREATE TABLE IF NOT EXISTS DetallesVenta (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        VentaId INTEGER NOT NULL,
                        SorteoDiarioId INTEGER NOT NULL,
                        Numero TEXT NOT NULL,
                        Monto REAL NOT NULL,
                        FechaCreacion TEXT NOT NULL,
                        FOREIGN KEY (VentaId) REFERENCES Ventas(Id),
                        FOREIGN KEY (SorteoDiarioId) REFERENCES SorteosDiarios(Id)
                    );

                    -- Tabla de configuración
                    CREATE TABLE IF NOT EXISTS Configuracion (
                        Clave TEXT PRIMARY KEY,
                        Valor TEXT NOT NULL
                    );

                    -- Tabla de auditoría
                    CREATE TABLE IF NOT EXISTS Auditoria (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Accion TEXT NOT NULL,
                        Modulo TEXT NOT NULL,
                        Detalle TEXT NOT NULL,
                        UsuarioId INTEGER,
                        FechaCreacion TEXT NOT NULL,
                        FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
                    );

                    -- Índices para optimización
                    CREATE INDEX IF NOT EXISTS idx_ventas_usuario ON Ventas(UsuarioId);
                    CREATE INDEX IF NOT EXISTS idx_ventas_fecha ON Ventas(FechaCreacion);
                    CREATE INDEX IF NOT EXISTS idx_sorteos_fecha ON SorteosDiarios(Fecha);
                    CREATE INDEX IF NOT EXISTS idx_detalles_venta ON DetallesVenta(VentaId);
                ";

                using (SqliteCommand comando = new SqliteCommand(sql, conexion))
                {
                    await comando.ExecuteNonQueryAsync();
                }

                // Insertar configuraciones por defecto si no existen
                await InsertarConfiguracionesPorDefectoAsync(conexion);
            }
        }

        /// <summary>
        /// Inserta las configuraciones por defecto del sistema
        /// </summary>
        private async Task InsertarConfiguracionesPorDefectoAsync(SqliteConnection conexion)
        {
            string sql = @"
                INSERT OR IGNORE INTO Configuracion (Clave, Valor) VALUES
                ('NombreNegocio', 'Sistema de Ventas Diaria Familiar'),
                ('Propietario', 'Junior Green'),
                ('Moneda', 'L'),
                ('MultiplicadorPremio', '70'),
                ('Telefono', ''),
                ('Direccion', ''),
                ('MensajeBoleto', 'Conserve este comprobante. Gracias por su preferencia.'),
                ('VistaPreviaAutomatica', '1'),
                ('MostrarTelefonoBoleto', '1'),
                ('MostrarVendedorBoleto', '1'),
                ('ConfirmarAnulaciones', '1'),
                ('AuditoriaActiva', '1'),
                ('PermitirReimpresionVendedor', '1'),
                ('RespaldoAutomatico', '1'),
                ('CantidadRespaldos', '15'),
                ('FechaUltimoRespaldoAutomatico', ''),
                ('FechaInicioOperacion', '2026-07-13');
            ";

            using (SqliteCommand comando = new SqliteCommand(sql, conexion))
            {
                await comando.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Ejecuta una query y retorna el resultado como escalar
        /// </summary>
        public async Task<object> ExecuteScalarAsync(string sql, Dictionary<string, object> parametros = null)
        {
            using (SqliteConnection conexion = ObtenerConexion())
            {
                await conexion.OpenAsync();
                using (SqliteCommand comando = new SqliteCommand(sql, conexion))
                {
                    if (parametros != null)
                    {
                        foreach (var param in parametros)
                        {
                            comando.Parameters.AddWithValue("@" + param.Key, param.Value ?? DBNull.Value);
                        }
                    }

                    return await comando.ExecuteScalarAsync();
                }
            }
        }

        /// <summary>
        /// Ejecuta una query sin retornar datos (INSERT, UPDATE, DELETE)
        /// </summary>
        public async Task<int> ExecuteNonQueryAsync(string sql, Dictionary<string, object> parametros = null)
        {
            using (SqliteConnection conexion = ObtenerConexion())
            {
                await conexion.OpenAsync();
                using (SqliteCommand comando = new SqliteCommand(sql, conexion))
                {
                    if (parametros != null)
                    {
                        foreach (var param in parametros)
                        {
                            comando.Parameters.AddWithValue("@" + param.Key, param.Value ?? DBNull.Value);
                        }
                    }

                    return await comando.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Verifica la integridad de la base de datos
        /// </summary>
        public async Task<bool> VerificarIntegridadAsync()
        {
            try
            {
                using (SqliteConnection conexion = ObtenerConexion())
                {
                    await conexion.OpenAsync();
                    using (SqliteCommand comando = new SqliteCommand("PRAGMA integrity_check;", conexion))
                    {
                        object resultado = await comando.ExecuteScalarAsync();
                        return resultado?.ToString() == "ok";
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Optimiza la base de datos
        /// </summary>
        public async Task OptimizarAsync()
        {
            try
            {
                using (SqliteConnection conexion = ObtenerConexion())
                {
                    await conexion.OpenAsync();
                    using (SqliteCommand comando = new SqliteCommand("PRAGMA optimize;", conexion))
                    {
                        await comando.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't throw
                System.Diagnostics.Debug.WriteLine($"Error optimizando BD: {ex.Message}");
            }
        }
    }
}
