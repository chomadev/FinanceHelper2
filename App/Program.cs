using FinanceHelper.Core.Application.Ports;
using FinanceHelper.Core.Application.Services;
using FinanceHelper.Core.Domain.Models;
using FinanceHelper.Core.Infrastructure.Ai;
using FinanceHelper.Core.Infrastructure.Database;
using FinanceHelper.Core.Infrastructure.File;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

Console.WriteLine("🚀 Finance Helper - Iniciando...");

// 1. Carregar Configurações do appsettings.json
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var apiKey = configuration["ApiKeys:Gemini"];
var connectionString = configuration.GetConnectionString("DefaultConnection");
var modelName = configuration["GeminiSettings:ModelName"];

if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(modelName))
{
    Console.WriteLine("ERRO: A chave 'ApiKeys:Gemini', a ConnectionString 'DefaultConnection' e 'GeminiSettings:ModelName' devem estar definidas no arquivo appsettings.json.");
    Console.ReadKey();
    return;
}

// Habilita a serialização JSON dinâmica para o Npgsql
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.EnableDynamicJson();
var dataSource = dataSourceBuilder.Build();

// 2. Localizar o diretório de dados
var projectRoot = Directory.GetCurrentDirectory();
if (Path.GetFileName(projectRoot) == "App")
{
    projectRoot = Directory.GetParent(projectRoot)?.FullName ?? projectRoot;
}
var sampleDataPath = Path.Combine(projectRoot, "sampledata"); //..\\..\\..\\..\\

if (!Directory.Exists(sampleDataPath))
{
    Console.WriteLine($"❌ Erro: O diretório de dados em '{sampleDataPath}' não foi encontrado.");
    return;
}

try
{
    // 3. Configurar e Iniciar o Banco de Dados
    var dbContextOptions = new DbContextOptionsBuilder<FinanceHelperDbContext>()
        .UseNpgsql(dataSource)
        .Options;

    await using var dbContext = new FinanceHelperDbContext(dbContextOptions);
    
    Console.WriteLine("✔️ Conectando ao banco de dados e aplicando migrations...");
    await dbContext.Database.MigrateAsync();
    Console.WriteLine("✔️ Banco de dados pronto.");

    // 4. Configurar e executar o serviço de análise (Arquitetura Hexagonal)
    
    // Registra todos os parsers de arquivo
    var parsers = new List<IFileParser>
    {
        new NuBankCsvParser(),
        new C6CsvParser()
    };
    
    ITransactionReader transactionReader = new CompositeTransactionReader(parsers);
    IAnalysisResultRepository analysisRepository = new EfAnalysisResultRepository(dbContext);
    IAiCategorizer aiCategorizer = new GeminiAiCategorizer(apiKey, modelName);
    var analysisService = new TransactionAnalysisService(transactionReader, aiCategorizer, analysisRepository);
    
    var analysisResult = await analysisService.GetAnalysisForDirectoryAsync(sampleDataPath);

    Console.Clear();
    Console.WriteLine("--- Análise Financeira da Inteligência Artificial ---");

    if (analysisResult == null)
    {
        Console.WriteLine("Não foi possível gerar a análise. Verifique os logs ou a resposta da API.");
    }
    else
    {
        PrintAnalysis(analysisResult);

        Console.WriteLine("\n💾 Salvando transações analisadas no banco de dados...");
        var transacoesParaSalvar = new List<Transacao>();

        // Processa despesas recorrentes
        foreach (var despesa in analysisResult.RecurringExpenses)
        {
            transacoesParaSalvar.Add(new Transacao
            {
                Data = DateOnly.FromDateTime(despesa.Data),
                Descricao = despesa.Descricao,
                Valor = despesa.Valor,
                CategoriaSugerida = despesa.CategoriaSugerida,
                IsRecorrente = true,
                Identificador = Guid.NewGuid().ToString()
            });
        }

        // Processa despesas eventuais
        foreach (var despesa in analysisResult.EventualExpenses)
        {
            transacoesParaSalvar.Add(new Transacao
            {
                Data = DateOnly.FromDateTime(despesa.Data),
                Descricao = despesa.Descricao,
                Valor = despesa.Valor,
                CategoriaSugerida = despesa.CategoriaSugerida,
                IsRecorrente = false,
                Identificador = Guid.NewGuid().ToString()
            });
        }

        await dbContext.Transacoes.AddRangeAsync(transacoesParaSalvar);
        await dbContext.SaveChangesAsync();
        Console.WriteLine("✔️ Transações salvas com sucesso.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ Ocorreu um erro inesperado durante a execução: {ex.Message}");
    Console.WriteLine($"Detalhes: {ex}");
}

Console.WriteLine("\n👋 Análise finalizada.");

void PrintAnalysis(FinancialAnalysis analysis)
{
    Console.WriteLine("\n--- 📊 Distribuição de Gastos por Categoria ---");
    if(analysis.CategoryDistribution.Any())
    {
        foreach (var dist in analysis.CategoryDistribution.OrderByDescending(d => d.Value.Percentual))
        {
            Console.WriteLine($"  - {dist.Key,-20} | {dist.Value.ValorTotal,10:C} | {dist.Value.Percentual:F2}%");
        }
    }
    else
    {
        Console.WriteLine("  Nenhuma distribuição de categoria foi gerada.");
    }


    Console.WriteLine("\n--- 🔁 Despesas Recorrentes Identificadas ---");
    if (analysis.RecurringExpenses.Any())
    {
        foreach (var expense in analysis.RecurringExpenses)
        {
            Console.WriteLine($"  - {expense.Data:dd/MM/yyyy}: {expense.Descricao,-40} | {expense.Valor,10:C} | Categoria: {expense.CategoriaSugerida}");
        }
    }
    else
    {
        Console.WriteLine("  Nenhuma despesa recorrente foi claramente identificada.");
    }

    Console.WriteLine("\n--- ⚡ Despesas Eventuais Identificadas ---");
    if (analysis.EventualExpenses.Any())
    {
        foreach (var expense in analysis.EventualExpenses)
        {
            Console.WriteLine($"  - {expense.Data:dd/MM/yyyy}: {expense.Descricao,-40} | {expense.Valor,10:C} | Categoria: {expense.CategoriaSugerida}");
        }
    }
    else
    {
        Console.WriteLine("  Nenhuma despesa eventual foi identificada.");
    }
}
