using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using FinanceHelper.Core.Application.Ports;
using FinanceHelper.Core.Domain.Models;

namespace FinanceHelper.Core.Infrastructure.File;

public class C6CsvParser : IFileParser
{
    public bool CanParse(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        return fileName.StartsWith("Fatura_", StringComparison.OrdinalIgnoreCase) || 
               fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) && fileName.Length == 36; // for 01JXXFWXKVY66G911H856JPT6H.csv
    }

    public IEnumerable<Transacao> Parse(string filePath)
    {
        var config = new CsvConfiguration(new CultureInfo("pt-BR"))
        {
            HasHeaderRecord = true,
            Delimiter = ";",
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<C6TransactionMap>();

        var records = csv.GetRecords<Transacao>().ToList();

        foreach (var record in records)
        {
            // In C6 statements, expenses are positive. We need to invert them.
            // Payments are negative, so they become positive (income).
            record.Valor *= -1;

            // Generate a unique ID based on the transaction data, as none is provided.
            record.Identificador = GenerateHashForTransaction(record);
        }

        return records;
    }

    private string GenerateHashForTransaction(Transacao t)
    {
        var input = $"{t.Data:yyyy-MM-dd}-{t.Descricao}-{t.Valor.ToString(CultureInfo.InvariantCulture)}";
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}

public class C6TransactionMap : ClassMap<Transacao>
{
    public C6TransactionMap()
    {
        Map(m => m.Data).Name("Data de Compra");
        Map(m => m.Descricao).Name("Descrição");
        Map(m => m.Valor).Name("Valor (em R$)");
        
        // Ignorar campos que não existem no CSV do C6 mas existem no nosso modelo
        Map(m => m.Identificador).Ignore();
        Map(m => m.CategoriaSugerida).Ignore();
        Map(m => m.IsRecorrente).Ignore();
    }
} 