using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Patsanstha.BuildingBlocks.Application.Abstractions.Observability;
using Patsanstha.BuildingBlocks.Infrastructure.Audit;

namespace Patsanstha.BuildingBlocks.Infrastructure.Middleware;

public sealed class AuditContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, AuditContextAccessor auditContextAccessor)
    {
        var tenantIdHeader = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
        var branchIdHeader = context.Request.Headers["X-Branch-Id"].FirstOrDefault();
        var userIdClaim = context.User.FindFirst("sub")?.Value
            ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        Guid? userId = Guid.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : null;
        Guid tenantId = Guid.TryParse(tenantIdHeader, out var parsedTenantId) ? parsedTenantId : Guid.Empty;
        Guid? branchId = Guid.TryParse(branchIdHeader, out var parsedBranchId) ? parsedBranchId : null;

        if (context.User.Identity?.IsAuthenticated == true)
        {
            userId = Guid.TryParse(
                context.User.FindFirstValue("sub") ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var claimUserId)
                ? claimUserId
                : userId;

            tenantId = Guid.TryParse(context.User.FindFirstValue("tenant_id"), out var claimTenantId)
                ? claimTenantId
                : tenantId;

            branchId = Guid.TryParse(context.User.FindFirstValue("branch_id"), out var claimBranchId)
                ? claimBranchId
                : branchId;
        }

        var correlationId = context.Items[CorrelationIdConstants.HttpContextItemKey]?.ToString()
            ?? context.TraceIdentifier;

        auditContextAccessor.Set(new AuditContext
        {
            UserId = userId,
            TenantId = tenantId,
            BranchId = branchId,
            CorrelationId = correlationId,
        });

        try
        {
            await next(context);
        }
        finally
        {
            auditContextAccessor.Clear();
        }
    }
}

public static class AuditContextMiddlewareExtensions
{
    public static IApplicationBuilder UseAuditContext(this IApplicationBuilder app) =>
        app.UseMiddleware<AuditContextMiddleware>();
}
