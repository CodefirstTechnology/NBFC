using Patsanstha.BuildingBlocks.Application.Abstractions.Audit;

namespace Patsanstha.BuildingBlocks.Infrastructure.Audit;

public sealed class AuditContext : IAuditContext
{
    public Guid? UserId { get; init; }

    public Guid TenantId { get; init; }

    public Guid? BranchId { get; init; }

    public string? CorrelationId { get; init; }

    public static AuditContext System { get; } = new()
    {
        UserId = null,
        TenantId = Guid.Empty,
        BranchId = null,
        CorrelationId = null,
    };
}

public sealed class AuditContextAccessor : IAuditContextAccessor
{
    private static readonly AsyncLocal<AuditContext?> CurrentContext = new();

    public IAuditContext Current => CurrentContext.Value ?? AuditContext.System;

    public void Set(AuditContext context) => CurrentContext.Value = context;

    public void Clear() => CurrentContext.Value = null;
}
