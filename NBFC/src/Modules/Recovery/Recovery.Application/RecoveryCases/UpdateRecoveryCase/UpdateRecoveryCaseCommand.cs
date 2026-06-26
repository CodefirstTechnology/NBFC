using FluentValidation;
using Patsanstha.Modules.Recovery.Application.Abstractions;
using Patsanstha.Modules.Recovery.Domain.Enums;

namespace Patsanstha.Modules.Recovery.Application.RecoveryCases.UpdateRecoveryCase;

public sealed record UpdateRecoveryCaseCommand(
    Guid RecoveryCaseId,
    RecoveryCaseStatus? Status,
    Guid? AssignedToUserId,
    string? Notes) : ICommand<RecoveryCaseDetailDto>;

public sealed class UpdateRecoveryCaseCommandValidator : AbstractValidator<UpdateRecoveryCaseCommand>
{
    public UpdateRecoveryCaseCommandValidator()
    {
        RuleFor(x => x.RecoveryCaseId).NotEmpty().WithErrorCode("Recovery.CaseId.Required");
        RuleFor(x => x)
            .Must(x => x.Status.HasValue || x.AssignedToUserId.HasValue || !string.IsNullOrWhiteSpace(x.Notes))
            .WithErrorCode("Recovery.Update.NoChanges");
        RuleFor(x => x.Notes).MaximumLength(4000).When(x => !string.IsNullOrWhiteSpace(x.Notes))
            .WithErrorCode("Recovery.Notes.TooLong");
    }
}

public sealed class UpdateRecoveryCaseCommandHandler(
    IRecoveryCaseRepository repository,
    IRecoveryCaseMapper mapper) : ICommandHandler<UpdateRecoveryCaseCommand, RecoveryCaseDetailDto>
{
    public async Task<Result<RecoveryCaseDetailDto>> Handle(
        UpdateRecoveryCaseCommand request,
        CancellationToken cancellationToken)
    {
        var recoveryCase = await repository.GetByIdAsync(request.RecoveryCaseId, cancellationToken);

        if (recoveryCase is null)
        {
            return Result.Failure<RecoveryCaseDetailDto>(
                Error.NotFound("Recovery.Case.NotFound", "Recovery case not found."));
        }

        try
        {
            if (request.Status.HasValue)
            {
                recoveryCase.UpdateStatus(request.Status.Value, DateOnly.FromDateTime(DateTime.UtcNow));
            }

            if (request.AssignedToUserId.HasValue)
            {
                recoveryCase.Assign(request.AssignedToUserId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                recoveryCase.AddNotes(request.Notes);
            }
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<RecoveryCaseDetailDto>(
                Error.Validation("Recovery.Update.Invalid", ex.Message));
        }

        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.ToDetail(recoveryCase));
    }
}
