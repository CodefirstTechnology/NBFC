using FluentValidation;
using Patsanstha.Modules.Accounting.Application.Abstractions;

namespace Patsanstha.Modules.Accounting.Application.JournalEntries.PostJournalEntry;

public sealed record PostJournalEntryCommand(Guid JournalEntryId) : ICommand<JournalEntryDetailDto>;

public sealed class PostJournalEntryCommandValidator : AbstractValidator<PostJournalEntryCommand>
{
    public PostJournalEntryCommandValidator()
    {
        RuleFor(x => x.JournalEntryId).NotEmpty().WithErrorCode("Accounting.JournalEntryId.Required");
    }
}

public sealed class PostJournalEntryCommandHandler(
    IJournalEntryRepository repository,
    IJournalEntryMapper mapper) : ICommandHandler<PostJournalEntryCommand, JournalEntryDetailDto>
{
    public async Task<Result<JournalEntryDetailDto>> Handle(
        PostJournalEntryCommand request,
        CancellationToken cancellationToken)
    {
        var entry = await repository.GetByIdAsync(request.JournalEntryId, cancellationToken);

        if (entry is null)
        {
            return Result.Failure<JournalEntryDetailDto>(
                Error.NotFound("Accounting.JournalEntry.NotFound", "Journal entry not found."));
        }

        try
        {
            entry.Post();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<JournalEntryDetailDto>(
                Error.Validation("Accounting.Post.Invalid", ex.Message));
        }

        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.ToDetail(entry));
    }
}
