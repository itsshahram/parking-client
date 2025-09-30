    using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Parking.App.Converters;

public class EnumDisplayNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return string.Empty;

        var type = value.GetType();
        var member = type.GetMember(value.ToString());
        if (member.Length > 0)
        {
            var displayAttr = member[0].GetCustomAttribute<DisplayAttribute>();
            if (displayAttr != null)
                return displayAttr.Name;
        }

        return value.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
