using Patsanstha.Modules.Accounting.Application.Abstractions;

namespace Patsanstha.Modules.Accounting.Application.JournalEntries.GetJournalEntry;

public sealed record GetJournalEntryQuery(Guid JournalEntryId) : IQuery<JournalEntryDetailDto>;

public sealed class GetJournalEntryQueryHandler(
    IJournalEntryRepository repository,
    IJournalEntryMapper mapper) : IQueryHandler<GetJournalEntryQuery, JournalEntryDetailDto>
{
    public async Task<Result<JournalEntryDetailDto>> Handle(
        GetJournalEntryQuery request,
        CancellationToken cancellationToken)
    {
        var entry = await repository.GetByIdAsync(request.JournalEntryId, cancellationToken);

        if (entry is null)
        {
            return Result.Failure<JournalEntryDetailDto>(
                Error.NotFound("Accounting.JournalEntry.NotFound", "Journal entry not found."));
        }

        return Result.Success(mapper.ToDetail(entry));
    }
}
