using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using WealthOS.Application.DTOs;
using WealthOS.Application.Services;
using WealthOS.Presentation.Converters;
using WealthOS.Presentation.Services;

namespace WealthOS.Presentation.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly DashboardService _service;
    private readonly NavigationService _navigation;
    private readonly TransactionsViewModel _transactionsVm;
    private readonly ReportsViewModel _reportsVm;
    private readonly AssetsViewModel _assetsVm;
    private readonly AnalyticsViewModel _analyticsVm;

    [ObservableProperty]
    private DashboardDto _dashboard = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _selectedTimeRange;

    public string CurrentDate => DateTime.Now.ToString("yyyy-MM-dd dddd");

    public ISeries[] AssetSeries { get; set; } = [];
    public ISeries[] NetWorthSeries { get; set; } = [];
    public Axis[] NetWorthXAxes { get; set; } = [];
    public Axis[] NetWorthYAxes { get; set; } = [];

    public DashboardViewModel(
        DashboardService service,
        NavigationService navigation,
        TransactionsViewModel transactionsVm,
        ReportsViewModel reportsVm,
        AssetsViewModel assetsVm,
        AnalyticsViewModel analyticsVm)
    {
        _service = service;
        _navigation = navigation;
        _transactionsVm = transactionsVm;
        _reportsVm = reportsVm;
        _assetsVm = assetsVm;
        _analyticsVm = analyticsVm;
        SafeInitializeAsync(LoadDataAsync);
    }

    public override IRelayCommand? RefreshCommand => LoadDataCommand;

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        ClearError();
        try
        {
            var days = SelectedTimeRange switch
            {
                0 => 30,
                1 => 90,
                2 => 365,
                3 => 3650,
                _ => 90
            };
            Dashboard = await _service.GetDashboardAsync(days);
            UpdateCharts();
        }
        catch (Exception ex)
        {
            SetError(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SetTimeRangeAsync(string range)
    {
        SelectedTimeRange = range switch
        {
            "30d" => 0,
            "90d" => 1,
            "1y" => 2,
            "all" => 3,
            _ => 1
        };
        await LoadDataAsync();
    }

    [RelayCommand]
    private void NavigateToTransactions()
    {
        _navigation.NavigateTo(_transactionsVm);
    }

    [RelayCommand]
    private void AddTransaction()
    {
        _navigation.NavigateTo(_transactionsVm);
        _transactionsVm.ShowAddDialogCommand.Execute(null);
    }

    [RelayCommand]
    private void NavigateToReports()
    {
        _navigation.NavigateTo(_reportsVm);
    }

    [RelayCommand]
    private void NavigateToAssets()
    {
        _navigation.NavigateTo(_assetsVm);
    }

    [RelayCommand]
    private void NavigateToAnalytics()
    {
        _navigation.NavigateTo(_analyticsVm);
    }

    private void UpdateCharts()
    {
        var primaryColor = GetResourceColor("PrimaryColor");
        var accentColor = GetResourceColor("AccentColor");
        var incomeColor = GetResourceColor("IncomeColor");
        var expenseColor = GetResourceColor("ExpenseColor");
        var transferColor = GetResourceColor("TransferColor");
        var warningColor = GetResourceColor("WarningColor");

        var colors = new SKColor[]
        {
            incomeColor,
            primaryColor,
            warningColor,
            accentColor,
            transferColor,
            expenseColor
        };

        AssetSeries = Dashboard.AssetAllocations.Select((a, i) =>
            new PieSeries<decimal>
            {
                Values = [a.Value],
                Name = a.Name,
                Fill = new SolidColorPaint(colors[i % colors.Length]),
                InnerRadius = 60,
                DataLabelsSize = 12,
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
                DataLabelsFormatter = p => $"{a.Name}: {a.Percentage:F1}%"
            }).ToArray();

        if (Dashboard.NetWorthHistory.Count > 0)
        {
            NetWorthSeries =
            [
                new LineSeries<decimal>
                {
                    Values = Dashboard.NetWorthHistory.Select(p => p.Value).ToArray(),
                    Fill = new SolidColorPaint(primaryColor.WithAlpha(50)),
                    Stroke = new SolidColorPaint(primaryColor) { StrokeThickness = 2 },
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 0.5
                }
            ];

            var borderColor = GetResourceColor("BorderColor");
            var textSecondaryColor = GetResourceColor("TextSecondaryColor");

            NetWorthXAxes =
            [
                new Axis
                {
                    Labels = Dashboard.NetWorthHistory.Select(p => p.Date.ToString("MM/dd")).ToArray(),
                    LabelsRotation = 0,
                    SeparatorsPaint = new SolidColorPaint(borderColor) { StrokeThickness = 1 },
                    LabelsPaint = new SolidColorPaint(textSecondaryColor)
                }
            ];

            NetWorthYAxes =
            [
                new Axis
                {
                    Labeler = value => value switch
                    {
                        >= 100_000_000 => $"{CurrencyConverter.CurrencySymbol}{string.Format(GetResourceString("Unit.Yi"), value / 100_000_000)}",
                        >= 10_000 => $"{CurrencyConverter.CurrencySymbol}{string.Format(GetResourceString("Unit.Wan"), value / 10_000)}",
                        _ => $"{CurrencyConverter.CurrencySymbol}{value:N0}"
                    },
                    SeparatorsPaint = new SolidColorPaint(borderColor) { StrokeThickness = 1 },
                    LabelsPaint = new SolidColorPaint(textSecondaryColor)
                }
            ];
        }

        OnPropertyChanged(nameof(AssetSeries));
        OnPropertyChanged(nameof(NetWorthSeries));
        OnPropertyChanged(nameof(NetWorthXAxes));
        OnPropertyChanged(nameof(NetWorthYAxes));
    }
}
