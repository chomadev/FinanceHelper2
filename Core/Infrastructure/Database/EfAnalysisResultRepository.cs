using FinanceHelper.Core.Application.Ports;
using FinanceHelper.Core.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceHelper.Core.Infrastructure.Database;

public class EfAnalysisResultRepository : IAnalysisResultRepository
{
    private readonly FinanceHelperDbContext _context;

    public EfAnalysisResultRepository(FinanceHelperDbContext context)
    {
        _context = context;
    }

    public async Task<AnalysisResult?> GetByHashAsync(string hash)
    {
        return await _context.AnalysisResults
            .FirstOrDefaultAsync(ar => ar.TransactionHash == hash);
    }

    public async Task SaveAsync(AnalysisResult analysisResult)
    {
        await _context.AnalysisResults.AddAsync(analysisResult);
        await _context.SaveChangesAsync();
    }
} 