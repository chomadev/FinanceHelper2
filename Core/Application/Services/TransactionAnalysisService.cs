using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FinanceHelper.Core.Application.Ports;
using FinanceHelper.Core.Domain.Models;

namespace FinanceHelper.Core.Application.Services;

public class TransactionAnalysisService
{
    private readonly ITransactionReader _transactionReader;
    private readonly IAiCategorizer _aiCategorizer;
    private readonly IAnalysisResultRepository _analysisResultRepository;

    public TransactionAnalysisService(
        ITransactionReader transactionReader, 
        IAiCategorizer aiCategorizer, 
        IAnalysisResultRepository analysisResultRepository)
    {
        _transactionReader = transactionReader;
        _aiCategorizer = aiCategorizer;
        _analysisResultRepository = analysisResultRepository;
    }

    public async Task<FinancialAnalysis?> GetAnalysisForDirectoryAsync(string directoryPath)
    {
        var filePaths = Directory.GetFiles(directoryPath, "*.csv", SearchOption.AllDirectories);
        
        var transactions = _transactionReader.ReadTransactions(filePaths);
        if (transactions.Count == 0)
        {
            Console.WriteLine("⚠️ Nenhum arquivo de transação encontrado ou os arquivos estão vazios.");
            return null;
        }
        
        var hash = ComputeTransactionHash(transactions);
        var cachedAnalysis = await _analysisResultRepository.GetByHashAsync(hash);

        if (cachedAnalysis != null)
        {
            Console.WriteLine("✅ Análise encontrada no cache. Retornando resultado salvo.");
            return cachedAnalysis.Analysis;
        }

        Console.WriteLine("🤖 Nenhuma análise encontrada no cache. Chamando a IA para categorização...");
        var categorizedExpenses = await _aiCategorizer.CategorizeTransactionsAsync(transactions);

        if (categorizedExpenses == null || !categorizedExpenses.Any())
        {
            Console.WriteLine("A IA não retornou nenhuma categoria.");
            return null;
        }
        
        Console.WriteLine("⚙️ Gerando análise final a partir das categorias...");
        var finalAnalysis = GenerateFinalAnalysis(categorizedExpenses);

        await _analysisResultRepository.SaveAsync(new AnalysisResult { TransactionHash = hash, Analysis = finalAnalysis });
        Console.WriteLine("💾 Análise da IA salva no cache para futuras consultas.");

        return finalAnalysis;
    }

    private FinancialAnalysis GenerateFinalAnalysis(List<CategorizedExpense> expenses)
    {
        var analysis = new FinancialAnalysis();

        // 1. Identificar despesas recorrentes e eventuais
        var expenseGroups = expenses.GroupBy(e => e.Descricao.Trim().ToLower());
        foreach (var group in expenseGroups)
        {
            if (group.Count() > 1) // Simples heurística: mais de uma ocorrência é recorrente
            {
                analysis.RecurringExpenses.AddRange(group);
            }
            else
            {
                analysis.EventualExpenses.AddRange(group);
            }
        }
        
        // 2. Calcular distribuição por categoria
        var totalAmount = expenses.Sum(e => e.Valor);
        var categoryGroups = expenses.GroupBy(e => e.CategoriaSugerida);

        foreach (var group in categoryGroups)
        {
            var categoryTotal = group.Sum(e => e.Valor);
            analysis.CategoryDistribution[group.Key] = new CategoryDistribution
            {
                ValorTotal = categoryTotal,
                Percentual = totalAmount != 0 ? (double)(categoryTotal / totalAmount) * 100 : 0
            };
        }

        return analysis;
    }

    private string ComputeTransactionHash(IEnumerable<Transacao> transactions)
    {
        var json = JsonSerializer.Serialize(transactions);
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(json));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
} 