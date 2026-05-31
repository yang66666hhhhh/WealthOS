using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WealthOS.Application.DTOs;
using WealthOS.Application.Services;
using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Presentation.ViewModels;

public partial class AccountsViewModel : ObservableObject
{
    private readonly AccountService _service;

    [ObservableProperty]
    private ObservableCollection<AccountDto> _accounts = [];

    [ObservableProperty]
    private decimal _totalBalance;

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
    private string _newName = string.Empty;

    [ObservableProperty]
    private AssetType _newType = AssetType.Bank;

    [ObservableProperty]
    private string _newInstitution = string.Empty;

    [ObservableProperty]
    private decimal _newBalance;

    [ObservableProperty]
    private string _newCurrency = "CNY";

    [ObservableProperty]
    private string? _newNote;

    public AssetType[] AccountTypes { get; } = Enum.GetValues<AssetType>();

    public AccountsViewModel(AccountService service)
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
            var items = await _service.GetAllAccountsAsync();
            Accounts = new ObservableCollection<AccountDto>(items);
            TotalBalance = items.Sum(a => a.Balance);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ShowAddDialog()
    {
        NewName = string.Empty;
        NewType = AssetType.Bank;
        NewInstitution = string.Empty;
        NewBalance = 0;
        NewCurrency = "CNY";
        NewNote = null;
        IsAddDialogOpen = true;
    }

    [RelayCommand]
    private void CancelAdd() => IsAddDialogOpen = false;

    [RelayCommand]
    private async Task ConfirmAddAsync()
    {
        if (string.IsNullOrWhiteSpace(NewName)) return;

        var account = new Account
        {
            Name = NewName,
            Type = NewType,
            Institution = NewInstitution,
            Balance = NewBalance,
            Currency = NewCurrency,
            Note = NewNote
        };

        await _service.AddAccountAsync(account);
        IsAddDialogOpen = false;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task ShowEditDialog(Guid id)
    {
        var item = await _service.GetAccountAsync(id);
        if (item == null) return;

        EditingId = id;
        NewName = item.Name;
        NewType = item.Type;
        NewInstitution = item.Institution;
        NewBalance = item.Balance;
        NewCurrency = item.Currency;
        NewNote = item.Note;
        IsEditDialogOpen = true;
    }

    [RelayCommand]
    private void CancelEdit() => IsEditDialogOpen = false;

    [RelayCommand]
    private async Task ConfirmEditAsync()
    {
        if (string.IsNullOrWhiteSpace(NewName)) return;

        var item = await _service.GetAccountAsync(EditingId);
        if (item == null) return;

        item.Name = NewName;
        item.Type = NewType;
        item.Institution = NewInstitution;
        item.Balance = NewBalance;
        item.Currency = NewCurrency;
        item.Note = NewNote;

        await _service.UpdateAccountAsync(item);
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
        await _service.DeleteAccountAsync(PendingDeleteId);
        IsConfirmDeleteOpen = false;
        await LoadDataAsync();
    }
}
