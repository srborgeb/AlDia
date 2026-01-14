using Microsoft.Maui.Controls;
using System.Globalization;

namespace AlDia.Converters
{
    // Esta clase permite que el XAML convierta el byte[] del modelo en algo que el control <Image> pueda mostrar
    public class ConvertidorBytesAImagen : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte[] datosImagen && datosImagen.Length > 0)
            {
                return ImageSource.FromStream(() => new MemoryStream(datosImagen));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}