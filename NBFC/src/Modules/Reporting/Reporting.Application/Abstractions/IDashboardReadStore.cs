namespace Patsanstha.Modules.Reporting.Application.Abstractions;

public interface IDashboardReadStore
{
    Task<ExecutiveDashboardDto> GetExecutiveDashboardAsync(
        Guid tenantId,
        Guid? branchId,
        CancellationToken cancellationToken = default);
}
