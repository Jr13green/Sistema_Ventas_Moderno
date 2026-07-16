using System.Windows;
using SistemaVentas.ViewModels;
using SistemaVentas.Services;

namespace SistemaVentas.Views
{
    public partial class UsuariosWindow : Window
    {
        public UsuariosWindow(
            UsuariosService usuarios,
            AuditoriaService auditoria)
        {
            InitializeComponent();

            var viewModel = new UsuariosViewModel(usuarios, auditoria);
            this.DataContext = viewModel;

            this.Loaded += async (s, e) => await viewModel.CargarUsuariosAsync();
        }
    }
}
