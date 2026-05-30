using System.Windows;
using WealthOS.Presentation.ViewModels;

namespace WealthOS.Presentation;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
