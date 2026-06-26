using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Patsanstha.BuildingBlocks.Infrastructure.Audit;

namespace Patsanstha.Modules.Loans.Infrastructure.Persistence;

public sealed class LoansDbContextFactory : IDesignTimeDbContextFactory<LoansDbContext>
{
    public LoansDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../../Host/Patsanstha.Api"))
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<LoansDbContext>();
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("Loans"));

        return new LoansDbContext(optionsBuilder.Options, new AuditContextAccessor());
    }
}
