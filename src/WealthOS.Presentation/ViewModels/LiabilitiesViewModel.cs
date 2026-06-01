using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WealthOS.Application.DTOs;
using WealthOS.Application.Services;
using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Presentation.ViewModels;

public partial class LiabilitiesViewModel : ViewModelBase
{
    private readonly LiabilityService _service;

    [ObservableProperty]
    private ObservableCollection<LiabilityDto> _liabilities = [];

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
    private LiabilityType _newType = LiabilityType.CreditCard;

    [ObservableProperty]
    private decimal _newBalance;

    [ObservableProperty]
    private decimal _newInterestRate;

    [ObservableProperty]
    private decimal _newMonthlyPayment;

    [ObservableProperty]
    private DateTime _newStartDate = DateTime.Today;

    [ObservableProperty]
    private string? _newInstitution;

    public LiabilityType[] LiabilityTypes { get; } = Enum.GetValues<LiabilityType>();

    public LiabilitiesViewModel(LiabilityService service)
    {
        _service = service;
        SafeInitializeAsync(LoadDataAsync);
    }

    public override IRelayCommand? RefreshCommand => LoadDataCommand;

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        ClearError();
        try {
            var items = await _service.GetAllLiabilitiesAsync();
            Liabilities = new ObservableCollection<LiabilityDto>(items);
            TotalBalance = items.Sum(l => l.Balance);
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
        NewType = LiabilityType.CreditCard;
        NewBalance = 0;
        NewInterestRate = 0;
        NewMonthlyPayment = 0;
        NewStartDate = DateTime.Today;
        NewInstitution = null;
        IsAddDialogOpen = true;
    }

    [RelayCommand]
    private void CancelAdd() => IsAddDialogOpen = false;

    [RelayCommand]
    private async Task ConfirmAddAsync()
    {
        if (string.IsNullOrWhiteSpace(NewName)) return;
        if (NewBalance < 0) { SetError(GetResourceString("Validation.NegativeLiability")); return; }
        if (NewInterestRate < 0 || NewMonthlyPayment < 0) { SetError(GetResourceString("Validation.NegativeRateOrPayment")); return; }

        try
        {
            var liability = new Liability
            {
                Name = NewName,
                Type = NewType,
                Balance = NewBalance,
                InterestRate = NewInterestRate,
                MonthlyPayment = NewMonthlyPayment,
                StartDate = NewStartDate,
                Institution = NewInstitution
            };

            await _service.AddLiabilityAsync(liability);
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
        var item = await _service.GetLiabilityAsync(id);
        if (item == null) return;

        EditingId = id;
        NewName = item.Name;
        NewType = item.Type;
        NewBalance = item.Balance;
        NewInterestRate = item.InterestRate;
        NewMonthlyPayment = item.MonthlyPayment;
        NewStartDate = item.StartDate;
        NewInstitution = item.Institution;
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
            var item = await _service.GetLiabilityAsync(EditingId);
            if (item == null) return;

            item.Name = NewName;
            item.Type = NewType;
            item.Balance = NewBalance;
            item.InterestRate = NewInterestRate;
            item.MonthlyPayment = NewMonthlyPayment;
            item.StartDate = NewStartDate;
            item.Institution = NewInstitution;

            await _service.UpdateLiabilityAsync(item);
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
            await _service.DeleteLiabilityAsync(PendingDeleteId);
            IsConfirmDeleteOpen = false;
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            SetError(ex);
        }
    }
}
