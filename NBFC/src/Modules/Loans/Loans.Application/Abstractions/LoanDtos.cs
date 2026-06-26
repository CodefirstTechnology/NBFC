using Patsanstha.Modules.Loans.Domain.Enums;

namespace Patsanstha.Modules.Loans.Application.Abstractions;

public sealed record LoanApplicationSummaryDto(
    Guid Id,
    string LoanNumber,
    string MemberName,
    string MemberNumber,
    LoanProductType ProductType,
    decimal RequestedAmount,
    decimal? ApprovedAmount,
    decimal? EmiAmount,
    LoanApplicationStatus Status,
    DateOnly AppliedOn);

public sealed record LoanApplicationDetailDto(
    Guid Id,
    Guid MemberId,
    string MemberNumber,
    string MemberName,
    Guid BranchId,
    string LoanNumber,
    LoanProductType ProductType,
    decimal RequestedAmount,
    decimal? ApprovedAmount,
    decimal InterestRate,
    int TenureMonths,
    decimal? EmiAmount,
    decimal? OutstandingPrincipal,
    string Purpose,
    LoanApplicationStatus Status,
    string? RejectionReason,
    DateOnly AppliedOn,
    DateOnly? ApprovedOn,
    DateOnly? DisbursedOn,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ModifiedAt);

public sealed record PagedLoanApplicationsResponse(
    IReadOnlyList<LoanApplicationSummaryDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record ListLoanApplicationsCriteria(
    int Page,
    int PageSize,
    string? Search,
    LoanProductType? ProductType,
    LoanApplicationStatus? Status,
    Guid? BranchId,
    Guid? MemberId);
