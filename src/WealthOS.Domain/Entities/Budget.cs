using WealthOS.Domain.Common;

namespace WealthOS.Domain.Entities;

public class Budget : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Spent { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Note { get; set; }
}
