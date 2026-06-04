using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using WealthOS.Application.DTOs;
using WealthOS.Application.Services;

namespace WealthOS.Presentation.ViewModels;

public record YearButton(int Year, bool IsSelected);

public partial class ReportsViewModel : ViewModelBase
{
    private readonly ReportService _reportService;
    private readonly TransactionService _transactionService;
    private readonly AccountService _accountService;

    [ObservableProperty]
    private AnnualReportDto? _report;

    public bool HasReport => Report != null;

    partial void OnReportChanged(AnnualReportDto? value) => OnPropertyChanged(nameof(HasReport));

    [ObservableProperty]
    private int _selectedYear = DateTime.UtcNow.Year;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isImportDialogOpen;

    [ObservableProperty]
    private ObservableCollection<AccountDto> _accounts = [];

    [ObservableProperty]
    private AccountDto? _selectedAccount;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public int[] AvailableYears { get; } = [DateTime.UtcNow.Year, DateTime.UtcNow.Year - 1, DateTime.UtcNow.Year - 2];

    public List<YearButton> YearButtons => AvailableYears.Select(y => new YearButton(y, y == SelectedYear)).ToList();

    partial void OnSelectedYearChanged(int value) => OnPropertyChanged(nameof(YearButtons));

    public ReportsViewModel(ReportService reportService, TransactionService transactionService, AccountService accountService)
    {
        _reportService = reportService;
        _transactionService = transactionService;
        _accountService = accountService;
        SafeInitializeAsync(LoadReportAsync);
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
    private async Task ExportPdfAsync()
    {
        try
        {
            var dialog = new SaveFileDialog
            {
                FileName = $"WealthOS_Report_{SelectedYear}",
                DefaultExt = ".pdf",
                Filter = "PDF Files|*.pdf"
            };

            if (dialog.ShowDialog() == true)
            {
                IsLoading = true;
                await _reportService.ExportAnnualReportToPdfAsync(dialog.FileName, SelectedYear, Converters.CurrencyConverter.CurrencySymbol);
                StatusMessage = string.Format(GetResourceString("Reports.PdfExportSuccess"), SelectedYear);
            }
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
    private async Task ShowImportDialogAsync()
    {
        try
        {
            var accountList = await _accountService.GetAllAccountsAsync();
            Accounts = new ObservableCollection<AccountDto>(accountList);
            SelectedAccount = Accounts.FirstOrDefault();
            StatusMessage = string.Empty;
            IsImportDialogOpen = true;
        }
        catch (Exception ex)
        {
            SetError(ex);
        }
    }

    [RelayCommand]
    private void CancelImport()
    {
        IsImportDialogOpen = false;
    }

    [RelayCommand]
    private async Task ConfirmImportAsync()
    {
        if (SelectedAccount == null) return;

        try
        {
            var dialog = new OpenFileDialog
            {
                Filter = GetResourceString("FileDialog.CsvFilter")
            };

            if (dialog.ShowDialog() == true)
            {
                IsImportDialogOpen = false;
                IsLoading = true;
                var count = await _transactionService.ImportTransactionsFromCsvAsync(dialog.FileName, SelectedAccount.Id);
                StatusMessage = string.Format(GetResourceString("Reports.ImportSuccess"), count);
                await LoadReportAsync();
            }
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
    private async Task ChangeYearAsync(object? parameter)
    {
        if (parameter is int year)
            SelectedYear = year;
        else if (parameter is string s && int.TryParse(s, out var parsed))
            SelectedYear = parsed;
        await LoadReportAsync();
    }
}
