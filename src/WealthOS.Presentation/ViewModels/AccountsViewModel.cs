using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WealthOS.Application.DTOs;
using WealthOS.Application.Services;
using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Presentation.ViewModels;

public partial class AccountsViewModel : ViewModelBase
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
    private AccountType _newType = AccountType.Bank;

    [ObservableProperty]
    private string _newInstitution = string.Empty;

    [ObservableProperty]
    private decimal _newBalance;

    [ObservableProperty]
    private string _newCurrency = "CNY";

    [ObservableProperty]
    private string? _newNote;

    public AccountType[] AccountTypes { get; } = Enum.GetValues<AccountType>();

    public AccountsViewModel(AccountService service)
    {
        _service = service;
        _ = LoadDataAsync();
    }

    public override IRelayCommand? RefreshCommand => LoadDataCommand;

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        ClearError();
        try {
            var items = await _service.GetAllAccountsAsync();
            Accounts = new ObservableCollection<AccountDto>(items);
            TotalBalance = items.Sum(a => a.Balance);
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
    private void ShowAddDialog()
    {
        NewName = string.Empty;
        NewType = AccountType.Bank;
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
        if (NewBalance < 0) { SetError(GetResourceString("Validation.NegativeBalance")); return; }

        try
        {
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
        catch (Exception ex)
        {
            SetError(ex);
        }
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

        try
        {
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
        try
        {
            await _service.DeleteAccountAsync(PendingDeleteId);
            IsConfirmDeleteOpen = false;
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            SetError(ex);
        }
    }
}
