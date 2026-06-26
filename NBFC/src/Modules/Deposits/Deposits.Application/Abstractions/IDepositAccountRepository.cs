using Patsanstha.Modules.Deposits.Domain.Entities;
using Patsanstha.Modules.Deposits.Domain.Enums;

namespace Patsanstha.Modules.Deposits.Application.Abstractions;

public interface IDepositAccountRepository
{
    Task AddAsync(DepositAccount account, CancellationToken cancellationToken = default);

    Task<DepositAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<DepositAccount> Items, int TotalCount)> ListAsync(
        ListDepositAccountsCriteria criteria,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IDepositAccountNumberGenerator
{
    Task<string> GenerateNextAsync(DepositProductType productType, CancellationToken cancellationToken = default);
}

public interface IDepositAccountMapper
{
    DepositAccountSummaryDto ToSummary(DepositAccount account);

    DepositAccountDetailDto ToDetail(DepositAccount account);
}
