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
    /// ViewModel para generación de reportes
    /// </summary>
    public class ReporteViewModel : BaseViewModel
    {
        private readonly ReportesService _reportes;
        private readonly ConfiguracionService _configuracion;

        private DateTime _fechaInicio;
        private DateTime _fechaFin;
        private decimal _totalVentas;
        private decimal _totalPremios;
        private decimal _ganancia;
        private int _totalTransacciones;
        private bool _cargando;
        private ObservableCollection<(string nombre, decimal total, int ventas)> _topVendedores;

        public ICommand GenerarReporteCommand { get; }
        public ICommand ExportarCSVCommand { get; }
        public ICommand LimpiarCommand { get; }

        public ReporteViewModel(
            ReportesService reportes,
            ConfiguracionService configuracion)
        {
            _reportes = reportes;
            _configuracion = configuracion;

            _fechaInicio = DateTime.Today.AddDays(-30);
            _fechaFin = DateTime.Today;
            _topVendedores = new ObservableCollection<(string, decimal, int)>();

            GenerarReporteCommand = new AsyncRelayCommand(GenerarReporteAsync);
            ExportarCSVCommand = new AsyncRelayCommand(ExportarCSVAsync, PuedeExportar);
            LimpiarCommand = new RelayCommand(Limpiar);
        }

        #region Propiedades

        public DateTime FechaInicio
        {
            get => _fechaInicio;
            set => SetProperty(ref _fechaInicio, value);
        }

        public DateTime FechaFin
        {
            get => _fechaFin;
            set => SetProperty(ref _fechaFin, value);
        }

        public decimal TotalVentas
        {
            get => _totalVentas;
            set => SetProperty(ref _totalVentas, value);
        }

        public decimal TotalPremios
        {
            get => _totalPremios;
            set => SetProperty(ref _totalPremios, value);
        }

        public decimal Ganancia
        {
            get => _ganancia;
            set => SetProperty(ref _ganancia, value);
        }

        public int TotalTransacciones
        {
            get => _totalTransacciones;
            set => SetProperty(ref _totalTransacciones, value);
        }

        public bool Cargando
        {
            get => _cargando;
            set => SetProperty(ref _cargando, value);
        }

        public ObservableCollection<(string, decimal, int)> TopVendedores
        {
            get => _topVendedores;
            set => SetProperty(ref _topVendedores, value);
        }

        #endregion

        #region Métodos

        private async Task GenerarReporteAsync()
        {
            try
            {
                Cargando = true;

                var (ventas, premios, ganancia, transacciones) = 
                    await _reportes.ObtenerResumenPeriodoAsync(FechaInicio, FechaFin);

                TotalVentas = ventas;
                TotalPremios = premios;
                Ganancia = ganancia;
                TotalTransacciones = transacciones;

                var vendedores = await _reportes.ObtenerTopVendedoresAsync(FechaInicio, FechaFin, 10);
                TopVendedores = new ObservableCollection<(string, decimal, int)>(vendedores);
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

        private async Task ExportarCSVAsync()
        {
            try
            {
                Cargando = true;
                var csv = await _reportes.ExportarVentasCSVAsync(FechaInicio, FechaFin);
                var rutaArchivo = SeleccionarRutaArchivoCsv();
                if (!string.IsNullOrWhiteSpace(rutaArchivo))
                {
                    GuardarArchivoTexto(rutaArchivo, csv);
                    MostrarMensaje("Archivo exportado correctamente", "Éxito");
                }
            }
            catch (Exception ex)
            {
                ManejarError("Error Exportando", ex);
            }
            finally
            {
                Cargando = false;
            }
        }

        private void Limpiar()
        {
            FechaInicio = DateTime.Today.AddDays(-30);
            FechaFin = DateTime.Today;
            TotalVentas = 0;
            TotalPremios = 0;
            Ganancia = 0;
            TotalTransacciones = 0;
            TopVendedores.Clear();
        }

        private bool PuedeExportar()
        {
            return TotalVentas > 0 && !Cargando;
        }

        protected virtual string SeleccionarRutaArchivoCsv()
        {
            var dialogo = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"Reporte_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                Filter = "CSV Files (*.csv)|*.csv"
            };

            return dialogo.ShowDialog() == true ? dialogo.FileName : string.Empty;
        }

        protected virtual void GuardarArchivoTexto(string ruta, string contenido)
        {
            System.IO.File.WriteAllText(ruta, contenido);
        }

        #endregion
    }
}
