using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Patsanstha.BuildingBlocks.Application.Abstractions.Outbox;
using Patsanstha.BuildingBlocks.Infrastructure;
using Patsanstha.BuildingBlocks.Infrastructure.Persistence.Outbox;
using Patsanstha.Modules.Members.Application.Abstractions;
using Patsanstha.Modules.Members.Infrastructure.Options;
using Patsanstha.Modules.Members.Infrastructure.Persistence;
using Patsanstha.Modules.Members.Infrastructure.Security;

namespace Patsanstha.Modules.Members.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMembersModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PiiEncryptionOptions>(configuration.GetSection(PiiEncryptionOptions.SectionName));

        services.AddDbContext<MembersDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Members"));
            options.AddBuildingBlocksInterceptors(sp);
        });

        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<IMemberNumberGenerator, MemberNumberGenerator>();
        services.AddScoped<IMemberMapper, MemberMapper>();
        services.AddSingleton<IPiiEncryptionService, AesPiiEncryptionService>();
        services.AddScoped<IOutboxDbContext>(sp => sp.GetRequiredService<MembersDbContext>());

        return services;
    }
}
