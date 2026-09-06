using FinanceHelper.Core.Domain.Models;

namespace FinanceHelper.Core.Application.Ports;

public interface IAiCategorizer
{
    Task<List<CategorizedExpense>?> CategorizeTransactionsAsync(IEnumerable<Transacao> transactions);
} 