using FluentValidation;
using Patsanstha.Modules.Loans.Application.Abstractions;

namespace Patsanstha.Modules.Loans.Application.LoanApplications.ApproveLoanApplication;

public sealed record ApproveLoanApplicationCommand(
    Guid LoanApplicationId,
    decimal ApprovedAmount) : ICommand<LoanApplicationDetailDto>;

public sealed class ApproveLoanApplicationCommandValidator : AbstractValidator<ApproveLoanApplicationCommand>
{
    public ApproveLoanApplicationCommandValidator()
    {
        RuleFor(x => x.LoanApplicationId).NotEmpty().WithErrorCode("Loans.ApplicationId.Required");
        RuleFor(x => x.ApprovedAmount).GreaterThan(0).WithErrorCode("Loans.ApprovedAmount.Invalid");
    }
}

public sealed class ApproveLoanApplicationCommandHandler(
    ILoanApplicationRepository repository,
    ILoanApplicationMapper mapper) : ICommandHandler<ApproveLoanApplicationCommand, LoanApplicationDetailDto>
{
    public async Task<Result<LoanApplicationDetailDto>> Handle(
        ApproveLoanApplicationCommand request,
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
            var emiAmount = LoanEmiCalculator.CalculateEmi(
                request.ApprovedAmount,
                application.InterestRate,
                application.TenureMonths);

            application.Approve(
                request.ApprovedAmount,
                emiAmount,
                DateOnly.FromDateTime(DateTime.UtcNow));
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<LoanApplicationDetailDto>(
                Error.Validation("Loans.Approve.Invalid", ex.Message));
        }

        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.ToDetail(application));
    }
}
