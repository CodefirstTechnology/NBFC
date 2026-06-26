using Patsanstha.Modules.Recovery.Domain.Entities;
using Patsanstha.Modules.Recovery.Domain.Enums;

namespace Patsanstha.Modules.Recovery.Application.Abstractions;

public interface IRecoveryCaseRepository
{
    Task AddAsync(RecoveryCase recoveryCase, CancellationToken cancellationToken = default);

    Task<RecoveryCase?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<RecoveryCase> Items, int TotalCount)> ListAsync(
        ListRecoveryCasesCriteria criteria,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ICaseNumberGenerator
{
    Task<string> GenerateNextAsync(CancellationToken cancellationToken = default);
}

public interface IRecoveryCaseMapper
{
    RecoveryCaseSummaryDto ToSummary(RecoveryCase recoveryCase);

    RecoveryCaseDetailDto ToDetail(RecoveryCase recoveryCase);
}
