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
    private readonly AccountsViewModel _accountsVm;
    private readonly InvestmentsViewModel _investmentsVm;
    private readonly FixedAssetsViewModel _fixedAssetsVm;
    private readonly AnalyticsViewModel _analyticsVm;
    private readonly TimelineViewModel _timelineVm;
    private readonly ReportsViewModel _reportsVm;
    private readonly SettingsViewModel _settingsVm;

    public NavigationService Navigation { get; }
    public LocalizationService Localization { get; }

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

    [ObservableProperty]
    private bool _isAccountsSelected;

    [ObservableProperty]
    private bool _isInvestmentsSelected;

    [ObservableProperty]
    private bool _isFixedAssetsSelected;

    [ObservableProperty]
    private bool _isAnalyticsSelected;

    [ObservableProperty]
    private bool _isTimelineSelected;

    [ObservableProperty]
    private bool _isReportsSelected;

    [ObservableProperty]
    private bool _isSettingsSelected;

    public MainWindowViewModel(
        NavigationService navigation,
        LocalizationService localization,
        DashboardViewModel dashboardVm,
        AssetsViewModel assetsVm,
        LiabilitiesViewModel liabilitiesVm,
        TransactionsViewModel transactionsVm,
        GoalsViewModel goalsVm,
        AccountsViewModel accountsVm,
        InvestmentsViewModel investmentsVm,
        FixedAssetsViewModel fixedAssetsVm,
        AnalyticsViewModel analyticsVm,
        TimelineViewModel timelineVm,
        ReportsViewModel reportsVm,
        SettingsViewModel settingsVm)
    {
        Navigation = navigation;
        Localization = localization;
        _dashboardVm = dashboardVm;
        _assetsVm = assetsVm;
        _liabilitiesVm = liabilitiesVm;
        _transactionsVm = transactionsVm;
        _goalsVm = goalsVm;
        _accountsVm = accountsVm;
        _investmentsVm = investmentsVm;
        _fixedAssetsVm = fixedAssetsVm;
        _analyticsVm = analyticsVm;
        _timelineVm = timelineVm;
        _reportsVm = reportsVm;
        _settingsVm = settingsVm;

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
        IsAccountsSelected = page == "Accounts";
        IsInvestmentsSelected = page == "Investments";
        IsFixedAssetsSelected = page == "FixedAssets";
        IsAnalyticsSelected = page == "Analytics";
        IsTimelineSelected = page == "Timeline";
        IsReportsSelected = page == "Reports";
        IsSettingsSelected = page == "Settings";

        CurrentPage = page;

        var target = page switch
        {
            "Dashboard" => (ObservableObject)_dashboardVm,
            "Assets" => _assetsVm,
            "Liabilities" => _liabilitiesVm,
            "Transactions" => _transactionsVm,
            "Goals" => _goalsVm,
            "Accounts" => _accountsVm,
            "Investments" => _investmentsVm,
            "FixedAssets" => _fixedAssetsVm,
            "Analytics" => _analyticsVm,
            "Timeline" => _timelineVm,
            "Reports" => _reportsVm,
            "Settings" => _settingsVm,
            _ => (ObservableObject)_dashboardVm
        };

        Navigation.NavigateTo(target);
        RefreshPageData(target);
    }

    [RelayCommand]
    private void ToggleLanguage()
    {
        Localization.ToggleLanguage();
        OnPropertyChanged(nameof(Localization));
    }

    private static void RefreshPageData(ObservableObject vm)
    {
        if (vm is DashboardViewModel dash) _ = dash.LoadDataCommand.ExecuteAsync(null);
        else if (vm is AssetsViewModel assets) _ = assets.LoadDataCommand.ExecuteAsync(null);
        else if (vm is LiabilitiesViewModel liab) _ = liab.LoadDataCommand.ExecuteAsync(null);
        else if (vm is TransactionsViewModel trans) _ = trans.LoadDataCommand.ExecuteAsync(null);
        else if (vm is GoalsViewModel goals) _ = goals.LoadDataCommand.ExecuteAsync(null);
        else if (vm is AccountsViewModel accounts) _ = accounts.LoadDataCommand.ExecuteAsync(null);
        else if (vm is InvestmentsViewModel investments) _ = investments.LoadDataCommand.ExecuteAsync(null);
        else if (vm is FixedAssetsViewModel fixedAssets) _ = fixedAssets.LoadDataCommand.ExecuteAsync(null);
        else if (vm is AnalyticsViewModel analytics) _ = analytics.LoadDataCommand.ExecuteAsync(null);
        else if (vm is TimelineViewModel timeline) _ = timeline.LoadDataCommand.ExecuteAsync(null);
        else if (vm is ReportsViewModel reports) _ = reports.LoadReportCommand.ExecuteAsync(null);
    }
}
