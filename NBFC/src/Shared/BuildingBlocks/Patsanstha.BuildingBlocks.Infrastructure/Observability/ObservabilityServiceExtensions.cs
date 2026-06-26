using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Patsanstha.BuildingBlocks.Application.Abstractions.Observability;

namespace Patsanstha.BuildingBlocks.Infrastructure.Observability;

public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    public bool EnableConsoleExporter { get; set; } = true;

    public string? OtlpEndpoint { get; set; }
}

public static class ObservabilityServiceExtensions
{
    public static IServiceCollection AddPatsansthaObservability(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration.GetSection(ObservabilityOptions.SectionName).Get<ObservabilityOptions>()
            ?? new ObservabilityOptions();

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(PatsansthaTelemetry.ServiceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(PatsansthaTelemetry.ActivitySourceName)
                    .AddAspNetCoreInstrumentation(aspNetCoreOptions =>
                    {
                        aspNetCoreOptions.RecordException = true;
                        aspNetCoreOptions.EnrichWithHttpRequest = (activity, request) =>
                        {
                            if (request.Headers.TryGetValue(CorrelationIdConstants.HeaderName, out var correlationId))
                            {
                                activity.SetTag("correlation.id", correlationId.ToString());
                            }
                        };
                    })
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation();

                if (options.EnableConsoleExporter)
                {
                    tracing.AddConsoleExporter();
                }

                if (!string.IsNullOrWhiteSpace(options.OtlpEndpoint))
                {
                    tracing.AddOtlpExporter(otlp => otlp.Endpoint = new Uri(options.OtlpEndpoint));
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                if (options.EnableConsoleExporter)
                {
                    metrics.AddConsoleExporter();
                }

                if (!string.IsNullOrWhiteSpace(options.OtlpEndpoint))
                {
                    metrics.AddOtlpExporter(otlp => otlp.Endpoint = new Uri(options.OtlpEndpoint));
                }
            });

        return services;
    }
}
