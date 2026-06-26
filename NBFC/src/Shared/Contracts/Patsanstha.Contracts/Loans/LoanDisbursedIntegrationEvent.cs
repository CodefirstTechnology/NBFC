using Patsanstha.Contracts.Abstractions;

namespace Patsanstha.Contracts.Loans;

public sealed record LoanDisbursedIntegrationEvent : IntegrationEventBase
{
    public required Guid LoanApplicationId { get; init; }

    public required Guid MemberId { get; init; }

    public required string LoanNumber { get; init; }

    public required decimal DisbursedAmount { get; init; }

    public required decimal EmiAmount { get; init; }
}
