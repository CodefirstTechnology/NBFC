using Patsanstha.BuildingBlocks.Domain.Abstractions;
using Patsanstha.Modules.Loans.Domain.Enums;

namespace Patsanstha.Modules.Loans.Domain.Events;

public sealed record LoanApplicationSubmittedDomainEvent(
    Guid LoanApplicationId,
    Guid TenantId,
    Guid MemberId,
    Guid BranchId,
    string LoanNumber,
    LoanProductType ProductType,
    decimal RequestedAmount) : DomainEventBase;
