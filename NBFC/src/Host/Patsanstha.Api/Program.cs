using Hangfire;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Patsanstha.Api;
using Patsanstha.BuildingBlocks.Infrastructure;
using Patsanstha.BuildingBlocks.Infrastructure.BackgroundJobs;
using Patsanstha.BuildingBlocks.Infrastructure.Health;
using Patsanstha.BuildingBlocks.Infrastructure.Middleware;
using Patsanstha.BuildingBlocks.Infrastructure.Observability;
using Patsanstha.Modules.Identity.Api;
using Patsanstha.Modules.Identity.Application;
using Patsanstha.Modules.Identity.Infrastructure;
using Patsanstha.Modules.Identity.Infrastructure.Hosting;
using Patsanstha.Modules.Members.Api;
using Patsanstha.Modules.Members.Application;
using Patsanstha.Modules.Members.Infrastructure;
using Patsanstha.Modules.Members.Infrastructure.Hosting;
using Patsanstha.Modules.Deposits.Api;
using Patsanstha.Modules.Deposits.Application;
using Patsanstha.Modules.Deposits.Infrastructure;
using Patsanstha.Modules.Deposits.Infrastructure.Hosting;
using Patsanstha.Modules.Loans.Api;
using Patsanstha.Modules.Loans.Application;
using Patsanstha.Modules.Loans.Infrastructure;
using Patsanstha.Modules.Loans.Infrastructure.Hosting;
using Patsanstha.Modules.Recovery.Api;
using Patsanstha.Modules.Recovery.Application;
using Patsanstha.Modules.Recovery.Infrastructure;
using Patsanstha.Modules.Recovery.Infrastructure.Hosting;
using Patsanstha.Modules.Collections.Api;
using Patsanstha.Modules.Collections.Application;
using Patsanstha.Modules.Collections.Infrastructure;
using Patsanstha.Modules.Collections.Infrastructure.Hosting;
using Patsanstha.Modules.Reporting.Api;
using Patsanstha.Modules.Reporting.Application;
using Patsanstha.Modules.Reporting.Infrastructure;
using Patsanstha.Modules.Reporting.Infrastructure.Hosting;
using Patsanstha.Modules.Accounting.Api;
using Patsanstha.Modules.Accounting.Application;
using Patsanstha.Modules.Accounting.Infrastructure;
using Patsanstha.Modules.Accounting.Infrastructure.Hosting;
using Serilog;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new CompactJsonFormatter())
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, _, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "Patsanstha.Api")
        .WriteTo.Console(new CompactJsonFormatter()));

    builder.Services.AddBuildingBlocksInfrastructure(builder.Configuration);
    builder.Services.AddPatsansthaObservability(builder.Configuration);
    builder.Services.AddIdentityApplication();
    builder.Services.AddIdentityModule(builder.Configuration);
    builder.Services.AddMembersApplication();
    builder.Services.AddMembersModule(builder.Configuration);
    builder.Services.AddDepositsApplication();
    builder.Services.AddDepositsModule(builder.Configuration);
    builder.Services.AddLoansApplication();
    builder.Services.AddLoansModule(builder.Configuration);
    builder.Services.AddRecoveryApplication();
    builder.Services.AddRecoveryModule(builder.Configuration);
    builder.Services.AddCollectionsApplication();
    builder.Services.AddCollectionsModule(builder.Configuration);
    builder.Services.AddReportingApplication();
    builder.Services.AddReportingModule(builder.Configuration);
    builder.Services.AddAccountingApplication();
    builder.Services.AddAccountingModule(builder.Configuration);
    builder.Services.AddPatsansthaModuleHealthChecks(builder.Configuration);
    builder.Services.AddPatsansthaBackgroundJobs(builder.Configuration);
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new() { Title = "Patsanstha API", Version = "v1" });
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("FrontendDev", policy =>
            policy.WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithExposedHeaders("X-Correlation-Id"));
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseCors("FrontendDev");
        app.MapHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = [app.Services.GetRequiredService<HangfireDashboardAuthorizationFilter>()],
        });
        await app.SeedIdentityDataAsync();
        await app.MigrateMembersSchemaAsync();
        await app.MigrateDepositsSchemaAsync();
        await app.MigrateLoansSchemaAsync();
        await app.MigrateRecoverySchemaAsync();
        await app.MigrateCollectionsSchemaAsync();
        await app.MigrateAccountingSchemaAsync();
        await app.MigrateReportingSchemaAsync();
    }

    app.UseExceptionHandling();
    app.UseCorrelationId();
    app.UseAuthentication();
    app.UseAuditContext();
    app.UseAuthorization();

    app.MapGet("/health/live", () => Results.Ok(new { status = "live" }))
        .WithTags("Health")
        .WithName("HealthLive");

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = ModuleHealthCheckResponseWriter.WriteAsync,
    })
        .WithTags("Health")
        .WithName("HealthReady");

    app.MapIdentityEndpoints();
    app.MapMembersEndpoints();
    app.MapDepositsEndpoints();
    app.MapLoansEndpoints();
    app.MapRecoveryEndpoints();
    app.MapCollectionsEndpoints();
    app.MapAccountingEndpoints();
    app.MapReportingEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
