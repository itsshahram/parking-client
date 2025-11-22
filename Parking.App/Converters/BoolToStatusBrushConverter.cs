using System.Globalization;
using Color = System.Windows.Media.Color;

namespace Parking.App.Converters
{
    public class BoolToStatusBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                // فعال = سبز
                if (b)
                    return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#22BB66"));

                // غیرفعال = قرمز
                return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF5555"));
            }

            return System.Windows.Media.Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
