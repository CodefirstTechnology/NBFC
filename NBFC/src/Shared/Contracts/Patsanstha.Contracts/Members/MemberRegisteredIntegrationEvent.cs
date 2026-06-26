using Patsanstha.Contracts.Abstractions;

namespace Patsanstha.Contracts.Members;

public sealed record MemberRegisteredIntegrationEvent : IntegrationEventBase
{
    public required Guid MemberId { get; init; }

    public required Guid BranchId { get; init; }

    public required string MemberNumber { get; init; }

    public required string FullName { get; init; }
}
