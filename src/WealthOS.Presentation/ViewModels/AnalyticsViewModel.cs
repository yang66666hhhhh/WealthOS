using System.Collections.ObjectModel;
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
using WealthOS.Domain.Enums;

namespace WealthOS.Presentation.ViewModels;

public partial class AnalyticsViewModel : ViewModelBase
{
    private readonly DashboardService _dashboardService;
    private readonly TransactionService _transactionService;

    public ISeries[] IncomeExpenseSeries { get; set; } = [];
    public Axis[] IncomeExpenseXAxes { get; set; } = [];
    public ISeries[] AssetTrendSeries { get; set; } = [];

    [ObservableProperty]
    private bool _isLoading;

    public AnalyticsViewModel(DashboardService dashboardService, TransactionService transactionService)
    {
        _dashboardService = dashboardService;
        _transactionService = transactionService;
        SafeInitializeAsync(LoadDataAsync);
    }

    public override IRelayCommand? RefreshCommand => LoadDataCommand;

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        ClearError();
        try {
            var dashboard = await _dashboardService.GetDashboardAsync();

            var now = DateTime.UtcNow;
            var months = 6;
            var incomeData = new decimal[months];
            var expenseData = new decimal[months];
            var labels = new string[months];

            var overallStart = new DateTime(now.Year, now.Month, 1).AddMonths(-(months - 1));
            var overallEnd = new DateTime(now.Year, now.Month, 1).AddMonths(1).AddDays(-1);
            var allTransactions = (await _transactionService.GetTransactionsAsync(overallStart, overallEnd)).ToList();

            for (int i = 0; i < months; i++)
            {
                var monthStart = new DateTime(now.Year, now.Month, 1).AddMonths(-(months - 1 - i));
                var monthEnd = monthStart.AddMonths(1);
                var monthTransactions = allTransactions.Where(t => t.OccurredAt >= monthStart && t.OccurredAt < monthEnd);

                incomeData[i] = monthTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
                expenseData[i] = monthTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
                labels[i] = monthStart.ToString("MM/yyyy");
            }

            IncomeExpenseSeries =
            [
                new ColumnSeries<decimal>
                {
                    Values = incomeData,
                    Name = GetResourceString("Analytics.Income"),
                    Fill = new SolidColorPaint(GetResourceColor("IncomeColor")),
                    Rx = 4, Ry = 4
                },
                new ColumnSeries<decimal>
                {
                    Values = expenseData,
                    Name = GetResourceString("Analytics.Expense"),
                    Fill = new SolidColorPaint(GetResourceColor("ExpenseColor")),
                    Rx = 4, Ry = 4
                }
            ];

            IncomeExpenseXAxes =
            [
                new Axis
                {
                    Labels = labels,
                    LabelsRotation = 0,
                    SeparatorsPaint = new SolidColorPaint(GetResourceColor("BorderColor")) { StrokeThickness = 1 },
                    LabelsPaint = new SolidColorPaint(GetResourceColor("TextSecondaryColor"))
                }
            ];

            AssetTrendSeries =
            [
                new PieSeries<decimal> { Values = [dashboard.CashTotal], Name = GetResourceString("Analytics.Cash"), Fill = new SolidColorPaint(GetResourceColor("IncomeColor")) },
                new PieSeries<decimal> { Values = [dashboard.InvestmentTotal], Name = GetResourceString("Analytics.Investment"), Fill = new SolidColorPaint(GetResourceColor("PrimaryColor")) },
                new PieSeries<decimal> { Values = [dashboard.TotalLiabilities], Name = GetResourceString("Analytics.Liabilities"), Fill = new SolidColorPaint(GetResourceColor("ExpenseColor")) }
            ];

            OnPropertyChanged(nameof(IncomeExpenseSeries));
            OnPropertyChanged(nameof(IncomeExpenseXAxes));
            OnPropertyChanged(nameof(AssetTrendSeries));
        }
        catch (Exception ex)
        {
            SetError(ex);
        }
        finally {
            IsLoading = false;
        }
    }
}
