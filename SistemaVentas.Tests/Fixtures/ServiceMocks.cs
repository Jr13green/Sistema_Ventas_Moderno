using Moq;
using SistemaVentas.Services;

namespace SistemaVentas.Tests.Fixtures;

public sealed class ServiceMocks
{
    public Mock<VentasService> Ventas { get; } = new();
    public Mock<SorteosService> Sorteos { get; } = new();
    public Mock<CajaService> Caja { get; } = new();
    public Mock<ConfiguracionService> Configuracion { get; } = new();
    public Mock<ReportesService> Reportes { get; } = new();
    public Mock<NotificacionesService> Notificaciones { get; } = new();
    public Mock<AuditoriaService> Auditoria { get; } = new();

    public Mock<UsuariosService> Usuarios { get; }

    public ServiceMocks()
    {
        Usuarios = new Mock<UsuariosService>(new BaseDatosService());
    }
}
