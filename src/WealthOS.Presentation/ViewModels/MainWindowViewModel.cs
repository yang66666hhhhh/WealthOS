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
    private readonly BudgetsViewModel _budgetsVm;

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

    [ObservableProperty]
    private bool _isBudgetsSelected;

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
        SettingsViewModel settingsVm,
        BudgetsViewModel budgetsVm)
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
        _budgetsVm = budgetsVm;

        Localization.CurrencyChanged += OnCurrencyChanged;
        Navigation.NavigateTo(_dashboardVm);
    }

    private void OnCurrencyChanged()
    {
        (Navigation.CurrentViewModel as ViewModelBase)?.RefreshCommand?.Execute(null);
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
        IsBudgetsSelected = page == "Budgets";

        CurrentPage = page;

        var target = page switch
        {
            "Dashboard" => (ViewModelBase)_dashboardVm,
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
            "Budgets" => _budgetsVm,
            _ => (ViewModelBase)_dashboardVm
        };

        Navigation.NavigateTo(target);
        (target as ViewModelBase)?.RefreshCommand?.Execute(null);
    }

    [RelayCommand]
    private void ToggleLanguage()
    {
        Localization.ToggleLanguage();
        OnPropertyChanged(nameof(Localization));
    }
}
