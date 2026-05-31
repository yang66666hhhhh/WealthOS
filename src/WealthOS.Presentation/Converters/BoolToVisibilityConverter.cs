using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WealthOS.Presentation.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            true => Visibility.Visible,
            false => Visibility.Collapsed,
            int i => i != 0 ? Visibility.Visible : Visibility.Collapsed,
            string s when !string.IsNullOrEmpty(s) => Visibility.Visible,
            _ => Visibility.Collapsed
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility.Visible;
    }
}
