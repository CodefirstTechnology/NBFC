using Patsanstha.Modules.Deposits.Application.Abstractions;

namespace Patsanstha.Modules.Deposits.Application.DepositAccounts.GetDepositAccount;

public sealed record GetDepositAccountQuery(Guid DepositAccountId) : IQuery<DepositAccountDetailDto>;

public sealed class GetDepositAccountQueryHandler(
    IDepositAccountRepository repository,
    IDepositAccountMapper mapper) : IQueryHandler<GetDepositAccountQuery, DepositAccountDetailDto>
{
    public async Task<Result<DepositAccountDetailDto>> Handle(
        GetDepositAccountQuery request,
        CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.DepositAccountId, cancellationToken);

        if (account is null)
        {
            return Result.Failure<DepositAccountDetailDto>(
                Error.NotFound("Deposits.Account.NotFound", "Deposit account not found."));
        }

        return Result.Success(mapper.ToDetail(account));
    }
}
