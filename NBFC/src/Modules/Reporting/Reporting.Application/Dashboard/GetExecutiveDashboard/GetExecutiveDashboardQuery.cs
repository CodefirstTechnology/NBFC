using Patsanstha.BuildingBlocks.Application.Abstractions.Audit;
using Patsanstha.BuildingBlocks.Application.Abstractions.Caching;
using Patsanstha.Modules.Reporting.Application.Abstractions;

namespace Patsanstha.Modules.Reporting.Application.Dashboard.GetExecutiveDashboard;

public sealed record GetExecutiveDashboardQuery(Guid? BranchId = null) : IQuery<ExecutiveDashboardDto>;

public sealed class GetExecutiveDashboardQueryHandler(
    IDashboardReadStore dashboardReadStore,
    IAuditContextAccessor auditContext,
    IReferenceDataCache referenceDataCache) : IQueryHandler<GetExecutiveDashboardQuery, ExecutiveDashboardDto>
{
    private static readonly TimeSpan DashboardCacheTtl = TimeSpan.FromMinutes(2);

    public async Task<Result<ExecutiveDashboardDto>> Handle(
        GetExecutiveDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = auditContext.Current.TenantId;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<ExecutiveDashboardDto>(
                Error.Validation("Reporting.Tenant.Required", "Tenant context is required."));
        }

        var cacheKey = CacheKeys.DashboardKpi(tenantId, request.BranchId);
        var dashboard = await referenceDataCache.GetOrSetAsync(
            cacheKey,
            ct => dashboardReadStore.GetExecutiveDashboardAsync(tenantId, request.BranchId, ct),
            DashboardCacheTtl,
            cancellationToken);

        return Result.Success(dashboard);
    }
}
