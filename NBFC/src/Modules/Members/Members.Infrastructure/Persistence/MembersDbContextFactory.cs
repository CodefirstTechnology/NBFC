using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Patsanstha.BuildingBlocks.Infrastructure.Audit;

namespace Patsanstha.Modules.Members.Infrastructure.Persistence;

public sealed class MembersDbContextFactory : IDesignTimeDbContextFactory<MembersDbContext>
{
    public MembersDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../../Host/Patsanstha.Api"))
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<MembersDbContext>();
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("Members"));

        return new MembersDbContext(optionsBuilder.Options, new AuditContextAccessor());
    }
}
