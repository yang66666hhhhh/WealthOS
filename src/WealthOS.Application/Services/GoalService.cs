using WealthOS.Application.DTOs;
using WealthOS.Application.Interfaces;
using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Application.Services;

public class GoalService
{
    private readonly IGoalRepository _repo;

    public GoalService(IGoalRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<GoalDto>> GetAllGoalsAsync()
    {
        var goals = await _repo.GetAllAsync();
        return goals.Select(g => new GoalDto
        {
            Id = g.Id,
            Name = g.Name,
            TargetAmount = g.TargetAmount,
            CurrentAmount = g.CurrentAmount,
            TargetDate = g.TargetDate,
            Status = g.Status,
            Icon = g.Icon
        });
    }

    public async Task<IEnumerable<GoalDto>> GetActiveGoalsAsync()
    {
        var goals = await _repo.GetByStatusAsync(GoalStatus.InProgress);
        return goals.Select(g => new GoalDto
        {
            Id = g.Id,
            Name = g.Name,
            TargetAmount = g.TargetAmount,
            CurrentAmount = g.CurrentAmount,
            TargetDate = g.TargetDate,
            Status = g.Status,
            Icon = g.Icon
        });
    }

    public async Task<Goal?> GetGoalAsync(Guid id) => await _repo.GetByIdAsync(id);
    public async Task<Guid> AddGoalAsync(Goal goal) => await _repo.AddAsync(goal);
    public async Task<bool> UpdateGoalAsync(Goal goal) => await _repo.UpdateAsync(goal);
    public async Task<bool> DeleteGoalAsync(Guid id) => await _repo.DeleteAsync(id);
}
