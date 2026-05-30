using CommunityToolkit.Mvvm.ComponentModel;

namespace WealthOS.Presentation.Services;

public partial class NavigationService : ObservableObject
{
    [ObservableProperty]
    private ObservableObject? _currentViewModel;

    public event Action? NavigationChanged;

    public void NavigateTo(ObservableObject viewModel)
    {
        CurrentViewModel = viewModel;
        NavigationChanged?.Invoke();
    }
}
