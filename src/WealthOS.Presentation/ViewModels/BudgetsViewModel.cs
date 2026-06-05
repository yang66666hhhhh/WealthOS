using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WealthOS.Application.DTOs;
using WealthOS.Application.Services;
using WealthOS.Domain.Entities;

namespace WealthOS.Presentation.ViewModels;

public partial class BudgetsViewModel : ViewModelBase
{
    private readonly BudgetService _service;
    private readonly CategoryService _categoryService;

    [ObservableProperty]
    private ObservableCollection<BudgetDto> _budgets = [];

    [ObservableProperty]
    private ObservableCollection<Category> _categories = [];

    [ObservableProperty]
    private decimal _totalBudget;

    [ObservableProperty]
    private decimal _totalSpent;

    [ObservableProperty]
    private decimal _totalRemaining;

    [ObservableProperty]
    private decimal _executionRate;

    [ObservableProperty]
    private double _totalProgress;

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
    private decimal _newAmount;

    [ObservableProperty]
    private decimal _newSpent;

    [ObservableProperty]
    private string? _newNote;

    [ObservableProperty]
    private Guid? _newCategoryId;

    [ObservableProperty]
    private int _selectedYear = DateTime.UtcNow.Year;

    [ObservableProperty]
    private int _selectedMonth = DateTime.UtcNow.Month;

    public BudgetsViewModel(BudgetService service, CategoryService categoryService)
    {
        _service = service;
        _categoryService = categoryService;
        SafeInitializeAsync(LoadDataAsync);
    }

    public override IRelayCommand? RefreshCommand => LoadDataCommand;

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        ClearError();
        try {
            var items = await _service.GetBudgetsAsync(SelectedYear, SelectedMonth);
            var categories = await _categoryService.GetAllAsync();
            Budgets = new ObservableCollection<BudgetDto>(items);
            Categories = new ObservableCollection<Category>(categories);
            TotalBudget = items.Sum(b => b.Amount);
            TotalSpent = items.Sum(b => b.Spent);
            TotalRemaining = TotalBudget - TotalSpent;
            ExecutionRate = TotalBudget > 0 ? TotalSpent / TotalBudget * 100 : 0;
            TotalProgress = TotalBudget > 0 ? (double)(TotalSpent / TotalBudget * 100) : 0;
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
        NewAmount = 0;
        NewSpent = 0;
        NewNote = null;
        NewCategoryId = null;
        IsAddDialogOpen = true;
    }

    [RelayCommand]
    private void CancelAdd() => IsAddDialogOpen = false;

    [RelayCommand]
    private async Task ConfirmAddAsync()
    {
        if (string.IsNullOrWhiteSpace(NewName) || NewAmount <= 0) return;

        try
        {
            var budget = new Budget
            {
                Name = NewName,
                Amount = NewAmount,
                Spent = NewSpent,
                Month = SelectedMonth,
                Year = SelectedYear,
                CategoryId = NewCategoryId,
                Note = NewNote
            };

            await _service.AddBudgetAsync(budget);
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
        try
        {
            var item = await _service.GetBudgetAsync(id);
            if (item == null) return;

            EditingId = id;
            NewName = item.Name;
            NewAmount = item.Amount;
            NewSpent = item.Spent;
            NewCategoryId = item.CategoryId;
            NewNote = item.Note;
            IsEditDialogOpen = true;
        }
        catch (Exception ex)
        {
            SetError(ex);
        }
    }

    [RelayCommand]
    private void CancelEdit() => IsEditDialogOpen = false;

    [RelayCommand]
    private async Task ConfirmEditAsync()
    {
        if (string.IsNullOrWhiteSpace(NewName)) return;

        try
        {
            var item = await _service.GetBudgetAsync(EditingId);
            if (item == null) return;

            item.Name = NewName;
            item.Amount = NewAmount;
            item.Spent = NewSpent;
            item.CategoryId = NewCategoryId;
            item.Note = NewNote;

            await _service.UpdateBudgetAsync(item);
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
            await _service.DeleteBudgetAsync(PendingDeleteId);
            IsConfirmDeleteOpen = false;
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            SetError(ex);
        }
    }

    [RelayCommand]
    private async Task ChangeMonthAsync(object? parameter)
    {
        int delta = parameter switch
        {
            int i => i,
            string s when int.TryParse(s, out var parsed) => parsed,
            _ => 0
        };
        SelectedMonth += delta;
        if (SelectedMonth > 12) { SelectedMonth = 1; SelectedYear++; }
        if (SelectedMonth < 1) { SelectedMonth = 12; SelectedYear--; }
        await LoadDataAsync();
    }
}
