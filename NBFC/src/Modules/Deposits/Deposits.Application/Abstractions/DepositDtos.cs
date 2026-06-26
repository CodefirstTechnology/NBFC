using Patsanstha.Modules.Deposits.Domain.Enums;

namespace Patsanstha.Modules.Deposits.Application.Abstractions;

public sealed record DepositAccountSummaryDto(
    Guid Id,
    string AccountNumber,
    string MemberName,
    string MemberNumber,
    DepositProductType ProductType,
    decimal CurrentBalance,
    decimal InterestRate,
    DateOnly? MaturityDate,
    DepositAccountStatus Status,
    DateOnly OpenedOn);

public sealed record DepositAccountDetailDto(
    Guid Id,
    Guid MemberId,
    string MemberNumber,
    string MemberName,
    Guid BranchId,
    string AccountNumber,
    DepositProductType ProductType,
    decimal PrincipalAmount,
    decimal CurrentBalance,
    decimal InterestRate,
    int? TenureMonths,
    InterestPayoutMode InterestPayoutMode,
    bool AutoRenewal,
    DateOnly OpenedOn,
    DateOnly? MaturityDate,
    DepositAccountStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ModifiedAt);

public sealed record PagedDepositAccountsResponse(
    IReadOnlyList<DepositAccountSummaryDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record ListDepositAccountsCriteria(
    int Page,
    int PageSize,
    string? Search,
    DepositProductType? ProductType,
    DepositAccountStatus? Status,
    Guid? BranchId,
    Guid? MemberId);
