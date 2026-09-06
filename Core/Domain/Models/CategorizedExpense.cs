using System.Text.Json.Serialization;

namespace FinanceHelper.Core.Domain.Models;

public class CategorizedExpense
{
    [JsonPropertyName("data")]
    public DateTime Data { get; set; }

    [JsonPropertyName("descricao")]
    public string Descricao { get; set; } = string.Empty;

    [JsonPropertyName("valor")]
    public decimal Valor { get; set; }

    [JsonPropertyName("categoria_sugerida")]
    public string CategoriaSugerida { get; set; } = string.Empty;
} 