using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using SistemaVentas.Commands;
using SistemaVentas.Models;
using SistemaVentas.Services;

namespace SistemaVentas.ViewModels
{
    /// <summary>
    /// ViewModel para el Dashboard principal
    /// Gestiona toda la lógica de presentación de la pantalla principal
    /// </summary>
    public class DashboardViewModel : BaseViewModel
    {
        private readonly VentasService _ventasService;
        private readonly SorteosService _sorteosService;
        private readonly CajaService _cajaService;
        private readonly ConfiguracionService _configuracion;
        private readonly ReportesService _reportes;
        private readonly NotificacionesService _notificaciones;

        // Propiedades para el binding
        private decimal _totalVentasHoy;
        private decimal _saldoCaja;
        private int _notificacionesNoLeidas;
        private string _nombreNegocio;
        private string _estadoSistema;
        private bool _cargando;

        // Colecciones
        private ObservableCollection<SorteoDiario> _sorteosDia;
        private ObservableCollection<Notificacion> _notificacionesActivas;

        // Comandos
        public ICommand ActualizarDatosCommand { get; }
        public ICommand CrearVentaCommand { get; }
        public ICommand GenerarReporteCommand { get; }

        public DashboardViewModel(
            VentasService ventasService,
            SorteosService sorteosService,
            CajaService cajaService,
            ConfiguracionService configuracion,
            ReportesService reportes,
            NotificacionesService notificaciones)
        {
            _ventasService = ventasService;
            _sorteosService = sorteosService;
            _cajaService = cajaService;
            _configuracion = configuracion;
            _reportes = reportes;
            _notificaciones = notificaciones;

            // Inicializar colecciones
            _sorteosDia = new ObservableCollection<SorteoDiario>();
            _notificacionesActivas = new ObservableCollection<Notificacion>();

            // Inicializar comandos
            ActualizarDatosCommand = new AsyncRelayCommand(ActualizarDatosAsync);
            CrearVentaCommand = new RelayCommand(() => OnCrearVenta());
            GenerarReporteCommand = new AsyncRelayCommand(GenerarReporteAsync);

            // Valores por defecto
            NombreNegocio = "Sistema de Ventas";
            EstadoSistema = "Iniciando...";
        }

        #region Propiedades

        public decimal TotalVentasHoy
        {
            get => _totalVentasHoy;
            set => SetProperty(ref _totalVentasHoy, value);
        }

        public decimal SaldoCaja
        {
            get => _saldoCaja;
            set => SetProperty(ref _saldoCaja, value);
        }

        public int NotificacionesNoLeidas
        {
            get => _notificacionesNoLeidas;
            set => SetProperty(ref _notificacionesNoLeidas, value);
        }

        public string NombreNegocio
        {
            get => _nombreNegocio;
            set => SetProperty(ref _nombreNegocio, value);
        }

        public string EstadoSistema
        {
            get => _estadoSistema;
            set => SetProperty(ref _estadoSistema, value);
        }

        public bool Cargando
        {
            get => _cargando;
            set => SetProperty(ref _cargando, value);
        }

        public ObservableCollection<SorteoDiario> SorteosDia
        {
            get => _sorteosDia;
            set => SetProperty(ref _sorteosDia, value);
        }

        public ObservableCollection<Notificacion> NotificacionesActivas
        {
            get => _notificacionesActivas;
            set => SetProperty(ref _notificacionesActivas, value);
        }

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Inicializa el Dashboard con los datos del día
        /// </summary>
        public async Task InicializarAsync()
        {
            try
            {
                Cargando = true;
                EstadoSistema = "Cargando datos...";

                // Cargar configuración
                await _configuracion.InicializarCacheAsync();
                NombreNegocio = _configuracion.ObtenerNombreNegocio();

                // Crear sorteos del día
                await _sorteosService.CrearSorteosDelDiaAsync();
                await _sorteosService.ActualizarEstadosSorteosAsync();

                // Cargar datos iniciales
                await ActualizarDatosAsync();

                EstadoSistema = "Sistema listo";
            }
            catch (Exception ex)
            {
                ManejarError("Error Inicializando", ex);
                EstadoSistema = "Error: " + ex.Message;
            }
            finally
            {
                Cargando = false;
            }
        }

        /// <summary>
        /// Actualiza todos los datos del Dashboard
        /// </summary>
        public async Task ActualizarDatosAsync()
        {
            try
            {
                Cargando = true;

                // Obtener datos en paralelo
                var tareasObtenerDatos = Task.WhenAll(
                    ObtenerVentasDelDiaAsync(),
                    ObtenerSaldoCajaAsync(),
                    ObtenerSorteosDelDiaAsync(),
                    ObtenerNotificacionesAsync()
                );

                await tareasObtenerDatos;
            }
            catch (Exception ex)
            {
                ManejarError("Error Actualizando Datos", ex);
            }
            finally
            {
                Cargando = false;
            }
        }

        #endregion

        #region Métodos Privados

        private async Task ObtenerVentasDelDiaAsync()
        {
            try
            {
                TotalVentasHoy = await _ventasService.ObtenerTotalVentasActivasPorFechaAsync(DateTime.Today);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo ventas: {ex.Message}");
            }
        }

        private async Task ObtenerSaldoCajaAsync()
        {
            try
            {
                SaldoCaja = await _cajaService.ObtenerSaldoCajaAsync(DateTime.Today);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo saldo: {ex.Message}");
            }
        }

        private async Task ObtenerSorteosDelDiaAsync()
        {
            try
            {
                var sorteos = await _sorteosService.ObtenerSorteosDiarioAsync(DateTime.Today);
                SorteosDia = new ObservableCollection<SorteoDiario>(sorteos);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo sorteos: {ex.Message}");
            }
        }

        private async Task ObtenerNotificacionesAsync()
        {
            try
            {
                NotificacionesNoLeidas = await _notificaciones.ObtenerConteoNoLeidasAsync();
                var notifs = await _notificaciones.ObtenerNotificacionesActivasAsync(5);
                NotificacionesActivas = new ObservableCollection<Notificacion>(notifs);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo notificaciones: {ex.Message}");
            }
        }

        private void OnCrearVenta()
        {
            // Evento disparado cuando se hace click en "Nueva Venta"
            System.Windows.MessageBox.Show("Abrir ventana de nueva venta");
        }

        private async Task GenerarReporteAsync()
        {
            try
            {
                Cargando = true;
                var (ventas, premios, ganancia, transacciones) = 
                    await _reportes.ObtenerResumenPeriodoAsync(
                        DateTime.Today.AddDays(-30),
                        DateTime.Today
                    );

                var mensaje = $"""Reporte del Último Mes:
                    Ventas: L {ventas:N2}
                    Premios: L {premios:N2}
                    Ganancia: L {ganancia:N2}
                    Transacciones: {transacciones}""";

                System.Windows.MessageBox.Show(mensaje, "Reporte");
            }
            catch (Exception ex)
            {
                ManejarError("Error Generando Reporte", ex);
            }
            finally
            {
                Cargando = false;
            }
        }

        #endregion
    }
}
