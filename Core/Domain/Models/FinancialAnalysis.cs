using System.Text.Json.Serialization;

namespace FinanceHelper.Core.Domain.Models;

public class FinancialAnalysis
{
    [JsonPropertyName("despesas_recorrentes")]
    public List<CategorizedExpense> RecurringExpenses { get; set; } = new();

    [JsonPropertyName("despesas_eventuais")]
    public List<CategorizedExpense> EventualExpenses { get; set; } = new();

    [JsonPropertyName("distribuicao_categorias")]
    public Dictionary<string, CategoryDistribution> CategoryDistribution { get; set; } = new();
}

public class CategoryDistribution
{
    [JsonPropertyName("valor_total")]
    public decimal ValorTotal { get; set; }
    
    [JsonPropertyName("percentual")]
    public double Percentual { get; set; }
} 