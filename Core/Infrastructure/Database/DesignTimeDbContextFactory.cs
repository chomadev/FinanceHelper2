using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FinanceHelper.Core.Infrastructure.Database;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FinanceHelperDbContext>
{
    public FinanceHelperDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FinanceHelperDbContext>();
        // Esta string de conexão é apenas para tempo de design, para criar a migration.
        // A string real será fornecida em tempo de execução.
        optionsBuilder.UseNpgsql("Host=localhost;Database=finance_helper_db;Username=postgres;Password=password");

        return new FinanceHelperDbContext(optionsBuilder.Options);
    }
} 