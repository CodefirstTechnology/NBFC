namespace Patsanstha.BuildingBlocks.Application.Abstractions.Audit;

public interface IAuditContext
{
    Guid? UserId { get; }

    Guid TenantId { get; }

    Guid? BranchId { get; }

    string? CorrelationId { get; }
}

public interface IAuditContextAccessor
{
    IAuditContext Current { get; }
}
