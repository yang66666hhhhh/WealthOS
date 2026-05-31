using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WealthOS.Application.DTOs;
using WealthOS.Application.Services;
using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Presentation.ViewModels;

public partial class InvestmentsViewModel : ObservableObject
{
    private readonly InvestmentService _service;

    [ObservableProperty]
    private ObservableCollection<InvestmentHoldingDto> _holdings = [];

    [ObservableProperty]
    private decimal _totalValue;

    [ObservableProperty]
    private decimal _totalCost;

    [ObservableProperty]
    private decimal _totalProfitLoss;

    [ObservableProperty]
    private decimal _totalProfitLossPercentage;

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
    private string _newSymbol = string.Empty;

    [ObservableProperty]
    private string _newName = string.Empty;

    [ObservableProperty]
    private AssetType _newAssetType = AssetType.Stock;

    [ObservableProperty]
    private decimal _newQuantity;

    [ObservableProperty]
    private decimal _newAverageCost;

    [ObservableProperty]
    private decimal _newCurrentPrice;

    [ObservableProperty]
    private string? _newNote;

    public AssetType[] InvestmentTypes { get; } = [AssetType.Stock, AssetType.Fund, AssetType.ETF, AssetType.Gold, AssetType.Crypto];

    public InvestmentsViewModel(InvestmentService service)
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
            var items = await _service.GetAllHoldingsAsync();
            Holdings = new ObservableCollection<InvestmentHoldingDto>(items);
            TotalValue = items.Sum(h => h.TotalValue);
            TotalCost = items.Sum(h => h.TotalCost);
            TotalProfitLoss = items.Sum(h => h.ProfitLoss);
            TotalProfitLossPercentage = TotalCost > 0 ? TotalProfitLoss / TotalCost * 100 : 0;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ShowAddDialog()
    {
        NewSymbol = string.Empty;
        NewName = string.Empty;
        NewAssetType = AssetType.Stock;
        NewQuantity = 0;
        NewAverageCost = 0;
        NewCurrentPrice = 0;
        NewNote = null;
        IsAddDialogOpen = true;
    }

    [RelayCommand]
    private void CancelAdd() => IsAddDialogOpen = false;

    [RelayCommand]
    private async Task ConfirmAddAsync()
    {
        if (string.IsNullOrWhiteSpace(NewSymbol) || NewQuantity <= 0) return;

        var holding = new InvestmentHolding
        {
            Symbol = NewSymbol.ToUpper(),
            Name = NewName,
            AssetType = NewAssetType,
            Quantity = NewQuantity,
            AverageCost = NewAverageCost,
            CurrentPrice = NewCurrentPrice,
            Note = NewNote
        };

        await _service.AddHoldingAsync(holding);
        IsAddDialogOpen = false;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task ShowEditDialog(Guid id)
    {
        var item = await _service.GetHoldingAsync(id);
        if (item == null) return;

        EditingId = id;
        NewSymbol = item.Symbol;
        NewName = item.Name;
        NewAssetType = item.AssetType;
        NewQuantity = item.Quantity;
        NewAverageCost = item.AverageCost;
        NewCurrentPrice = item.CurrentPrice;
        NewNote = item.Note;
        IsEditDialogOpen = true;
    }

    [RelayCommand]
    private void CancelEdit() => IsEditDialogOpen = false;

    [RelayCommand]
    private async Task ConfirmEditAsync()
    {
        if (string.IsNullOrWhiteSpace(NewSymbol) || NewQuantity <= 0) return;

        var item = await _service.GetHoldingAsync(EditingId);
        if (item == null) return;

        item.Symbol = NewSymbol.ToUpper();
        item.Name = NewName;
        item.AssetType = NewAssetType;
        item.Quantity = NewQuantity;
        item.AverageCost = NewAverageCost;
        item.CurrentPrice = NewCurrentPrice;
        item.Note = NewNote;

        await _service.UpdateHoldingAsync(item);
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
        await _service.DeleteHoldingAsync(PendingDeleteId);
        IsConfirmDeleteOpen = false;
        await LoadDataAsync();
    }
}
