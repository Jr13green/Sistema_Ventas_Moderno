using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SistemaVentas.ViewModels
{
    /// <summary>
    /// Clase base para todos los ViewModels
    /// Implementa INotifyPropertyChanged para actualizar la UI automáticamente
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifica a la UI que una propiedad cambió
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Establece el valor de una propiedad y notifica si cambió
        /// </summary>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Manejo centralizado de errores
        /// </summary>
        protected void ManejarError(string titulo, Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR [{titulo}]: {ex.Message}");
            System.Windows.MessageBox.Show(
                $"Error: {ex.Message}",
                titulo,
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error
            );
        }
    }
}
