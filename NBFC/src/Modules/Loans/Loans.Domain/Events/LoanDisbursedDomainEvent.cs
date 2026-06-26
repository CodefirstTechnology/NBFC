using Patsanstha.BuildingBlocks.Domain.Abstractions;

namespace Patsanstha.Modules.Loans.Domain.Events;

public sealed record LoanDisbursedDomainEvent(
    Guid LoanApplicationId,
    Guid TenantId,
    Guid MemberId,
    string LoanNumber,
    decimal DisbursedAmount,
    decimal EmiAmount) : DomainEventBase;
