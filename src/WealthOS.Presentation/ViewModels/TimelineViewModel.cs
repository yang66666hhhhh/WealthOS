using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WealthOS.Application.DTOs;
using WealthOS.Application.Services;
using WealthOS.Domain.Enums;

namespace WealthOS.Presentation.ViewModels;

public partial class TimelineViewModel : ViewModelBase
{
    private readonly TransactionService _transactionService;

    [ObservableProperty]
    private ObservableCollection<TimelineDayDto> _days = [];

    [ObservableProperty]
    private bool _isLoading;

    public TimelineViewModel(TransactionService transactionService)
    {
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
            var now = DateTime.UtcNow;
            var start = now.AddDays(-90);
            var transactions = await _transactionService.GetTransactionsAsync(start, now);

            var grouped = transactions
                .GroupBy(t => t.OccurredAt.Date)
                .OrderByDescending(g => g.Key)
                .Select(g => new TimelineDayDto
                {
                    Date = g.Key,
                    Transactions = [.. g.OrderBy(t => t.OccurredAt)],
                    TotalIncome = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                    TotalExpense = g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
                });

            Days = new ObservableCollection<TimelineDayDto>(grouped);
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

public class TimelineDayDto
{
    public DateTime Date { get; init; }
    public List<TransactionDto> Transactions { get; init; } = [];
    public decimal TotalIncome { get; init; }
    public decimal TotalExpense { get; init; }
    public decimal NetChange => TotalIncome - TotalExpense;
}
