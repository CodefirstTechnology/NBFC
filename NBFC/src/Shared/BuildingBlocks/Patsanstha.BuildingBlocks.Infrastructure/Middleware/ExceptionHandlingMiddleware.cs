using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Patsanstha.BuildingBlocks.Infrastructure.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (BadHttpRequestException ex) when (IsJsonBindingFailure(ex))
        {
            logger.LogWarning(
                ex,
                "Invalid JSON request body for {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            if (context.Response.HasStarted)
            {
                throw;
            }

            await Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "invalid_request_body",
                detail: BuildJsonBindingDetail(ex, environment),
                type: "https://patsanstha.local/errors/validation",
                extensions: new Dictionary<string, object?>
                {
                    ["traceId"] = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier,
                })
                .ExecuteAsync(context);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unhandled exception for {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var detail = environment.IsDevelopment()
                ? ex.Message
                : "An unexpected error occurred. Please retry or contact support.";

            await Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "internal_server_error",
                detail: detail,
                type: "https://patsanstha.local/errors/internal",
                extensions: environment.IsDevelopment()
                    ? new Dictionary<string, object?>
                    {
                        ["traceId"] = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier,
                    }
                    : null)
                .ExecuteAsync(context);
        }
    }

    private static bool IsJsonBindingFailure(BadHttpRequestException ex) =>
        ex.Message.Contains("from the request body as JSON", StringComparison.OrdinalIgnoreCase);

    private static string BuildJsonBindingDetail(BadHttpRequestException ex, IHostEnvironment environment)
    {
        var inner = ex.InnerException?.Message;
        if (!string.IsNullOrWhiteSpace(inner))
        {
            if (inner.Contains("DateOnly", StringComparison.OrdinalIgnoreCase)
                || inner.Contains("dateOfBirth", StringComparison.OrdinalIgnoreCase))
            {
                return "Date of birth must be a valid date in YYYY-MM-DD format.";
            }

            if (inner.Contains("Guid", StringComparison.OrdinalIgnoreCase)
                || inner.Contains("branchId", StringComparison.OrdinalIgnoreCase))
            {
                return "Branch ID must be a valid GUID.";
            }

            return environment.IsDevelopment() ? inner : "The request body contains invalid JSON or field values.";
        }

        return "The request body could not be read as JSON. Check field formats and try again.";
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app) =>
        app.UseMiddleware<ExceptionHandlingMiddleware>();
}
