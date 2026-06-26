using FluentValidation;
using Patsanstha.Modules.Loans.Application.Abstractions;

namespace Patsanstha.Modules.Loans.Application.LoanApplications.DisburseLoanApplication;

public sealed record DisburseLoanApplicationCommand(Guid LoanApplicationId) : ICommand<LoanApplicationDetailDto>;

public sealed class DisburseLoanApplicationCommandValidator : AbstractValidator<DisburseLoanApplicationCommand>
{
    public DisburseLoanApplicationCommandValidator()
    {
        RuleFor(x => x.LoanApplicationId).NotEmpty().WithErrorCode("Loans.ApplicationId.Required");
    }
}

public sealed class DisburseLoanApplicationCommandHandler(
    ILoanApplicationRepository repository,
    ILoanApplicationMapper mapper) : ICommandHandler<DisburseLoanApplicationCommand, LoanApplicationDetailDto>
{
    public async Task<Result<LoanApplicationDetailDto>> Handle(
        DisburseLoanApplicationCommand request,
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
            application.Disburse(DateOnly.FromDateTime(DateTime.UtcNow));
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<LoanApplicationDetailDto>(
                Error.Validation("Loans.Disburse.Invalid", ex.Message));
        }

        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.ToDetail(application));
    }
}
