using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

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
            MostrarMensaje(
                $"Error: {ex.Message}",
                titulo,
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }

        protected virtual void MostrarMensaje(
            string mensaje,
            string titulo,
            MessageBoxButton button = MessageBoxButton.OK,
            MessageBoxImage icon = MessageBoxImage.None)
        {
            MessageBox.Show(mensaje, titulo, button, icon);
        }

        protected virtual MessageBoxResult MostrarConfirmacion(
            string mensaje,
            string titulo,
            MessageBoxButton button = MessageBoxButton.YesNo)
        {
            return MessageBox.Show(mensaje, titulo, button);
        }
    }
}
