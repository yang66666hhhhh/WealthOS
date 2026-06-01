using System.Globalization;
using System.Windows.Data;
using WealthOS.Domain.Enums;

namespace WealthOS.Presentation.Converters;

public class TransactionSignMultiConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[0] is TransactionType type && values[1] is decimal amount)
        {
            var sym = CurrencyConverter.CurrencySymbol;
            return type switch
            {
                TransactionType.Income => $"+{sym}{amount:N2}",
                TransactionType.Expense => $"-{sym}{amount:N2}",
                TransactionType.Transfer => $"{sym}{amount:N2}",
                _ => $"{sym}{amount:N2}"
            };
        }
        return $"{CurrencyConverter.CurrencySymbol}0";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
