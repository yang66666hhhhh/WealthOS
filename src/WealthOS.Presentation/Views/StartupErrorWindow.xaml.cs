using System.Windows;

namespace WealthOS.Presentation.Views;

public partial class StartupErrorWindow : Window
{
    public StartupErrorWindow(string title, string message, string? stackTrace = null)
    {
        InitializeComponent();
        Title = $"WealthOS - {title}";
        var fullMessage = message;
        if (!string.IsNullOrEmpty(stackTrace))
            fullMessage += $"\n\n{stackTrace}";
        ErrorTextBlock.Text = fullMessage;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
