using System.Globalization;
using System.Windows.Data;

namespace WealthOS.Presentation.Converters;

public class CurrencyConverter : IValueConverter
{
    public static string CurrencySymbol { get; set; } = "￥";

    public static string[] AllSymbols { get; } = ["￥", "$", "€", "£", "¥"];

    private static string FormatUnit(string key, decimal value)
    {
        try
        {
            if (System.Windows.Application.Current?.TryFindResource(key) is string fmt && fmt.Contains("{0"))
                return string.Format(fmt, value);
        }
        catch { }
        return $"{CurrencySymbol}{value:N2}";
    }

    private static string GetUnitSuffix(string key)
    {
        try
        {
            if (System.Windows.Application.Current?.TryFindResource(key) is string fmt)
                return fmt.Replace("{0:N1}", "").Replace("{0:N2}", "").Trim();
        }
        catch { }
        return "";
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal amount)
        {
            return amount switch
            {
                >= 100_000_000 => FormatUnit("Unit.Yi", amount / 100_000_000),
                >= 10_000 => FormatUnit("Unit.Wan", amount / 10_000),
                _ => $"{CurrencySymbol}{amount:N2}"
            };
        }
        return $"{CurrencySymbol}0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string s)
        {
            foreach (var symbol in AllSymbols)
                s = s.Replace(symbol, "");
            s = s.Replace(GetUnitSuffix("Unit.Yi"), "").Replace(GetUnitSuffix("Unit.Wan"), "").Trim();
            if (decimal.TryParse(s, out var result))
                return result;
        }
        return 0m;
    }
}
