using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using WealthOS.Domain.Enums;

namespace WealthOS.Presentation.Converters;

public class TransactionTypeToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is TransactionType type ? type switch
        {
            TransactionType.Income => new SolidColorBrush(Color.FromRgb(76, 175, 80)),
            TransactionType.Expense => new SolidColorBrush(Color.FromRgb(244, 67, 54)),
            TransactionType.Transfer => new SolidColorBrush(Color.FromRgb(33, 150, 243)),
            _ => new SolidColorBrush(Colors.Gray)
        } : new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
