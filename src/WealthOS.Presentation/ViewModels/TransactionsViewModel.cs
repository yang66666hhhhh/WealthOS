using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WealthOS.Application.DTOs;
using WealthOS.Application.Services;
using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Presentation.ViewModels;

public partial class TransactionsViewModel : ViewModelBase
{
    private readonly TransactionService _service;
    private readonly AccountService _accountService;
    private readonly CategoryService _categoryService;
    private List<TransactionDto> _allTransactions = [];

    [ObservableProperty]
    private ObservableCollection<TransactionDto> _transactions = [];

    [ObservableProperty]
    private ObservableCollection<AccountDto> _accounts = [];

    [ObservableProperty]
    private ObservableCollection<Category> _categories = [];

    [ObservableProperty]
    private AccountDto? _selectedAccount;

    [ObservableProperty]
    private Category? _selectedCategory;

    [ObservableProperty]
    private string _searchText = string.Empty;

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

    [ObservableProperty]
    private string _selectedFilter = "All";

    public TransactionType[] TransactionTypes { get; } = Enum.GetValues<TransactionType>();

    public TransactionsViewModel(TransactionService service, AccountService accountService, CategoryService categoryService)
    {
        _service = service;
        _accountService = accountService;
        _categoryService = categoryService;
        _ = LoadDataAsync();
    }

    public override IRelayCommand? RefreshCommand => LoadDataCommand;

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        ClearError();
        try {
            var end = DateTime.UtcNow;
            var start = end.AddDays(-90);
            var items = await _service.GetTransactionsAsync(start, end);

            _allTransactions = items.ToList();

            var accountList = await _accountService.GetAllAccountsAsync();
            Accounts = new ObservableCollection<AccountDto>(accountList);

            var categoryList = await _categoryService.GetAllAsync();
            Categories = new ObservableCollection<Category>(categoryList);

            ApplyFilters();
        }
        catch (Exception ex)
        {
            SetError(ex);
        }
        finally {
            IsLoading = false;
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = _allTransactions.AsEnumerable();

        if (FilterType.HasValue)
            filtered = filtered.Where(t => t.Type == FilterType.Value);

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            filtered = filtered.Where(t =>
                (t.Note != null && t.Note.ToLower().Contains(search)) ||
                t.AccountName.ToLower().Contains(search) ||
                (t.CategoryName != null && t.CategoryName.ToLower().Contains(search)));
        }

        Transactions = new ObservableCollection<TransactionDto>(filtered);
    }

    [RelayCommand]
    private void ShowAddDialog()
    {
        NewType = TransactionType.Expense;
        NewAmount = 0;
        NewNote = null;
        NewDate = DateTime.Today;
        SelectedAccount = Accounts.FirstOrDefault();
        SelectedCategory = Categories.FirstOrDefault(c => c.Type == NewType);
        IsAddDialogOpen = true;
    }

    partial void OnNewTypeChanged(TransactionType value)
    {
        if (IsAddDialogOpen || IsEditDialogOpen)
        {
            SelectedCategory = Categories.FirstOrDefault(c => c.Type == value);
        }
    }

    [RelayCommand]
    private void CancelAdd() => IsAddDialogOpen = false;

    [RelayCommand]
    private async Task ConfirmAddAsync()
    {
        if (NewAmount <= 0 || SelectedAccount == null) return;

        try
        {
            var transaction = new Transaction
            {
                Type = NewType,
                Amount = NewAmount,
                Note = NewNote,
                OccurredAt = NewDate,
                AccountId = SelectedAccount.Id,
                CategoryId = SelectedCategory?.Id
            };

            await _service.AddTransactionAsync(transaction);
            IsAddDialogOpen = false;
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            SetError(ex);
        }
    }

    [RelayCommand]
    private async Task ShowEditDialog(TransactionDto dto)
    {
        EditingId = dto.Id;
        NewType = dto.Type;
        NewAmount = dto.Amount;
        NewNote = dto.Note;
        NewDate = dto.OccurredAt;
        SelectedAccount = Accounts.FirstOrDefault(a => a.Id == dto.AccountId);
        SelectedCategory = Categories.FirstOrDefault(c => c.Id == dto.CategoryId);
        IsEditDialogOpen = true;
    }

    [RelayCommand]
    private void CancelEdit() => IsEditDialogOpen = false;

    [RelayCommand]
    private async Task ConfirmEditAsync()
    {
        if (NewAmount <= 0 || SelectedAccount == null) return;

        try
        {
            var transaction = new Transaction
            {
                Type = NewType,
                Amount = NewAmount,
                Note = NewNote,
                OccurredAt = NewDate,
                AccountId = SelectedAccount.Id,
                CategoryId = SelectedCategory?.Id
            };

            await _service.UpdateTransactionAsync(EditingId, transaction);
            IsEditDialogOpen = false;
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            SetError(ex);
        }
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
    private async Task FilterByTypeAsync(object? parameter)
    {
        TransactionType? type = parameter switch
        {
            TransactionType t => t,
            string s when Enum.TryParse<TransactionType>(s, out var parsed) => parsed,
            _ => null
        };
        FilterType = type;
        SelectedFilter = type?.ToString() ?? "All";
        ApplyFilters();
    }
}
