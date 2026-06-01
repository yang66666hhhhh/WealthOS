using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WealthOS.Application.DTOs;
using WealthOS.Application.Services;
using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Presentation.ViewModels;

public partial class AssetsViewModel : ViewModelBase
{
    private readonly AssetService _service;

    [ObservableProperty]
    private ObservableCollection<AssetCardDto> _assets = [];

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

    [ObservableProperty]
    private string _selectedFilter = "All";

    public AssetType[] AssetTypes { get; } = Enum.GetValues<AssetType>();

    public AssetsViewModel(AssetService service)
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
            var assets = FilterType.HasValue
                ? await _service.GetAssetsByTypeAsync(FilterType.Value)
                : await _service.GetAllAssetsAsync();
            Assets = new ObservableCollection<AssetCardDto>(assets);
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
        NewInstitution = string.Empty;
        NewCurrentValue = 0;
        NewInitialValue = 0;
        NewType = AssetType.Bank;
        NewNote = string.Empty;
        IsAddDialogOpen = true;
    }

    [RelayCommand]
    private void CancelAdd() => IsAddDialogOpen = false;

    [RelayCommand]
    private async Task ConfirmAddAsync()
    {
        if (string.IsNullOrWhiteSpace(NewName)) return;
        if (NewCurrentValue < 0 || NewInitialValue < 0) { SetError(GetResourceString("Validation.NegativeValue")); return; }

        try
        {
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
        catch (Exception ex)
        {
            SetError(ex);
        }
    }

    [RelayCommand]
    private async Task ShowEditDialog(Guid id)
    {
        var asset = await _service.GetAssetAsync(id);
        if (asset == null) return;

        EditingId = id;
        NewName = asset.Name;
        NewInstitution = asset.Institution;
        NewCurrentValue = asset.CurrentValue;
        NewInitialValue = asset.InitialValue;
        NewType = asset.Type;
        NewNote = asset.Note ?? string.Empty;
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
            var asset = await _service.GetAssetAsync(EditingId);
            if (asset == null) return;

            asset.Name = NewName;
            asset.Type = NewType;
            asset.CurrentValue = NewCurrentValue;
            asset.InitialValue = NewInitialValue;
            asset.Institution = NewInstitution;
            asset.Note = NewNote;

            await _service.UpdateAssetAsync(asset);
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
            await _service.DeleteAssetAsync(PendingDeleteId);
            IsConfirmDeleteOpen = false;
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            SetError(ex);
        }
    }

    [RelayCommand]
    private async Task FilterByTypeAsync(object? parameter)
    {
        AssetType? type = parameter switch
        {
            AssetType t => t,
            string s when Enum.TryParse<AssetType>(s, out var parsed) => parsed,
            _ => null
        };
        FilterType = type;
        SelectedFilter = type?.ToString() ?? "All";
        await LoadDataAsync();
    }
}
