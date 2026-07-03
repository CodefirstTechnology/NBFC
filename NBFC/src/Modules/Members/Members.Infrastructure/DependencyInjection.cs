using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Patsanstha.BuildingBlocks.Application.Abstractions.Outbox;
using Patsanstha.BuildingBlocks.Infrastructure;
using Patsanstha.BuildingBlocks.Infrastructure.Persistence.Outbox;
using Patsanstha.Modules.Members.Application.Abstractions;
using Patsanstha.Modules.Members.Infrastructure.Options;
using Patsanstha.Modules.Members.Infrastructure.Persistence;
using Patsanstha.Modules.Members.Infrastructure.Security;
using Patsanstha.Modules.Members.Infrastructure.Storage;

namespace Patsanstha.Modules.Members.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMembersModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PiiEncryptionOptions>(configuration.GetSection(PiiEncryptionOptions.SectionName));
        services.AddOptions<MemberDocumentStorageOptions>()
            .Bind(configuration.GetSection(MemberDocumentStorageOptions.SectionName))
            .PostConfigure<IHostEnvironment>((options, environment) =>
            {
                options.RootPath = Path.GetFullPath(
                    Path.IsPathRooted(options.RootPath)
                        ? options.RootPath
                        : Path.Combine(environment.ContentRootPath, options.RootPath));

                Directory.CreateDirectory(options.RootPath);
            });

        services.AddDbContext<MembersDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Members"));
            options.AddBuildingBlocksInterceptors(sp);
        });

        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<IMemberNumberGenerator, MemberNumberGenerator>();
        services.AddScoped<IMemberMapper, MemberMapper>();
        services.AddSingleton<IPiiEncryptionService, AesPiiEncryptionService>();
        services.AddSingleton<IMemberDocumentStorage, LocalMemberDocumentStorage>();
        services.AddScoped<IOutboxDbContext>(sp => sp.GetRequiredService<MembersDbContext>());

        return services;
    }
}
