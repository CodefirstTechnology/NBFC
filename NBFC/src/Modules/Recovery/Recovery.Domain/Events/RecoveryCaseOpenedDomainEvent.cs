using Patsanstha.BuildingBlocks.Domain.Abstractions;

namespace Patsanstha.Modules.Recovery.Domain.Events;

public sealed record RecoveryCaseOpenedDomainEvent(
    Guid RecoveryCaseId,
    Guid TenantId,
    Guid LoanApplicationId,
    Guid MemberId,
    Guid BranchId,
    string CaseNumber,
    string LoanNumber,
    decimal OutstandingAmount,
    int DaysPastDue) : DomainEventBase;
