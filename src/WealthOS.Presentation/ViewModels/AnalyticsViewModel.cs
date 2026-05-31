using System.Collections.ObjectModel;
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

public partial class AnalyticsViewModel : ObservableObject
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
        _ = LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var dashboard = await _dashboardService.GetDashboardAsync();

            var now = DateTime.UtcNow;
            var months = 6;
            var incomeData = new decimal[months];
            var expenseData = new decimal[months];
            var labels = new string[months];

            for (int i = 0; i < months; i++)
            {
                var monthStart = new DateTime(now.Year, now.Month, 1).AddMonths(-(months - 1 - i));
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                var transactions = await _transactionService.GetTransactionsAsync(monthStart, monthEnd);

                incomeData[i] = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
                expenseData[i] = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
                labels[i] = monthStart.ToString("MM/yyyy");
            }

            IncomeExpenseSeries =
            [
                new ColumnSeries<decimal>
                {
                    Values = incomeData,
                    Name = "收入",
                    Fill = new SolidColorPaint(SKColors.Teal),
                    Rx = 4, Ry = 4
                },
                new ColumnSeries<decimal>
                {
                    Values = expenseData,
                    Name = "支出",
                    Fill = new SolidColorPaint(SKColors.Coral),
                    Rx = 4, Ry = 4
                }
            ];

            IncomeExpenseXAxes =
            [
                new Axis
                {
                    Labels = labels,
                    LabelsRotation = 0,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 1 },
                    LabelsPaint = new SolidColorPaint(SKColors.Gray)
                }
            ];

            AssetTrendSeries =
            [
                new PieSeries<decimal> { Values = [dashboard.CashTotal], Name = "现金", Fill = new SolidColorPaint(SKColors.Teal) },
                new PieSeries<decimal> { Values = [dashboard.InvestmentTotal], Name = "投资", Fill = new SolidColorPaint(SKColors.MediumPurple) },
                new PieSeries<decimal> { Values = [dashboard.TotalLiabilities], Name = "负债", Fill = new SolidColorPaint(SKColors.Coral) }
            ];

            OnPropertyChanged(nameof(IncomeExpenseSeries));
            OnPropertyChanged(nameof(IncomeExpenseXAxes));
            OnPropertyChanged(nameof(AssetTrendSeries));
        }
        finally
        {
            IsLoading = false;
        }
    }
}
