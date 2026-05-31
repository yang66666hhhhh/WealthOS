using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;

namespace WealthOS.Presentation.Services;

public partial class LocalizationService : ObservableObject
{
    [ObservableProperty]
    private bool _isChinese;

    [ObservableProperty]
    private bool _isDarkMode;

    public event Action? LanguageChanged;
    public event Action? ThemeChanged;

    public LocalizationService()
    {
        IsChinese = false;
        IsDarkMode = false;
    }

    public void ToggleLanguage()
    {
        IsChinese = !IsChinese;
        ApplyLanguage();
        LanguageChanged?.Invoke();
    }

    public void ToggleTheme()
    {
        IsDarkMode = !IsDarkMode;
        ApplyTheme();
        ThemeChanged?.Invoke();
    }

    public void ApplyLanguage()
    {
        var app = System.Windows.Application.Current;
        if (app == null) return;

        var dict = new ResourceDictionary
        {
            Source = new Uri(IsChinese
                ? "pack://application:,,,/WealthOS.Presentation;component/Resources/Strings.zh.xaml"
                : "pack://application:,,,/WealthOS.Presentation;component/Resources/Strings.en.xaml")
        };

        var existing = app.Resources.MergedDictionaries
            .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Strings."));

        if (existing != null)
            app.Resources.MergedDictionaries.Remove(existing);

        app.Resources.MergedDictionaries.Add(dict);
    }

    public void ApplyTheme()
    {
        var app = System.Windows.Application.Current;
        if (app == null) return;

        var dict = new ResourceDictionary
        {
            Source = new Uri(IsDarkMode
                ? "pack://application:,,,/WealthOS.Presentation;component/Resources/Theme.Dark.xaml"
                : "pack://application:,,,/WealthOS.Presentation;component/Resources/Theme.xaml")
        };

        var existing = app.Resources.MergedDictionaries
            .FirstOrDefault(d => d.Source != null &&
                (d.Source.OriginalString.Contains("Theme.xaml") ||
                 d.Source.OriginalString.Contains("Theme.Dark.xaml")));

        if (existing != null)
            app.Resources.MergedDictionaries.Remove(existing);

        app.Resources.MergedDictionaries.Insert(0, dict);
    }
}
