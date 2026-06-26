using FluentValidation;
using Patsanstha.Modules.Loans.Application.Abstractions;

namespace Patsanstha.Modules.Loans.Application.LoanApplications.RejectLoanApplication;

public sealed record RejectLoanApplicationCommand(
    Guid LoanApplicationId,
    string Reason) : ICommand<LoanApplicationDetailDto>;

public sealed class RejectLoanApplicationCommandValidator : AbstractValidator<RejectLoanApplicationCommand>
{
    public RejectLoanApplicationCommandValidator()
    {
        RuleFor(x => x.LoanApplicationId).NotEmpty().WithErrorCode("Loans.ApplicationId.Required");
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500).WithErrorCode("Loans.RejectionReason.Required");
    }
}

public sealed class RejectLoanApplicationCommandHandler(
    ILoanApplicationRepository repository,
    ILoanApplicationMapper mapper) : ICommandHandler<RejectLoanApplicationCommand, LoanApplicationDetailDto>
{
    public async Task<Result<LoanApplicationDetailDto>> Handle(
        RejectLoanApplicationCommand request,
        CancellationToken cancellationToken)
    {
        var application = await repository.GetByIdAsync(request.LoanApplicationId, cancellationToken);

        if (application is null)
        {
            return Result.Failure<LoanApplicationDetailDto>(
                Error.NotFound("Loans.Application.NotFound", "Loan application not found."));
        }

        try
        {
            application.Reject(request.Reason, DateOnly.FromDateTime(DateTime.UtcNow));
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<LoanApplicationDetailDto>(
                Error.Validation("Loans.Reject.Invalid", ex.Message));
        }

        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.ToDetail(application));
    }
}
