using FluentValidation;
using Patsanstha.Modules.Accounting.Application.Abstractions;
using Patsanstha.Modules.Accounting.Domain.Entities;

namespace Patsanstha.Modules.Accounting.Application.JournalEntries.CreateJournalEntry;

public sealed record CreateJournalEntryCommand(
    string Description,
    DateOnly EntryDate,
    string DebitAccountCode,
    string CreditAccountCode,
    decimal Amount,
    string? ReferenceType = null,
    Guid? ReferenceId = null) : ICommand<JournalEntryDetailDto>;

public sealed class CreateJournalEntryCommandValidator : AbstractValidator<CreateJournalEntryCommand>
{
    public CreateJournalEntryCommandValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500).WithErrorCode("Accounting.Description.Required");
        RuleFor(x => x.DebitAccountCode).NotEmpty().MaximumLength(32).WithErrorCode("Accounting.DebitAccountCode.Required");
        RuleFor(x => x.CreditAccountCode).NotEmpty().MaximumLength(32).WithErrorCode("Accounting.CreditAccountCode.Required");
        RuleFor(x => x.Amount).GreaterThan(0).WithErrorCode("Accounting.Amount.Invalid");
        RuleFor(x => x)
            .Must(x => !string.Equals(x.DebitAccountCode.Trim(), x.CreditAccountCode.Trim(), StringComparison.OrdinalIgnoreCase))
            .WithErrorCode("Accounting.AccountCodes.MustDiffer");
        RuleFor(x => x.ReferenceType).MaximumLength(64).When(x => x.ReferenceType is not null);
    }
}

public sealed class CreateJournalEntryCommandHandler(
    IJournalEntryRepository repository,
    IEntryNumberGenerator entryNumberGenerator,
    IJournalEntryMapper mapper,
    Patsanstha.BuildingBlocks.Application.Abstractions.Audit.IAuditContextAccessor auditContext)
    : ICommandHandler<CreateJournalEntryCommand, JournalEntryDetailDto>
{
    public async Task<Result<JournalEntryDetailDto>> Handle(
        CreateJournalEntryCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = auditContext.Current.TenantId;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<JournalEntryDetailDto>(
                Error.Validation("Accounting.Tenant.Required", "Tenant context is required."));
        }

        try
        {
            var entryNumber = await entryNumberGenerator.GenerateNextAsync(cancellationToken);

            var entry = JournalEntry.Create(
                tenantId,
                entryNumber,
                request.Description,
                request.EntryDate,
                request.DebitAccountCode,
                request.CreditAccountCode,
                request.Amount,
                request.ReferenceType,
                request.ReferenceId);

            await repository.AddAsync(entry, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            return Result.Success(mapper.ToDetail(entry));
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<JournalEntryDetailDto>(
                Error.Validation("Accounting.Create.Invalid", ex.Message));
        }
    }
}
