using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WealthOS.Application.DTOs;
using WealthOS.Application.Services;
using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Presentation.ViewModels;

public partial class AssetsViewModel : ObservableObject
{
    private readonly AssetService _service;

    [ObservableProperty]
    private ObservableCollection<AssetCardDto> _assets = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isAddDialogOpen;

    [ObservableProperty]
    private string _newName = string.Empty;

    [ObservableProperty]
    private string _newInstitution = string.Empty;

    [ObservableProperty]
    private decimal _newCurrentValue;

    [ObservableProperty]
    private decimal _newInitialValue;

    [ObservableProperty]
    private AssetType _newType = AssetType.Bank;

    [ObservableProperty]
    private string _newNote = string.Empty;

    [ObservableProperty]
    private AssetType? _filterType;

    public AssetType[] AssetTypes { get; } = Enum.GetValues<AssetType>();

    public AssetsViewModel(AssetService service)
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
            var assets = FilterType.HasValue
                ? await _service.GetAssetsByTypeAsync(FilterType.Value)
                : await _service.GetAllAssetsAsync();
            Assets = new ObservableCollection<AssetCardDto>(assets);
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
        NewInstitution = string.Empty;
        NewCurrentValue = 0;
        NewInitialValue = 0;
        NewType = AssetType.Bank;
        NewNote = string.Empty;
        IsAddDialogOpen = true;
    }

    [RelayCommand]
    private void CancelAdd()
    {
        IsAddDialogOpen = false;
    }

    [RelayCommand]
    private async Task ConfirmAddAsync()
    {
        if (string.IsNullOrWhiteSpace(NewName)) return;

        var asset = new Asset
        {
            Name = NewName,
            Type = NewType,
            CurrentValue = NewCurrentValue,
            InitialValue = NewInitialValue,
            Institution = NewInstitution,
            Note = NewNote
        };

        await _service.AddAssetAsync(asset);
        IsAddDialogOpen = false;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task DeleteAssetAsync(Guid id)
    {
        await _service.DeleteAssetAsync(id);
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task FilterByTypeAsync(AssetType? type)
    {
        FilterType = type;
        await LoadDataAsync();
    }
}
