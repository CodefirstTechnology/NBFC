using FluentValidation;
using Patsanstha.Modules.Accounting.Application.Abstractions;
using Patsanstha.Modules.Accounting.Domain.Entities;
using Patsanstha.Modules.Accounting.Domain.Enums;

namespace Patsanstha.Modules.Accounting.Application.JournalEntries.ListJournalEntries;

public sealed record ListJournalEntriesQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    JournalEntryStatus? Status = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null) : IQuery<PagedJournalEntriesResponse>;

public sealed class ListJournalEntriesQueryValidator : AbstractValidator<ListJournalEntriesQuery>
{
    public ListJournalEntriesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0).WithErrorCode("Accounting.Page.Invalid");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithErrorCode("Accounting.PageSize.Invalid");
    }
}

public sealed class ListJournalEntriesQueryHandler(
    IJournalEntryRepository repository,
    IJournalEntryMapper mapper) : IQueryHandler<ListJournalEntriesQuery, PagedJournalEntriesResponse>
{
    public async Task<Result<PagedJournalEntriesResponse>> Handle(
        ListJournalEntriesQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.ListAsync(
            new ListJournalEntriesCriteria(
                request.Page,
                request.PageSize,
                request.Search,
                request.Status,
                request.FromDate,
                request.ToDate),
            cancellationToken);

        var summaries = items.Select(mapper.ToSummary).ToList();

        return Result.Success(new PagedJournalEntriesResponse(
            summaries,
            request.Page,
            request.PageSize,
            totalCount));
    }
}
