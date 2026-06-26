using Patsanstha.BuildingBlocks.Domain.Abstractions;

namespace Patsanstha.Modules.Accounting.Domain.Events;

public sealed record JournalEntryPostedDomainEvent(
    Guid JournalEntryId,
    Guid TenantId,
    string EntryNumber,
    string DebitAccountCode,
    string CreditAccountCode,
    decimal Amount,
    DateOnly EntryDate,
    string? ReferenceType,
    Guid? ReferenceId) : DomainEventBase;
