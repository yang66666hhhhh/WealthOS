using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;

namespace WealthOS.Presentation.Services;

public partial class LocalizationService : ObservableObject
{
    [ObservableProperty]
    private bool _isChinese;

    public event Action? LanguageChanged;

    public LocalizationService()
    {
        IsChinese = false;
    }

    public void ToggleLanguage()
    {
        IsChinese = !IsChinese;
        ApplyLanguage();
        LanguageChanged?.Invoke();
    }

    public void ApplyLanguage()
    {
        var app = System.Windows.Application.Current;
        if (app == null) return;

        var dict = new ResourceDictionary
        {
            Source = new Uri(IsChinese
                ? "Resources/Strings.zh.xaml"
                : "Resources/Strings.en.xaml", UriKind.Relative)
        };

        var existing = app.Resources.MergedDictionaries
            .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Strings."));

        if (existing != null)
            app.Resources.MergedDictionaries.Remove(existing);

        app.Resources.MergedDictionaries.Add(dict);
    }
}
