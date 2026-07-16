using System.Windows;
using SistemaVentas.ViewModels;
using SistemaVentas.Services;

namespace SistemaVentas.Views
{
    public partial class CajaWindow : Window
    {
        public CajaWindow(
            CajaService caja,
            AuditoriaService auditoria)
        {
            InitializeComponent();

            var viewModel = new CajaViewModel(caja, auditoria);
            this.DataContext = viewModel;

            this.Loaded += async (s, e) => await viewModel.InicializarAsync();
        }
    }
}
