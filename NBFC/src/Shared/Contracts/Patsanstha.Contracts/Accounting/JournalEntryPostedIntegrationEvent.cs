using Patsanstha.Contracts.Abstractions;

namespace Patsanstha.Contracts.Accounting;

public sealed record JournalEntryPostedIntegrationEvent : IntegrationEventBase
{
    public required Guid JournalEntryId { get; init; }

    public required string EntryNumber { get; init; }

    public required string DebitAccountCode { get; init; }

    public required string CreditAccountCode { get; init; }

    public required decimal Amount { get; init; }

    public required DateOnly EntryDate { get; init; }

    public string? ReferenceType { get; init; }

    public Guid? ReferenceId { get; init; }
}
