using System.Windows;
using SistemaVentas.ViewModels;
using SistemaVentas.Services;

namespace SistemaVentas.Views
{
    public partial class ReporteWindow : Window
    {
        public ReporteWindow(
            ReportesService reportes,
            ConfiguracionService configuracion)
        {
            InitializeComponent();

            var viewModel = new ReporteViewModel(reportes, configuracion);
            this.DataContext = viewModel;
        }
    }
}
