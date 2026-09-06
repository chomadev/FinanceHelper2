using FinanceHelper.Core.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceHelper.Core.Infrastructure.Database;

public class FinanceHelperDbContext : DbContext
{
    public DbSet<Transacao> Transacoes { get; set; }
    public DbSet<AnalysisResult> AnalysisResults { get; set; }

    public FinanceHelperDbContext(DbContextOptions<FinanceHelperDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AnalysisResult>()
            .HasIndex(ar => ar.TransactionHash)
            .IsUnique();
    }
} 