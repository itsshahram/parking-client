using System.Globalization;
using System.Windows.Data;




namespace Parking.App;

public class TimeFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            // تبدیل AM و PM به ق.ظ و ب.ظ
            return dateTime.ToString("hh:mm tt", CultureInfo.InvariantCulture)
                .Replace("AM", "ق.ظ")
                .Replace("PM", "ب.ظ");
        }
        if (value is TimeOnly time)
        {
            // تبدیل AM و PM به ق.ظ و ب.ظ
            return time.ToString("hh:mm tt", CultureInfo.InvariantCulture)
                .Replace("AM", "ق.ظ")
                .Replace("PM", "ب.ظ");
        }
        return value?.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}