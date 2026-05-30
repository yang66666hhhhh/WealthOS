using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WealthOS.Application.DTOs;
using WealthOS.Application.Services;
using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Presentation.ViewModels;

public partial class TransactionsViewModel : ObservableObject
{
    private readonly TransactionService _service;

    [ObservableProperty]
    private ObservableCollection<TransactionDto> _transactions = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isAddDialogOpen;

    [ObservableProperty]
    private TransactionType _newType = TransactionType.Expense;

    [ObservableProperty]
    private decimal _newAmount;

    [ObservableProperty]
    private string? _newNote;

    [ObservableProperty]
    private DateTime _newDate = DateTime.Today;

    [ObservableProperty]
    private TransactionType? _filterType;

    public TransactionType[] TransactionTypes { get; } = Enum.GetValues<TransactionType>();

    public TransactionsViewModel(TransactionService service)
    {
        _service = service;
        _ = LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var end = DateTime.UtcNow;
            var start = end.AddDays(-90);
            var items = await _service.GetTransactionsAsync(start, end);

            if (FilterType.HasValue)
                items = items.Where(t => t.Type == FilterType.Value);

            Transactions = new ObservableCollection<TransactionDto>(items);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ShowAddDialog()
    {
        NewType = TransactionType.Expense;
        NewAmount = 0;
        NewNote = null;
        NewDate = DateTime.Today;
        IsAddDialogOpen = true;
    }

    [RelayCommand]
    private void CancelAdd() => IsAddDialogOpen = false;

    [RelayCommand]
    private async Task ConfirmAddAsync()
    {
        if (NewAmount <= 0) return;

        var transaction = new Transaction
        {
            Type = NewType,
            Amount = NewAmount,
            Note = NewNote,
            OccurredAt = NewDate,
            AccountId = Guid.Empty
        };

        await _service.AddTransactionAsync(transaction);
        IsAddDialogOpen = false;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task FilterByTypeAsync(TransactionType? type)
    {
        FilterType = type;
        await LoadDataAsync();
    }
}
