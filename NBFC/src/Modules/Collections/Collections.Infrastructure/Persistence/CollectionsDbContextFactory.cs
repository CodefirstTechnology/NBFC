using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Patsanstha.BuildingBlocks.Infrastructure.Audit;

namespace Patsanstha.Modules.Collections.Infrastructure.Persistence;

public sealed class CollectionsDbContextFactory : IDesignTimeDbContextFactory<CollectionsDbContext>
{
    public CollectionsDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../../Host/Patsanstha.Api"))
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<CollectionsDbContext>();
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("Collections"));

        return new CollectionsDbContext(optionsBuilder.Options, new AuditContextAccessor());
    }
}
