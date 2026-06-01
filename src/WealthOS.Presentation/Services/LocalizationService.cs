using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;

namespace WealthOS.Presentation.Services;

public partial class LocalizationService : ObservableObject
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WealthOS", "settings.json");

    [ObservableProperty]
    private bool _isChinese;

    [ObservableProperty]
    private bool _isDarkMode;

    public event Action? LanguageChanged;
    public event Action? ThemeChanged;

    public LocalizationService()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                var settings = JsonSerializer.Deserialize<SettingsData>(json);
                if (settings != null)
                {
                    IsChinese = settings.IsChinese;
                    IsDarkMode = settings.IsDarkMode;
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
        }

        IsChinese = true;
        IsDarkMode = false;
    }

    private void SaveSettings()
    {
        try
        {
            var directory = Path.GetDirectoryName(SettingsPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var settings = new SettingsData
            {
                IsChinese = IsChinese,
                IsDarkMode = IsDarkMode
            };
            var json = JsonSerializer.Serialize(settings);
            File.WriteAllText(SettingsPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
        }
    }

    public void ToggleLanguage()
    {
        IsChinese = !IsChinese;
        ApplyLanguage();
        SaveSettings();
        LanguageChanged?.Invoke();
    }

    public void ToggleTheme()
    {
        IsDarkMode = !IsDarkMode;
        ApplyTheme();
        SaveSettings();
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

    private class SettingsData
    {
        public bool IsChinese { get; set; } = true;
        public bool IsDarkMode { get; set; }
    }
}
