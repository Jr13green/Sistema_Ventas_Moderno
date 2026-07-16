using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaVentas.Services
{
    /// <summary>
    /// Servicio para gestionar la configuración del sistema
    /// Almacena y recupera valores de configuración en la base de datos
    /// </summary>
    public class ConfiguracionService
    {
        private readonly BaseDatosService _baseDatos;
        private readonly Dictionary<string, string> _cachLocal = new();
        private bool _cacheInicializado = false;

        public ConfiguracionService(BaseDatosService baseDatos)
        {
            _baseDatos = baseDatos ?? throw new ArgumentNullException(nameof(baseDatos));
        }

        /// <summary>
        /// Inicializa el caché local de configuraciones
        /// Se ejecuta una sola vez al iniciar la aplicación
        /// </summary>
        public async Task InicializarCacheAsync()
        {
            try
            {
                string sql = "SELECT Clave, Valor FROM Configuracion;";
                
                using (var conexion = _baseDatos.ObtenerConexion())
                {
                    await conexion.OpenAsync();
                    using (var comando = new Microsoft.Data.Sqlite.SqliteCommand(sql, conexion))
                    {
                        using (var lector = await comando.ExecuteReaderAsync())
                        {
                            _cachLocal.Clear();
                            while (await lector.ReadAsync())
                            {
                                _cachLocal[lector.GetString(0)] = lector.GetString(1);
                            }
                        }
                    }
                }

                _cacheInicializado = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error inicializando caché: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene un valor de configuración como string
        /// </summary>
        public string ObtenerTexto(string clave, string valorPorDefecto = "")
        {
            if (!_cacheInicializado)
                return valorPorDefecto;

            return _cachLocal.TryGetValue(clave, out string valor) ? valor : valorPorDefecto;
        }

        /// <summary>
        /// Obtiene un valor de configuración como número decimal
        /// </summary>
        public decimal ObtenerDecimal(string clave, decimal valorPorDefecto = 0)
        {
            string texto = ObtenerTexto(clave);
            return decimal.TryParse(texto, out decimal valor) ? valor : valorPorDefecto;
        }

        /// <summary>
        /// Obtiene un valor de configuración como entero
        /// </summary>
        public int ObtenerEntero(string clave, int valorPorDefecto = 0)
        {
            string texto = ObtenerTexto(clave);
            return int.TryParse(texto, out int valor) ? valor : valorPorDefecto;
        }

        /// <summary>
        /// Obtiene un valor de configuración como booleano
        /// </summary>
        public bool ObtenerBooleano(string clave, bool valorPorDefecto = false)
        {
            string texto = ObtenerTexto(clave);
            if (text.Equals("1", StringComparison.OrdinalIgnoreCase) || 
                texto.Equals("true", StringComparison.OrdinalIgnoreCase))
                return true;

            return valorPorDefecto;
        }

        /// <summary>
        /// Guarda un valor de configuración
        /// </summary>
        public async Task GuardarAsync(string clave, string valor)
        {
            if (string.IsNullOrEmpty(clave))
                return;

            try
            {
                string sql = @"
                    INSERT INTO Configuracion (Clave, Valor)
                    VALUES (@clave, @valor)
                    ON CONFLICT(Clave) DO UPDATE SET Valor = excluded.Valor;
                ";

                var parametros = new Dictionary<string, object>
                {
                    { "clave", clave },
                    { "valor", valor ?? "" }
                };

                await _baseDatos.ExecuteNonQueryAsync(sql, parametros);

                // Actualizar caché
                if (_cacheInicializado)
                    _cachLocal[clave] = valor ?? "";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error guardando configuración: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene el nombre del negocio
        /// </summary>
        public string ObtenerNombreNegocio()
        {
            return ObtenerTexto("NombreNegocio", "Mi Negocio");
        }

        /// <summary>
        /// Obtiene el símbolo de moneda
        /// </summary>
        public string ObtenerMoneda()
        {
            return ObtenerTexto("Moneda", "L");
        }

        /// <summary>
        /// Obtiene el multiplicador de premios
        /// </summary>
        public decimal ObtenerMultiplicadorPremio()
        {
            return ObtenerDecimal("MultiplicadorPremio", 70);
        }

        /// <summary>
        /// Obtiene el número de teléfono del negocio
        /// </summary>
        public string ObtenerTelefono()
        {
            return ObtenerTexto("Telefono", "");
        }

        /// <summary>
        /// Obtiene la dirección del negocio
        /// </summary>
        public string ObtenerDireccion()
        {
            return ObtenerTexto("Direccion", "");
        }

        /// <summary>
        /// Verifica si las reimpresiones de vendedor están habilitadas
        /// </summary>
        public bool PermitirReimpresionVendedor()
        {
            return ObtenerBooleano("PermitirReimpresionVendedor", true);
        }

        /// <summary>
        /// Verifica si la auditoría está activa
        /// </summary>
        public bool AuditoriaActiva()
        {
            return ObtenerBooleano("AuditoriaActiva", true);
        }

        /// <summary>
        /// Verifica si la vista previa automática está habilitada
        /// </summary>
        public bool VistaPreviaAutomatica()
        {
            return ObtenerBooleano("VistaPreviaAutomatica", true);
        }

        /// <summary>
        /// Obtiene la fecha de inicio de operación del sistema
        /// </summary>
        public DateTime ObtenerFechaInicioOperacion()
        {
            string texto = ObtenerTexto("FechaInicioOperacion", "2026-07-13");
            return DateTime.TryParse(texto, out DateTime fecha) ? fecha : new DateTime(2026, 7, 13);
        }

        /// <summary>
        /// Limpia el caché (útil después de cambios en configuración)
        /// </summary>
        public async Task LimpiarCacheAsync()
        {
            _cachLocal.Clear();
            _cacheInicializado = false;
            await InicializarCacheAsync();
        }
    }
}
