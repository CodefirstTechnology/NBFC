using Patsanstha.Modules.Accounting.Domain.Entities;
using Patsanstha.Modules.Accounting.Domain.Enums;

namespace Patsanstha.Modules.Accounting.Application.Abstractions;

public sealed record JournalEntrySummaryDto(
    Guid Id,
    string EntryNumber,
    string Description,
    DateOnly EntryDate,
    string DebitAccountCode,
    string CreditAccountCode,
    decimal Amount,
    JournalEntryStatus Status);

public sealed record JournalEntryDetailDto(
    Guid Id,
    string EntryNumber,
    string Description,
    DateOnly EntryDate,
    string DebitAccountCode,
    string CreditAccountCode,
    decimal Amount,
    JournalEntryStatus Status,
    string? ReferenceType,
    Guid? ReferenceId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ModifiedAt);

public sealed record PagedJournalEntriesResponse(
    IReadOnlyList<JournalEntrySummaryDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record ListJournalEntriesCriteria(
    int Page,
    int PageSize,
    string? Search,
    JournalEntryStatus? Status,
    DateOnly? FromDate,
    DateOnly? ToDate);
