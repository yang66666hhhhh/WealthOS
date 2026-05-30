using System.Globalization;
using System.Windows.Data;

namespace WealthOS.Presentation.Converters;

public class ProgressToWidthConverter : IValueConverter
{
    public double MaxWidth { get; set; } = 300;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal progress)
        {
            var clamped = Math.Min(progress, 100);
            return (double)(clamped / 100m * (decimal)MaxWidth);
        }
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
