using FinanceHelper.Core.Application.Ports;
using FinanceHelper.Core.Domain.Models;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FinanceHelper.Core.Infrastructure.File;

public class CompositeTransactionReader : ITransactionReader
{
    private readonly IEnumerable<IFileParser> _parsers;

    public CompositeTransactionReader(IEnumerable<IFileParser> parsers)
    {
        _parsers = parsers;
    }

    public IList<Transacao> ReadTransactions(IEnumerable<string> filePaths)
    {
        var allTransactions = new List<Transacao>();

        foreach (var file in filePaths)
        {
            var parser = _parsers.FirstOrDefault(p => p.CanParse(file));
            if (parser != null)
            {
                try
                {
                    allTransactions.AddRange(parser.Parse(file));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao ler o arquivo {Path.GetFileName(file)}: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"Aviso: Nenhum leitor apropriado encontrado para o arquivo {Path.GetFileName(file)}.");
            }
        }

        return allTransactions;
    }
} 