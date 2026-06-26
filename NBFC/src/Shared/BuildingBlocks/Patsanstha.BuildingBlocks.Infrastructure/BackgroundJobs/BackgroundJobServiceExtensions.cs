using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Patsanstha.BuildingBlocks.Infrastructure.BackgroundJobs;

public sealed class HangfireDashboardAuthorizationFilter(IWebHostEnvironment environment) : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => environment.IsDevelopment();
}

public sealed class RecurringBackgroundJobRegistrar(
    IRecurringJobManager recurringJobManager,
    IOptions<BackgroundJobOptions> options) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled)
        {
            return Task.CompletedTask;
        }

        var jobOptions = options.Value;

        recurringJobManager.AddOrUpdate<EmiGenerationJob>(
            "emi-generation",
            job => job.ExecuteAsync(CancellationToken.None),
            jobOptions.EmiGenerationCron);

        recurringJobManager.AddOrUpdate<InterestAccrualJob>(
            "interest-accrual",
            job => job.ExecuteAsync(CancellationToken.None),
            jobOptions.InterestAccrualCron);

        recurringJobManager.AddOrUpdate<AgeingRecalculationJob>(
            "ageing-recalculation",
            job => job.ExecuteAsync(CancellationToken.None),
            jobOptions.AgeingRecalculationCron);

        recurringJobManager.AddOrUpdate<NotificationDispatchJob>(
            "notification-dispatch",
            job => job.ExecuteAsync(CancellationToken.None),
            jobOptions.NotificationDispatchCron);

        recurringJobManager.AddOrUpdate<ReportPreAggregationJob>(
            "report-pre-aggregation",
            job => job.ExecuteAsync(CancellationToken.None),
            jobOptions.ReportPreAggregationCron);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

public static class BackgroundJobServiceExtensions
{
    public static IServiceCollection AddPatsansthaBackgroundJobs(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<BackgroundJobOptions>(configuration.GetSection(BackgroundJobOptions.SectionName));

        var hangfireConnection = configuration.GetConnectionString("Hangfire")
            ?? configuration.GetConnectionString("Identity")
            ?? throw new InvalidOperationException("Hangfire or Identity connection string is required.");

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(
                bootstrap => bootstrap.UseNpgsqlConnection(hangfireConnection),
                new PostgreSqlStorageOptions { SchemaName = "hangfire" }));

        services.AddHangfireServer();
        services.AddSingleton<HangfireDashboardAuthorizationFilter>();
        services.AddScoped<EmiGenerationJob>();
        services.AddScoped<InterestAccrualJob>();
        services.AddScoped<AgeingRecalculationJob>();
        services.AddScoped<NotificationDispatchJob>();
        services.AddScoped<ReportPreAggregationJob>();
        services.AddHostedService<RecurringBackgroundJobRegistrar>();

        return services;
    }
}
