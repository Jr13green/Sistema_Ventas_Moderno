using System.Threading.Tasks;

namespace SistemaVentas.Services
{
    /// <summary>
    /// Servicio base de configuración para binding MVVM.
    /// </summary>
    public class ConfiguracionService
    {
        public Task InicializarCacheAsync() => Task.CompletedTask;

        public string ObtenerNombreNegocio() => "Sistema de Ventas Diaria Familiar";
    }
}
