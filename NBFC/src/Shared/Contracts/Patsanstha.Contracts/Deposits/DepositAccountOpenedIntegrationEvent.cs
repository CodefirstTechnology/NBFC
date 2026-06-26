using Patsanstha.Contracts.Abstractions;

namespace Patsanstha.Contracts.Deposits;

public sealed record DepositAccountOpenedIntegrationEvent : IntegrationEventBase
{
    public required Guid DepositAccountId { get; init; }

    public required Guid MemberId { get; init; }

    public required Guid BranchId { get; init; }

    public required string AccountNumber { get; init; }

    public required string ProductType { get; init; }

    public required decimal PrincipalAmount { get; init; }
}
