using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceHelper.Core.Domain.Models;

[Table("analysis_results")]
public class AnalysisResult
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("transactions_hash")]
    public required string TransactionHash { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("analysis", TypeName = "jsonb")]
    public required FinancialAnalysis Analysis { get; set; }
} 