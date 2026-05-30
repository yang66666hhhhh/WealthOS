using WealthOS.Domain.Common;
using WealthOS.Domain.Enums;

namespace WealthOS.Domain.Entities;

public class Transaction : BaseEntity
{
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid AccountId { get; set; }
    public Guid? ToAccountId { get; set; }
    public string? Note { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
