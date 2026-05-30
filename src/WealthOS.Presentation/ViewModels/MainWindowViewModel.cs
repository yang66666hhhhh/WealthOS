using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WealthOS.Presentation.Services;

namespace WealthOS.Presentation.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly DashboardViewModel _dashboardVm;
    private readonly AssetsViewModel _assetsVm;
    private readonly LiabilitiesViewModel _liabilitiesVm;
    private readonly TransactionsViewModel _transactionsVm;
    private readonly GoalsViewModel _goalsVm;

    public NavigationService Navigation { get; }

    [ObservableProperty]
    private string _currentPage = "Dashboard";

    [ObservableProperty]
    private bool _isDashboardSelected = true;

    [ObservableProperty]
    private bool _isAssetsSelected;

    [ObservableProperty]
    private bool _isLiabilitiesSelected;

    [ObservableProperty]
    private bool _isTransactionsSelected;

    [ObservableProperty]
    private bool _isGoalsSelected;

    public MainWindowViewModel(
        NavigationService navigation,
        DashboardViewModel dashboardVm,
        AssetsViewModel assetsVm,
        LiabilitiesViewModel liabilitiesVm,
        TransactionsViewModel transactionsVm,
        GoalsViewModel goalsVm)
    {
        Navigation = navigation;
        _dashboardVm = dashboardVm;
        _assetsVm = assetsVm;
        _liabilitiesVm = liabilitiesVm;
        _transactionsVm = transactionsVm;
        _goalsVm = goalsVm;

        Navigation.NavigateTo(_dashboardVm);
    }

    [RelayCommand]
    private void NavigateTo(string page)
    {
        IsDashboardSelected = page == "Dashboard";
        IsAssetsSelected = page == "Assets";
        IsLiabilitiesSelected = page == "Liabilities";
        IsTransactionsSelected = page == "Transactions";
        IsGoalsSelected = page == "Goals";

        CurrentPage = page;

        Navigation.NavigateTo(page switch
        {
            "Dashboard" => _dashboardVm,
            "Assets" => _assetsVm,
            "Liabilities" => _liabilitiesVm,
            "Transactions" => _transactionsVm,
            "Goals" => _goalsVm,
            _ => _dashboardVm
        });
    }
}
