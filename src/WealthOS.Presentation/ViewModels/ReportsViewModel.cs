using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using WealthOS.Application.DTOs;
using WealthOS.Application.Services;

namespace WealthOS.Presentation.ViewModels;

public partial class ReportsViewModel : ViewModelBase
{
    private readonly ReportService _reportService;

    [ObservableProperty]
    private AnnualReportDto? _report;

    public bool HasReport => Report != null;

    partial void OnReportChanged(AnnualReportDto? value) => OnPropertyChanged(nameof(HasReport));

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

    public override IRelayCommand? RefreshCommand => LoadReportCommand;

    [RelayCommand]
    private async Task LoadReportAsync()
    {
        IsLoading = true;
        ClearError();
        try {
            Report = await _reportService.GenerateAnnualReportAsync(SelectedYear);
        }
        catch (Exception ex)
        {
            SetError(ex);
        }
        finally {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ExportCsvAsync()
    {
        try
        {
            var dialog = new SaveFileDialog
            {
                FileName = $"WealthOS_Transactions_{SelectedYear}",
                DefaultExt = ".csv",
                Filter = GetResourceString("FileDialog.CsvFilter")
            };

            if (dialog.ShowDialog() == true)
            {
                var start = new DateTime(SelectedYear, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var end = new DateTime(SelectedYear, 12, 31, 23, 59, 59, DateTimeKind.Utc);
                await _reportService.ExportTransactionsToCsvAsync(dialog.FileName, start, end);
            }
        }
        catch (Exception ex)
        {
            SetError(ex);
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
