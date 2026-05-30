using System.Globalization;
using System.Windows.Data;

namespace WealthOS.Presentation.Converters;

public class CurrencyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal amount)
        {
            return amount switch
            {
                >= 100_000_000 => $"￥{amount / 100_000_000:N2}亿",
                >= 10_000 => $"￥{amount / 10_000:N2}万",
                _ => $"￥{amount:N2}"
            };
        }
        return "￥0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string s)
        {
            s = s.Replace("￥", "").Replace("亿", "").Replace("万", "").Trim();
            if (decimal.TryParse(s, out var result))
                return result;
        }
        return 0m;
    }
}
