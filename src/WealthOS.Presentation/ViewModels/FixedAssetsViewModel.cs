using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WealthOS.Application.DTOs;
using WealthOS.Application.Services;
using WealthOS.Domain.Enums;

namespace WealthOS.Presentation.ViewModels;

public partial class FixedAssetsViewModel : ObservableObject
{
    private readonly AssetService _assetService;

    [ObservableProperty]
    private ObservableCollection<AssetCardDto> _fixedAssets = [];

    [ObservableProperty]
    private decimal _totalInitialValue;

    [ObservableProperty]
    private decimal _totalCurrentValue;

    [ObservableProperty]
    private decimal _totalDepreciation;

    [ObservableProperty]
    private bool _isLoading;

    public FixedAssetsViewModel(AssetService assetService)
    {
        _assetService = assetService;
        _ = LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var allAssets = await _assetService.GetAllAssetsAsync();
            var fixedAssets = allAssets.Where(a =>
                a.Type is AssetType.House or AssetType.Car or AssetType.Collection or AssetType.Other
                && a.DepreciationRate.HasValue);

            FixedAssets = new ObservableCollection<AssetCardDto>(fixedAssets);
            TotalInitialValue = FixedAssets.Sum(a => a.InitialValue);
            TotalCurrentValue = FixedAssets.Sum(a => a.CurrentValue);
            TotalDepreciation = TotalInitialValue - TotalCurrentValue;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
