using FinanceHelper.Core.Domain.Models;

namespace FinanceHelper.Core.Application.Ports;

public interface IAnalysisResultRepository
{
    Task<AnalysisResult?> GetByHashAsync(string hash);
    Task SaveAsync(AnalysisResult analysisResult);
} 