using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkiaSharp;

namespace WealthOS.Presentation.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    public abstract IRelayCommand? RefreshCommand { get; }

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _hasError;

    [RelayCommand]
    protected void ClearError()
    {
        ErrorMessage = null;
        HasError = false;
    }

    protected void SetError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }

    protected void SetError(Exception ex)
    {
        ErrorMessage = ex.Message;
        HasError = true;
    }

    protected void SafeInitializeAsync(Func<Task> action)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"初始化失败: {ex.Message}");
                System.Windows.Application.Current?.Dispatcher.Invoke(() => SetError(ex));
            }
        });
    }

    protected static string GetResourceString(string key)
    {
        var app = System.Windows.Application.Current;
        if (app?.TryFindResource(key) is string value)
            return value;
        return key;
    }

    protected static SKColor GetResourceColor(string key)
    {
        var app = System.Windows.Application.Current;
        if (app?.TryFindResource(key) is SolidColorBrush brush)
        {
            var color = brush.Color;
            return new SKColor(color.R, color.G, color.B, color.A);
        }
        if (app?.TryFindResource(key) is System.Windows.Media.Color mediaColor)
        {
            return new SKColor(mediaColor.R, mediaColor.G, mediaColor.B, mediaColor.A);
        }
        return SKColors.Gray;
    }
}
