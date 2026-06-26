using Patsanstha.Modules.Collections.Domain.Enums;

namespace Patsanstha.Modules.Collections.Application.Abstractions;

public sealed record CollectionReceiptSummaryDto(
    Guid Id,
    string ReceiptNumber,
    string LoanNumber,
    string MemberName,
    string MemberNumber,
    decimal Amount,
    PaymentMode PaymentMode,
    CollectionReceiptStatus Status,
    DateOnly CollectedOn);

public sealed record CollectionReceiptDetailDto(
    Guid Id,
    Guid MemberId,
    string MemberNumber,
    string MemberName,
    Guid LoanApplicationId,
    string LoanNumber,
    Guid BranchId,
    string ReceiptNumber,
    decimal Amount,
    PaymentMode PaymentMode,
    string? ReferenceNumber,
    DateOnly CollectedOn,
    CollectionReceiptStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ModifiedAt);

public sealed record PagedCollectionReceiptsResponse(
    IReadOnlyList<CollectionReceiptSummaryDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record ListCollectionReceiptsCriteria(
    int Page,
    int PageSize,
    string? Search,
    Guid? BranchId,
    Guid? MemberId,
    string? LoanNumber);
