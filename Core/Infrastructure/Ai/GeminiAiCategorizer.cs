using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FinanceHelper.Core.Application.Ports;
using FinanceHelper.Core.Domain.Models;
using GenerativeAI.Types;
using GenerativeAI;

namespace FinanceHelper.Core.Infrastructure.Ai;

public class GeminiAiCategorizer : IAiCategorizer
{
    private readonly GoogleAi _googleAI;
    private readonly GenerationConfig _generationConfig;
    private readonly string _modelName;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public GeminiAiCategorizer(string apiKey, string modelName)
    {
        _googleAI = new GoogleAi(apiKey);
        _modelName = modelName;
        _generationConfig = new GenerationConfig()
        {
            ResponseMimeType = "application/json",
            MaxOutputTokens = 8192,
        };
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
    }

    public async Task<List<CategorizedExpense>?> CategorizeTransactionsAsync(IEnumerable<Transacao> transactions)
    {
        var allCategorizedExpenses = new List<CategorizedExpense>();
        const int pageSize = 50;
        var transactionPages = transactions.Chunk(pageSize).ToList();

        Console.WriteLine($"\nDividindo {transactions.Count()} transações em {transactionPages.Count} páginas de até {pageSize} cada para pré-categorização...");

        for (var i = 0; i < transactionPages.Count; i++)
        {
            Console.WriteLine($"  -> Processando página {i + 1}/{transactionPages.Count}...");
            var categorizedChunk = await CategorizeTransactionsChunkAsync(transactionPages[i]);
            if (categorizedChunk != null)
            {
                allCategorizedExpenses.AddRange(categorizedChunk);
            }
        }

        return allCategorizedExpenses;
    }

    private async Task<List<CategorizedExpense>?> CategorizeTransactionsChunkAsync(IEnumerable<Transacao> transactionChunk)
    {
        var generativeModel = _googleAI.CreateGeminiModel(_modelName, _generationConfig);

        var prompt = new StringBuilder();
        prompt.AppendLine("Você é um assistente financeiro especialista em categorizar transações bancárias.");
        prompt.AppendLine("Analise a lista de transações a seguir e retorne APENAS o array JSON, sem nenhum outro texto ou markdown.");
        prompt.AppendLine("Para cada transação, forneça a data, a descrição original, o valor e uma categoria sugerida em português.");
        prompt.AppendLine("O campo 'valor' DEVE ser um número decimal (JSON number), não uma string. Exemplo: 123.45 ou -50.00. Use o ponto como separador decimal.");
        prompt.AppendLine("As categorias possíveis são: Alimentação, Moradia, Transporte, Saúde, Educação, Lazer, Compras, Serviços, Impostos, Investimentos, Recebimento, Outros.");
        prompt.AppendLine("\nTransações para analisar:");

        var transactionsAsJson = JsonSerializer.Serialize(transactionChunk.Select(t => new { t.Data, t.Descricao, t.Valor }));
        prompt.AppendLine(transactionsAsJson);

        Console.WriteLine("    -> Enviando prompt para a API Gemini...");
        var response = await generativeModel.GenerateContentAsync(prompt.ToString());
        Console.WriteLine("    -> Resposta recebida da API.");
        var jsonResponse = response.Text;

        if (string.IsNullOrWhiteSpace(jsonResponse))
        {
            Console.WriteLine("    -> A resposta da API está vazia.");
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<List<CategorizedExpense>>(jsonResponse, _jsonSerializerOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Falha ao desserializar o lote de categorização. Erro: {ex.Message}\n\n--- CONTEÚDO RECEBIDO ---\n{jsonResponse}\n--------------------------", ex);
        }
    }

    private static string ExtractJson(string text)
    {
        return text;
    }

    private string BuildCategorizationPrompt(IEnumerable<Transacao> transactions)
    {
        var promptBuilder = new StringBuilder();
        promptBuilder.AppendLine("Você é um assistente financeiro especialista em categorizar despesas.");
        promptBuilder.AppendLine("Analise a lista de transações a seguir e retorne um array JSON de objetos.");
        promptBuilder.AppendLine("Cada objeto deve conter os campos 'data' (formato AAAA-MM-DD), 'descricao', 'valor' e 'categoria_sugerida'.");
        promptBuilder.AppendLine("Categorias possíveis: 'Moradia', 'Alimentação', 'Transporte', 'Saúde', 'Lazer', 'Educação', 'Investimentos', 'Compras', 'Serviços', 'Outros'.");
        promptBuilder.AppendLine("Retorne APENAS o array JSON, sem nenhum texto adicional.");
        promptBuilder.AppendLine("\n--- DADOS DAS TRANSAÇÕES ---");
        promptBuilder.AppendLine(JsonSerializer.Serialize(transactions.Select(t => new { t.Data, t.Descricao, t.Valor })));
        
        return promptBuilder.ToString();
    }
} 