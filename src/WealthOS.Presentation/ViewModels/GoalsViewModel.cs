using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WealthOS.Application.DTOs;
using WealthOS.Application.Services;
using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Presentation.ViewModels;

public partial class GoalsViewModel : ObservableObject
{
    private readonly GoalService _service;

    [ObservableProperty]
    private ObservableCollection<GoalDto> _goals = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isAddDialogOpen;

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
        try
        {
            var items = await _service.GetAllGoalsAsync();
            Goals = new ObservableCollection<GoalDto>(items);
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
    private async Task DeleteGoalAsync(Guid id)
    {
        await _service.DeleteGoalAsync(id);
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task UpdateGoalProgressAsync((Guid id, decimal amount) args)
    {
        var goal = await _service.GetGoalAsync(args.id);
        if (goal != null)
        {
            goal.CurrentAmount = args.amount;
            if (goal.CurrentAmount >= goal.TargetAmount)
                goal.Status = GoalStatus.Completed;
            await _service.UpdateGoalAsync(goal);
            await LoadDataAsync();
        }
    }
}
