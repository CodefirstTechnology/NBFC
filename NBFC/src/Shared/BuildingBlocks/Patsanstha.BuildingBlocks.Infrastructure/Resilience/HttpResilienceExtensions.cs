using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Patsanstha.BuildingBlocks.Application.Abstractions.Observability;

namespace Patsanstha.BuildingBlocks.Infrastructure.Resilience;

public static class ExternalHttpClientNames
{
    public const string SmsGateway = "external-sms";
    public const string EKycProvider = "external-ekyc";
    public const string PaymentGateway = "external-payment";
    public const string UpiProvider = "external-upi";
}

public static class HttpResilienceExtensions
{
    public static IServiceCollection AddPatsansthaResilientHttpClients(this IServiceCollection services)
    {
        RegisterClient(services, ExternalHttpClientNames.SmsGateway);
        RegisterClient(services, ExternalHttpClientNames.EKycProvider);
        RegisterClient(services, ExternalHttpClientNames.PaymentGateway);
        RegisterClient(services, ExternalHttpClientNames.UpiProvider);

        services.AddTransient<CorrelationIdDelegatingHandler>();

        return services;
    }

    public static IHttpClientBuilder AddPatsansthaResilience(this IHttpClientBuilder builder)
    {
        builder.AddHttpMessageHandler<CorrelationIdDelegatingHandler>();
        builder.AddStandardResilienceHandler(options =>
        {
            options.Retry.MaxRetryAttempts = 3;
            options.Retry.Delay = TimeSpan.FromMilliseconds(500);
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
            options.CircuitBreaker.FailureRatio = 0.5;
            options.CircuitBreaker.MinimumThroughput = 5;
            options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
        });

        return builder;
    }

    private static void RegisterClient(IServiceCollection services, string clientName) =>
        services.AddHttpClient(clientName)
            .AddPatsansthaResilience();
}

public sealed class CorrelationIdDelegatingHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var correlationId = httpContextAccessor.HttpContext?.Items[CorrelationIdConstants.HttpContextItemKey]?.ToString();
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            request.Headers.TryAddWithoutValidation(CorrelationIdConstants.HeaderName, correlationId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
