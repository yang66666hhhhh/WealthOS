using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Application.Interfaces;

public interface IGoalRepository : IRepository<Goal>
{
    Task<IEnumerable<Goal>> GetByStatusAsync(GoalStatus status);
}
