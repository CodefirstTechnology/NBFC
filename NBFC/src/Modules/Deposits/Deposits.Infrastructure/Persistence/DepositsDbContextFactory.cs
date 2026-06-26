using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Patsanstha.BuildingBlocks.Infrastructure.Audit;

namespace Patsanstha.Modules.Deposits.Infrastructure.Persistence;

public sealed class DepositsDbContextFactory : IDesignTimeDbContextFactory<DepositsDbContext>
{
    public DepositsDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../../Host/Patsanstha.Api"))
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<DepositsDbContext>();
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("Deposits"));

        return new DepositsDbContext(optionsBuilder.Options, new AuditContextAccessor());
    }
}
