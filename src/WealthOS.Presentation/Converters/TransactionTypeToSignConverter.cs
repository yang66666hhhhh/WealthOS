using System.Globalization;
using System.Windows.Data;
using WealthOS.Domain.Enums;

namespace WealthOS.Presentation.Converters;

public class TransactionTypeToSignConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TransactionType type && parameter is decimal amount)
        {
            return type switch
            {
                TransactionType.Income => $"+￥{amount:N2}",
                TransactionType.Expense => $"-￥{amount:N2}",
                TransactionType.Transfer => $"￥{amount:N2}",
                _ => $"￥{amount:N2}"
            };
        }
        return "￥0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
