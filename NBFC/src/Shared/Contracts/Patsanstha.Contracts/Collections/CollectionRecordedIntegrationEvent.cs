using Patsanstha.Contracts.Abstractions;

namespace Patsanstha.Contracts.Collections;

public sealed record CollectionRecordedIntegrationEvent : IntegrationEventBase
{
    public required Guid CollectionReceiptId { get; init; }

    public required Guid MemberId { get; init; }

    public required Guid LoanApplicationId { get; init; }

    public required string LoanNumber { get; init; }

    public required string ReceiptNumber { get; init; }

    public required decimal Amount { get; init; }

    public required string PaymentMode { get; init; }

    public required DateOnly CollectedOn { get; init; }
}
