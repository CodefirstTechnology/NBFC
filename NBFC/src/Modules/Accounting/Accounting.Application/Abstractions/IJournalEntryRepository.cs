using Patsanstha.Modules.Accounting.Domain.Entities;

namespace Patsanstha.Modules.Accounting.Application.Abstractions;

public interface IJournalEntryRepository
{
    Task AddAsync(JournalEntry entry, CancellationToken cancellationToken = default);

    Task<JournalEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<JournalEntry> Items, int TotalCount)> ListAsync(
        ListJournalEntriesCriteria criteria,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IEntryNumberGenerator
{
    Task<string> GenerateNextAsync(CancellationToken cancellationToken = default);
}

public interface IJournalEntryMapper
{
    JournalEntrySummaryDto ToSummary(JournalEntry entry);

    JournalEntryDetailDto ToDetail(JournalEntry entry);
}
