namespace SistemaVentas.Caching
{
    /// <summary>
    /// Claves de caché estandarizadas para evitar strings mágicos.
    /// </summary>
    public static class CacheKeys
    {
        // Dashboard
        public const string TotalVentasHoy        = "dashboard:ventas:hoy";
        public const string SaldoCaja              = "dashboard:caja:saldo";
        public const string NotificacionesConteo   = "dashboard:notificaciones:conteo";
        public const string NombreNegocio          = "config:nombre_negocio";

        // Sorteos
        public const string SorteosDia            = "sorteos:dia:{0}";      // {0} = fecha yyyy-MM-dd
        public const string SorteosActivos        = "sorteos:activos";

        // Ventas
        public const string VentasPorFecha        = "ventas:fecha:{0}";     // {0} = fecha yyyy-MM-dd
        public const string VentasPorUsuario      = "ventas:usuario:{0}";   // {0} = usuarioId
        public const string TotalVentasFecha      = "ventas:total:{0}";     // {0} = fecha yyyy-MM-dd

        // Usuarios
        public const string UsuariosActivos       = "usuarios:activos";
        public const string UsuarioPorId          = "usuarios:id:{0}";      // {0} = id
        public const string UsuarioPorNumero      = "usuarios:numero:{0}";  // {0} = numero

        // Reportes
        public const string ResumenPeriodo        = "reportes:resumen:{0}:{1}"; // {0}=inicio {1}=fin
        public const string TopVendedores         = "reportes:top_vendedores:{0}:{1}";

        // Configuración
        public const string ConfiguracionSistema  = "config:sistema";

        // TTL predeterminados (segundos)
        public const int TtlCorto     = 30;    // 30 segundos - datos muy volátiles
        public const int TtlMedio     = 300;   // 5 minutos   - datos semi-estables
        public const int TtlLargo     = 1800;  // 30 minutos  - datos estables
        public const int TtlMuyLargo  = 86400; // 24 horas    - configuración

        /// <summary>Formatea una clave con parámetros.</summary>
        public static string Format(string template, params object[] args)
            => string.Format(template, args);
    }
}
