using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using WealthOS.Application.DTOs;
using WealthOS.Application.Services;

namespace WealthOS.Presentation.ViewModels;

public partial class ReportsViewModel : ObservableObject
{
    private readonly ReportService _reportService;

    [ObservableProperty]
    private AnnualReportDto? _report;

    [ObservableProperty]
    private int _selectedYear = DateTime.UtcNow.Year;

    [ObservableProperty]
    private bool _isLoading;

    public int[] AvailableYears { get; } = [DateTime.UtcNow.Year, DateTime.UtcNow.Year - 1, DateTime.UtcNow.Year - 2];

    public ReportsViewModel(ReportService reportService)
    {
        _reportService = reportService;
        _ = LoadReportAsync();
    }

    [RelayCommand]
    private async Task LoadReportAsync()
    {
        IsLoading = true;
        try
        {
            Report = await _reportService.GenerateAnnualReportAsync(SelectedYear);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ExportCsvAsync()
    {
        var dialog = new SaveFileDialog
        {
            FileName = $"WealthOS_Transactions_{SelectedYear}",
            DefaultExt = ".csv",
            Filter = "CSV 文件|*.csv"
        };

        if (dialog.ShowDialog() == true)
        {
            var start = new DateTime(SelectedYear, 1, 1);
            var end = new DateTime(SelectedYear, 12, 31, 23, 59, 59);
            await _reportService.ExportTransactionsToCsvAsync(dialog.FileName, start, end);
        }
    }

    [RelayCommand]
    private async Task ChangeYearAsync(object? parameter)
    {
        if (parameter is int year)
            SelectedYear = year;
        else if (parameter is string s && int.TryParse(s, out var parsed))
            SelectedYear = parsed;
        await LoadReportAsync();
    }
}
