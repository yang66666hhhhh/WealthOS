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
    private readonly AccountService _accountService;

    [ObservableProperty]
    private ObservableCollection<TransactionDto> _transactions = [];

    [ObservableProperty]
    private ObservableCollection<AccountDto> _accounts = [];

    [ObservableProperty]
    private AccountDto? _selectedAccount;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isAddDialogOpen;

    [ObservableProperty]
    private bool _isEditDialogOpen;

    [ObservableProperty]
    private bool _isConfirmDeleteOpen;

    [ObservableProperty]
    private Guid _pendingDeleteId;

    [ObservableProperty]
    private Guid _editingId;

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

    public TransactionsViewModel(TransactionService service, AccountService accountService)
    {
        _service = service;
        _accountService = accountService;
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

            var accountList = await _accountService.GetAllAccountsAsync();
            Accounts = new ObservableCollection<AccountDto>(accountList);
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
        SelectedAccount = Accounts.FirstOrDefault();
        IsAddDialogOpen = true;
    }

    [RelayCommand]
    private void CancelAdd() => IsAddDialogOpen = false;

    [RelayCommand]
    private async Task ConfirmAddAsync()
    {
        if (NewAmount <= 0 || SelectedAccount == null) return;

        var transaction = new Transaction
        {
            Type = NewType,
            Amount = NewAmount,
            Note = NewNote,
            OccurredAt = NewDate,
            AccountId = SelectedAccount.Id
        };

        await _service.AddTransactionAsync(transaction);
        IsAddDialogOpen = false;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task ShowEditDialog(TransactionDto dto)
    {
        EditingId = dto.Id;
        NewType = dto.Type;
        NewAmount = dto.Amount;
        NewNote = dto.Note;
        NewDate = dto.OccurredAt;
        SelectedAccount = Accounts.FirstOrDefault(a => a.Name == dto.AccountName);
        IsEditDialogOpen = true;
    }

    [RelayCommand]
    private void CancelEdit() => IsEditDialogOpen = false;

    [RelayCommand]
    private async Task ConfirmEditAsync()
    {
        if (NewAmount <= 0 || SelectedAccount == null) return;

        await _service.DeleteTransactionAsync(EditingId);

        var transaction = new Transaction
        {
            Type = NewType,
            Amount = NewAmount,
            Note = NewNote,
            OccurredAt = NewDate,
            AccountId = SelectedAccount.Id
        };

        await _service.AddTransactionAsync(transaction);
        IsEditDialogOpen = false;
        await LoadDataAsync();
    }

    [RelayCommand]
    private void ConfirmDelete(Guid id)
    {
        PendingDeleteId = id;
        IsConfirmDeleteOpen = true;
    }

    [RelayCommand]
    private void CancelDelete() => IsConfirmDeleteOpen = false;

    [RelayCommand]
    private async Task ExecuteDeleteAsync()
    {
        await _service.DeleteTransactionAsync(PendingDeleteId);
        IsConfirmDeleteOpen = false;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task FilterByTypeAsync(TransactionType? type)
    {
        FilterType = type;
        await LoadDataAsync();
    }
}
