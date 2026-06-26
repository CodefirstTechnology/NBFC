using Patsanstha.Modules.Recovery.Domain.Enums;

namespace Patsanstha.Modules.Recovery.Application.Abstractions;

public sealed record RecoveryCaseSummaryDto(
    Guid Id,
    string CaseNumber,
    string LoanNumber,
    string MemberName,
    string MemberNumber,
    decimal OutstandingAmount,
    int DaysPastDue,
    RecoveryCaseStatus Status,
    DateOnly OpenedOn,
    Guid? AssignedToUserId);

public sealed record RecoveryCaseDetailDto(
    Guid Id,
    Guid LoanApplicationId,
    string LoanNumber,
    Guid MemberId,
    string MemberNumber,
    string MemberName,
    Guid BranchId,
    string CaseNumber,
    decimal OutstandingAmount,
    int DaysPastDue,
    RecoveryCaseStatus Status,
    string? Notes,
    Guid? AssignedToUserId,
    DateOnly OpenedOn,
    DateOnly? ResolvedOn,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ModifiedAt);

public sealed record PagedRecoveryCasesResponse(
    IReadOnlyList<RecoveryCaseSummaryDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record ListRecoveryCasesCriteria(
    int Page,
    int PageSize,
    string? Search,
    RecoveryCaseStatus? Status,
    Guid? BranchId,
    Guid? MemberId,
    Guid? AssignedToUserId);
