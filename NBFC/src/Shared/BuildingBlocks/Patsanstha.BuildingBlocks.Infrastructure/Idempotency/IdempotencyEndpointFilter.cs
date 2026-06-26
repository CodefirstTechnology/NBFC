using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Patsanstha.BuildingBlocks.Application.Abstractions.Idempotency;
using Patsanstha.BuildingBlocks.Infrastructure.Api;
using Patsanstha.BuildingBlocks.Infrastructure.Caching;

namespace Patsanstha.BuildingBlocks.Infrastructure.Idempotency;

public sealed class IdempotencyEndpointFilter(
    IIdempotencyStore idempotencyStore,
    IOptions<CacheOptions> cacheOptions) : IEndpointFilter
{
    private static readonly HashSet<string> MutatingMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        HttpMethods.Post,
        HttpMethods.Put,
        HttpMethods.Patch,
        HttpMethods.Delete,
    };

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        if (!MutatingMethods.Contains(httpContext.Request.Method))
        {
            return await next(context);
        }

        if (!httpContext.Request.Headers.TryGetValue(IdempotencyConstants.HeaderName, out var keyValues)
            || string.IsNullOrWhiteSpace(keyValues.FirstOrDefault()))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "idempotency_key_required",
                detail: $"The {IdempotencyConstants.HeaderName} header is required for this operation.",
                type: "https://patsanstha.local/errors/validation");
        }

        var idempotencyKey = keyValues.ToString();
        var existing = await idempotencyStore.GetAsync(idempotencyKey, httpContext.RequestAborted);
        if (existing is not null)
        {
            return new ReplayIdempotencyResult(existing);
        }

        var result = await next(context);
        if (result is not IResult typedResult)
        {
            return result;
        }

        var originalBody = httpContext.Response.Body;
        await using var capture = new MemoryStream();
        httpContext.Response.Body = capture;

        try
        {
            await typedResult.ExecuteAsync(httpContext);

            capture.Position = 0;
            var body = await new StreamReader(capture, Encoding.UTF8).ReadToEndAsync(httpContext.RequestAborted);
            var statusCode = httpContext.Response.StatusCode;
            var contentType = httpContext.Response.ContentType ?? "application/json";

            if (statusCode is >= 200 and < 300)
            {
                await idempotencyStore.StoreAsync(
                    new IdempotencyRecord(
                        idempotencyKey,
                        statusCode,
                        contentType,
                        body,
                        DateTimeOffset.UtcNow),
                    cacheOptions.Value.IdempotencyTtl,
                    httpContext.RequestAborted);
            }

            capture.Position = 0;
            await capture.CopyToAsync(originalBody, httpContext.RequestAborted);
        }
        finally
        {
            httpContext.Response.Body = originalBody;
        }

        return Results.Empty;
    }

    private sealed class ReplayIdempotencyResult(IdempotencyRecord record) : IResult
    {
        public Task ExecuteAsync(HttpContext httpContext)
        {
            httpContext.Response.StatusCode = record.StatusCode;
            httpContext.Response.ContentType = record.ContentType;
            return httpContext.Response.WriteAsync(record.Body);
        }
    }
}

public static class IdempotencyEndpointExtensions
{
    public static RouteHandlerBuilder RequireIdempotency(this RouteHandlerBuilder builder) =>
        builder.AddEndpointFilter<IdempotencyEndpointFilter>();
}
