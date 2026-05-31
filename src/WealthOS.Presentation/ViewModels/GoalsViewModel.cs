using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WealthOS.Application.DTOs;
using WealthOS.Application.Services;
using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Presentation.ViewModels;

public partial class GoalsViewModel : ViewModelBase
{
    private readonly GoalService _service;

    [ObservableProperty]
    private ObservableCollection<GoalDto> _goals = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isAddDialogOpen;

    [ObservableProperty]
    private bool _isEditDialogOpen;

    [ObservableProperty]
    private bool _isConfirmDeleteOpen;

    [ObservableProperty]
    private bool _isProgressDialogOpen;

    [ObservableProperty]
    private Guid _pendingDeleteId;

    [ObservableProperty]
    private Guid _editingId;

    [ObservableProperty]
    private Guid _progressGoalId;

    [ObservableProperty]
    private string _progressGoalName = string.Empty;

    [ObservableProperty]
    private decimal _progressAddAmount;

    [ObservableProperty]
    private string _newName = string.Empty;

    [ObservableProperty]
    private decimal _newTargetAmount;

    [ObservableProperty]
    private decimal _newCurrentAmount;

    [ObservableProperty]
    private DateTime _newTargetDate = DateTime.Today.AddYears(1);

    [ObservableProperty]
    private string? _newNote;

    public GoalsViewModel(GoalService service)
    {
        _service = service;
        _ = LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        ClearError();
        try {
            var items = await _service.GetAllGoalsAsync();
            Goals = new ObservableCollection<GoalDto>(items);
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
        NewTargetAmount = 0;
        NewCurrentAmount = 0;
        NewTargetDate = DateTime.Today.AddYears(1);
        NewNote = null;
        IsAddDialogOpen = true;
    }

    [RelayCommand]
    private void CancelAdd() => IsAddDialogOpen = false;

    [RelayCommand]
    private async Task ConfirmAddAsync()
    {
        if (string.IsNullOrWhiteSpace(NewName) || NewTargetAmount <= 0) return;

        var goal = new Goal
        {
            Name = NewName,
            TargetAmount = NewTargetAmount,
            CurrentAmount = NewCurrentAmount,
            TargetDate = NewTargetDate,
            Note = NewNote
        };

        await _service.AddGoalAsync(goal);
        IsAddDialogOpen = false;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task ShowEditDialog(Guid id)
    {
        var item = await _service.GetGoalAsync(id);
        if (item == null) return;

        EditingId = id;
        NewName = item.Name;
        NewTargetAmount = item.TargetAmount;
        NewCurrentAmount = item.CurrentAmount;
        NewTargetDate = item.TargetDate;
        NewNote = item.Note;
        IsEditDialogOpen = true;
    }

    [RelayCommand]
    private void CancelEdit() => IsEditDialogOpen = false;

    [RelayCommand]
    private async Task ConfirmEditAsync()
    {
        if (string.IsNullOrWhiteSpace(NewName) || NewTargetAmount <= 0) return;

        var item = await _service.GetGoalAsync(EditingId);
        if (item == null) return;

        item.Name = NewName;
        item.TargetAmount = NewTargetAmount;
        item.CurrentAmount = NewCurrentAmount;
        item.TargetDate = NewTargetDate;
        item.Note = NewNote;

        await _service.UpdateGoalAsync(item);
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
        await _service.DeleteGoalAsync(PendingDeleteId);
        IsConfirmDeleteOpen = false;
        await LoadDataAsync();
    }

    [RelayCommand]
    private void ShowProgressDialog(GoalDto goal)
    {
        ProgressGoalId = goal.Id;
        ProgressGoalName = goal.Name;
        ProgressAddAmount = 0;
        IsProgressDialogOpen = true;
    }

    [RelayCommand]
    private void CancelProgress() => IsProgressDialogOpen = false;

    [RelayCommand]
    private async Task ConfirmProgressAsync()
    {
        if (ProgressAddAmount <= 0) return;

        var goal = await _service.GetGoalAsync(ProgressGoalId);
        if (goal != null)
        {
            goal.CurrentAmount += ProgressAddAmount;
            if (goal.CurrentAmount >= goal.TargetAmount)
                goal.Status = GoalStatus.Completed;
            await _service.UpdateGoalAsync(goal);
        }

        IsProgressDialogOpen = false;
        await LoadDataAsync();
    }
}
