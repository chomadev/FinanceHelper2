using FinanceHelper.Core.Domain.Models;

namespace FinanceHelper.Core.Application.Ports;

public interface ITransactionReader
{
    IList<Transacao> ReadTransactions(IEnumerable<string> filePaths);
} 