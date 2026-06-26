using Patsanstha.Modules.Members.Domain.Enums;

namespace Patsanstha.Modules.Members.Application.Abstractions;

public sealed record MemberSummaryDto(
    Guid Id,
    string MemberNumber,
    string FullName,
    string MobileNumber,
    Guid BranchId,
    MemberStatus Status,
    DateOnly JoinedOn);

public sealed record MemberDetailDto(
    Guid Id,
    string MemberNumber,
    string FullName,
    DateOnly DateOfBirth,
    string Gender,
    string MobileNumber,
    string? Email,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string PinCode,
    string AadhaarMasked,
    string PanMasked,
    string? NomineeName,
    string? NomineeRelation,
    Guid BranchId,
    MemberStatus Status,
    DateOnly JoinedOn,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ModifiedAt);

public sealed record PagedMembersResponse(
    IReadOnlyList<MemberSummaryDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record ListMembersCriteria(
    int Page,
    int PageSize,
    string? Search,
    MemberStatus? Status,
    Guid? BranchId);
