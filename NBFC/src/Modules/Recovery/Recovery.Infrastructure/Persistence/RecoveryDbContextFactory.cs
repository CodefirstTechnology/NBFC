using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Patsanstha.BuildingBlocks.Infrastructure.Audit;

namespace Patsanstha.Modules.Recovery.Infrastructure.Persistence;

public sealed class RecoveryDbContextFactory : IDesignTimeDbContextFactory<RecoveryDbContext>
{
    public RecoveryDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../../Host/Patsanstha.Api"))
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<RecoveryDbContext>();
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("Recovery"));

        return new RecoveryDbContext(optionsBuilder.Options, new AuditContextAccessor());
    }
}
