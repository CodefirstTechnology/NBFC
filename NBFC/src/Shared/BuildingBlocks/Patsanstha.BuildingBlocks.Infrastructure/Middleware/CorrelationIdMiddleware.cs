using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Patsanstha.BuildingBlocks.Application.Abstractions.Observability;
using Patsanstha.BuildingBlocks.Infrastructure.Observability;

namespace Patsanstha.BuildingBlocks.Infrastructure.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdConstants.HeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        context.Items[CorrelationIdConstants.HttpContextItemKey] = correlationId;
        context.Response.Headers[CorrelationIdConstants.HeaderName] = correlationId;
        context.TraceIdentifier = correlationId;

        using var activity = PatsansthaTelemetry.ActivitySource.StartActivity(
            "http.request",
            ActivityKind.Server);
        activity?.SetTag("correlation.id", correlationId);
        activity?.SetTag("http.route", context.Request.Path.Value);

        using (logger.BeginScope(new Dictionary<string, object>
        {
            [CorrelationIdConstants.LogPropertyName] = correlationId,
        }))
        {
            await next(context);
        }
    }
}

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app) =>
        app.UseMiddleware<CorrelationIdMiddleware>();
}
