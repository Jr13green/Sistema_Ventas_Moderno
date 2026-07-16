using System.Reflection;
using SistemaVentas.Models;
using SistemaVentas.Services;
using SistemaVentas.ViewModels;

namespace SistemaVentas.Tests.Fixtures;

public sealed class TestDashboardViewModel : DashboardViewModel
{
    public string? UltimoTitulo { get; private set; }
    public string? UltimoMensaje { get; private set; }

    public TestDashboardViewModel(
        VentasService ventasService,
        SorteosService sorteosService,
        CajaService cajaService,
        ConfiguracionService configuracion,
        ReportesService reportes,
        NotificacionesService notificaciones)
        : base(ventasService, sorteosService, cajaService, configuracion, reportes, notificaciones)
    {
    }

    protected override void MostrarMensaje(string mensaje, string titulo, System.Windows.MessageBoxButton button = System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage icon = System.Windows.MessageBoxImage.None)
    {
        UltimoTitulo = titulo;
        UltimoMensaje = mensaje;
    }
}

public sealed class TestVentaViewModel : VentaViewModel
{
    public string? UltimoTitulo { get; private set; }
    public string? UltimoMensaje { get; private set; }

    public TestVentaViewModel(VentasService ventasService, SorteosService sorteosService, long usuarioId)
        : base(ventasService, sorteosService, usuarioId)
    {
    }

    protected override void MostrarMensaje(string mensaje, string titulo, System.Windows.MessageBoxButton button = System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage icon = System.Windows.MessageBoxImage.None)
    {
        UltimoTitulo = titulo;
        UltimoMensaje = mensaje;
    }
}

public sealed class TestReporteViewModel : ReporteViewModel
{
    private readonly string _ruta;

    public TestReporteViewModel(ReportesService reportes, ConfiguracionService configuracion, string ruta)
        : base(reportes, configuracion)
    {
        _ruta = ruta;
    }

    protected override string SeleccionarRutaArchivoCsv() => _ruta;

    protected override void MostrarMensaje(string mensaje, string titulo, System.Windows.MessageBoxButton button = System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage icon = System.Windows.MessageBoxImage.None)
    {
    }
}

public sealed class TestCajaViewModel : CajaViewModel
{
    public TestCajaViewModel(CajaService caja, AuditoriaService auditoria)
        : base(caja, auditoria)
    {
    }

    protected override void MostrarMensaje(string mensaje, string titulo, System.Windows.MessageBoxButton button = System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage icon = System.Windows.MessageBoxImage.None)
    {
    }
}

public sealed class TestUsuariosViewModel : UsuariosViewModel
{
    private readonly System.Windows.MessageBoxResult _resultadoConfirmacion;

    public TestUsuariosViewModel(UsuariosService usuarios, AuditoriaService auditoria, System.Windows.MessageBoxResult resultadoConfirmacion = System.Windows.MessageBoxResult.Yes)
        : base(usuarios, auditoria)
    {
        _resultadoConfirmacion = resultadoConfirmacion;
    }

    protected override void MostrarMensaje(string mensaje, string titulo, System.Windows.MessageBoxButton button = System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage icon = System.Windows.MessageBoxImage.None)
    {
    }

    protected override System.Windows.MessageBoxResult MostrarConfirmacion(string mensaje, string titulo, System.Windows.MessageBoxButton button = System.Windows.MessageBoxButton.YesNo)
    {
        return _resultadoConfirmacion;
    }
}

public static class UsuariosServiceState
{
    public static void Reset()
    {
        var usuariosField = typeof(UsuariosService).GetField("_usuarios", BindingFlags.NonPublic | BindingFlags.Static);
        var nextIdField = typeof(UsuariosService).GetField("_nextId", BindingFlags.NonPublic | BindingFlags.Static);

        if (usuariosField?.GetValue(null) is List<Usuario> usuarios)
        {
            usuarios.Clear();
        }

        nextIdField?.SetValue(null, 1L);
    }
}
