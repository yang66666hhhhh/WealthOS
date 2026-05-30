using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WealthOS.Application.DTOs;
using WealthOS.Application.Services;
using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Presentation.ViewModels;

public partial class LiabilitiesViewModel : ObservableObject
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
        _ = LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var items = await _service.GetAllLiabilitiesAsync();
            Liabilities = new ObservableCollection<LiabilityDto>(items);
            TotalBalance = items.Sum(l => l.Balance);
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

    [RelayCommand]
    private async Task DeleteLiabilityAsync(Guid id)
    {
        await _service.DeleteLiabilityAsync(id);
        await LoadDataAsync();
    }
}
