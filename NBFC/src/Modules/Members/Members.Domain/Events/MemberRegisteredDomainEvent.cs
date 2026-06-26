using Patsanstha.BuildingBlocks.Domain.Abstractions;

namespace Patsanstha.Modules.Members.Domain.Events;

public sealed record MemberRegisteredDomainEvent(
    Guid MemberId,
    Guid TenantId,
    Guid BranchId,
    string MemberNumber,
    string FullName) : DomainEventBase;
