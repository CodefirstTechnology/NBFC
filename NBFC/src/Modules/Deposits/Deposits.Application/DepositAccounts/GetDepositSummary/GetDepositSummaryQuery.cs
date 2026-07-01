using Patsanstha.Modules.Deposits.Application.Abstractions;

namespace Patsanstha.Modules.Deposits.Application.DepositAccounts.GetDepositSummary;

public sealed record GetDepositSummaryQuery(Guid? BranchId = null) : IQuery<DepositSummaryDto>;

public sealed class GetDepositSummaryQueryHandler(
    IDepositAccountRepository repository) : IQueryHandler<GetDepositSummaryQuery, DepositSummaryDto>
{
    public async Task<Result<DepositSummaryDto>> Handle(
        GetDepositSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var summary = await repository.GetSummaryAsync(request.BranchId, cancellationToken);
        return Result.Success(summary);
    }
}
