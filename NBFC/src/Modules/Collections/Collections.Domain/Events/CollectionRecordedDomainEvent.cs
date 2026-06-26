using Patsanstha.BuildingBlocks.Domain.Abstractions;
using Patsanstha.Modules.Collections.Domain.Enums;

namespace Patsanstha.Modules.Collections.Domain.Events;

public sealed record CollectionRecordedDomainEvent(
    Guid CollectionReceiptId,
    Guid TenantId,
    Guid MemberId,
    Guid LoanApplicationId,
    string LoanNumber,
    string ReceiptNumber,
    decimal Amount,
    PaymentMode PaymentMode,
    DateOnly CollectedOn) : DomainEventBase;
