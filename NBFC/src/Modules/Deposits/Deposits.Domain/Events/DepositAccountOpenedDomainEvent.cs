using Patsanstha.BuildingBlocks.Domain.Abstractions;
using Patsanstha.Modules.Deposits.Domain.Enums;

namespace Patsanstha.Modules.Deposits.Domain.Events;

public sealed record DepositAccountOpenedDomainEvent(
    Guid DepositAccountId,
    Guid TenantId,
    Guid MemberId,
    Guid BranchId,
    string AccountNumber,
    DepositProductType ProductType,
    decimal PrincipalAmount) : DomainEventBase;
