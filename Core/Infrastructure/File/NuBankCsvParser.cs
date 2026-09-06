using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using FinanceHelper.Core.Application.Ports;
using FinanceHelper.Core.Domain.Models;

namespace FinanceHelper.Core.Infrastructure.File;

public class NuBankCsvParser : IFileParser
{
    public bool CanParse(string filePath)
    {
        return Path.GetFileName(filePath).StartsWith("NU_", StringComparison.OrdinalIgnoreCase);
    }

    public IEnumerable<Transacao> Parse(string filePath)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);
        
        var records = csv.GetRecords<Transacao>().ToList();
        return records;
    }
} 