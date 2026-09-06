using FinanceHelper.Core.Domain.Models;

namespace FinanceHelper.Core.Application.Ports;

public interface IFileParser
{
    bool CanParse(string filePath);
    IEnumerable<Transacao> Parse(string filePath);
} 