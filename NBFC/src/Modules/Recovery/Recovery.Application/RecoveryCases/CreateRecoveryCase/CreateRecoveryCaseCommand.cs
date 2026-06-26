using FluentValidation;
using Patsanstha.Modules.Recovery.Application.Abstractions;
using Patsanstha.Modules.Recovery.Domain.Entities;

namespace Patsanstha.Modules.Recovery.Application.RecoveryCases.CreateRecoveryCase;

public sealed record CreateRecoveryCaseCommand(
    Guid LoanApplicationId,
    string LoanNumber,
    Guid MemberId,
    string MemberNumber,
    string MemberName,
    Guid BranchId,
    decimal OutstandingAmount,
    int DaysPastDue,
    string? Notes) : ICommand<RecoveryCaseDetailDto>;

public sealed class CreateRecoveryCaseCommandValidator : AbstractValidator<CreateRecoveryCaseCommand>
{
    public CreateRecoveryCaseCommandValidator()
    {
        RuleFor(x => x.LoanApplicationId).NotEmpty().WithErrorCode("Recovery.LoanApplicationId.Required");
        RuleFor(x => x.LoanNumber).NotEmpty().MaximumLength(32).WithErrorCode("Recovery.LoanNumber.Required");
        RuleFor(x => x.MemberId).NotEmpty().WithErrorCode("Recovery.MemberId.Required");
        RuleFor(x => x.MemberNumber).NotEmpty().MaximumLength(32).WithErrorCode("Recovery.MemberNumber.Required");
        RuleFor(x => x.MemberName).NotEmpty().MaximumLength(200).WithErrorCode("Recovery.MemberName.Required");
        RuleFor(x => x.BranchId).NotEmpty().WithErrorCode("Recovery.BranchId.Required");
        RuleFor(x => x.OutstandingAmount).GreaterThan(0).WithErrorCode("Recovery.OutstandingAmount.Invalid");
        RuleFor(x => x.DaysPastDue).GreaterThanOrEqualTo(0).WithErrorCode("Recovery.DaysPastDue.Invalid");
        RuleFor(x => x.Notes).MaximumLength(4000).When(x => x.Notes is not null)
            .WithErrorCode("Recovery.Notes.TooLong");
    }
}

public sealed class CreateRecoveryCaseCommandHandler(
    IRecoveryCaseRepository repository,
    ICaseNumberGenerator caseNumberGenerator,
    IRecoveryCaseMapper mapper,
    Patsanstha.BuildingBlocks.Application.Abstractions.Audit.IAuditContextAccessor auditContext)
    : ICommandHandler<CreateRecoveryCaseCommand, RecoveryCaseDetailDto>
{
    public async Task<Result<RecoveryCaseDetailDto>> Handle(
        CreateRecoveryCaseCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = auditContext.Current.TenantId;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<RecoveryCaseDetailDto>(
                Error.Validation("Recovery.Tenant.Required", "Tenant context is required."));
        }

        try
        {
            var caseNumber = await caseNumberGenerator.GenerateNextAsync(cancellationToken);

            var recoveryCase = RecoveryCase.Open(
                tenantId,
                request.LoanApplicationId,
                request.LoanNumber,
                request.MemberId,
                request.MemberNumber,
                request.MemberName,
                request.BranchId,
                caseNumber,
                request.OutstandingAmount,
                request.DaysPastDue,
                request.Notes,
                DateOnly.FromDateTime(DateTime.UtcNow));

            await repository.AddAsync(recoveryCase, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            return Result.Success(mapper.ToDetail(recoveryCase));
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<RecoveryCaseDetailDto>(
                Error.Validation("Recovery.Create.Invalid", ex.Message));
        }
    }
}
