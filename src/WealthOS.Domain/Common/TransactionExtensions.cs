using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Domain.Common;

public static class TransactionExtensions
{
    public static decimal GetBalanceImpact(this Transaction transaction)
    {
        return transaction.Type switch
        {
            TransactionType.Income => transaction.Amount,
            TransactionType.Expense => -transaction.Amount,
            TransactionType.Transfer => -transaction.Amount,
            _ => 0
        };
    }
}
