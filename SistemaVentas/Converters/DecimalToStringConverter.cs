using System;
using System.Globalization;
using System.Windows.Data;

namespace SistemaVentas.Converters
{
    public class DecimalToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal dec)
                return dec.ToString("N2");
            return "0.00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (decimal.TryParse((string)value, out decimal result))
                return result;
            return 0m;
        }
    }
}
