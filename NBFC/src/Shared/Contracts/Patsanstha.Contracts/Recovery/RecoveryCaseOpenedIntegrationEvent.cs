using Patsanstha.Contracts.Abstractions;

namespace Patsanstha.Contracts.Recovery;

public sealed record RecoveryCaseOpenedIntegrationEvent : IntegrationEventBase
{
    public required Guid RecoveryCaseId { get; init; }

    public required Guid LoanApplicationId { get; init; }

    public required Guid MemberId { get; init; }

    public required Guid BranchId { get; init; }

    public required string CaseNumber { get; init; }

    public required string LoanNumber { get; init; }

    public required decimal OutstandingAmount { get; init; }

    public required int DaysPastDue { get; init; }
}
