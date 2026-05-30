using WealthOS.Domain.Enums;

namespace WealthOS.Application.DTOs;

public record TransactionDto
{
    public Guid Id { get; init; }
    public TransactionType Type { get; init; }
    public decimal Amount { get; init; }
    public string? CategoryName { get; init; }
    public string AccountName { get; init; } = string.Empty;
    public string? Note { get; init; }
    public DateTime OccurredAt { get; init; }
}
