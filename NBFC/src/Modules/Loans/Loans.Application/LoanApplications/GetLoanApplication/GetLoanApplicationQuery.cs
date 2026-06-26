using Patsanstha.Modules.Loans.Application.Abstractions;

namespace Patsanstha.Modules.Loans.Application.LoanApplications.GetLoanApplication;

public sealed record GetLoanApplicationQuery(Guid LoanApplicationId) : IQuery<LoanApplicationDetailDto>;

public sealed class GetLoanApplicationQueryHandler(
    ILoanApplicationRepository repository,
    ILoanApplicationMapper mapper) : IQueryHandler<GetLoanApplicationQuery, LoanApplicationDetailDto>
{
    public async Task<Result<LoanApplicationDetailDto>> Handle(
        GetLoanApplicationQuery request,
        CancellationToken cancellationToken)
    {
        var application = await repository.GetByIdAsync(request.LoanApplicationId, cancellationToken);

        if (application is null)
        {
            return Result.Failure<LoanApplicationDetailDto>(
                Error.NotFound("Loans.Application.NotFound", "Loan application not found."));
        }

        return Result.Success(mapper.ToDetail(application));
    }
}
