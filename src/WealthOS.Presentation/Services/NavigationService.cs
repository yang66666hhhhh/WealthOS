using CommunityToolkit.Mvvm.ComponentModel;
using WealthOS.Presentation.ViewModels;

namespace WealthOS.Presentation.Services;

public partial class NavigationService : ObservableObject
{
    [ObservableProperty]
    private ObservableObject? _currentViewModel;

    public event Action? NavigationChanged;

    public void NavigateTo(ViewModelBase viewModel)
    {
        CurrentViewModel = viewModel;
        NavigationChanged?.Invoke();
    }
}
