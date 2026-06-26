using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Patsanstha.BuildingBlocks.Infrastructure.Health;
using Patsanstha.Modules.Accounting.Infrastructure.Persistence;
using Patsanstha.Modules.Collections.Infrastructure.Persistence;
using Patsanstha.Modules.Deposits.Infrastructure.Persistence;
using Patsanstha.Modules.Identity.Infrastructure.Persistence;
using Patsanstha.Modules.Loans.Infrastructure.Persistence;
using Patsanstha.Modules.Members.Infrastructure.Persistence;
using Patsanstha.Modules.Recovery.Infrastructure.Persistence;
using Patsanstha.Modules.Reporting.Infrastructure.Persistence;

namespace Patsanstha.Api;

public static class PatsansthaModuleHealthChecks
{
    public static IServiceCollection AddPatsansthaModuleHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var builder = services.AddHealthChecks()
            .AddModuleDbContextCheck<IdentityDbContext>("identity")
            .AddModuleDbContextCheck<MembersDbContext>("members")
            .AddModuleDbContextCheck<DepositsDbContext>("deposits")
            .AddModuleDbContextCheck<LoansDbContext>("loans")
            .AddModuleDbContextCheck<CollectionsDbContext>("collections")
            .AddModuleDbContextCheck<RecoveryDbContext>("recovery")
            .AddModuleDbContextCheck<AccountingDbContext>("accounting")
            .AddModuleDbContextCheck<ReportingDbContext>("reporting");

        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            builder.AddRedis(redisConnection, name: "redis", tags: ["ready"]);
        }

        return services;
    }
}
