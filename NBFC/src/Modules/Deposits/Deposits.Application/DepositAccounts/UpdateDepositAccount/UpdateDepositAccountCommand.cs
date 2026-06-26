using FluentValidation;
using Patsanstha.Modules.Deposits.Application.Abstractions;
using Patsanstha.Modules.Deposits.Domain.Enums;

namespace Patsanstha.Modules.Deposits.Application.DepositAccounts.UpdateDepositAccount;

public sealed record UpdateDepositAccountCommand(
    Guid DepositAccountId,
    DepositAccountStatus? Status,
    bool? AutoRenewal) : ICommand<DepositAccountDetailDto>;

public sealed class UpdateDepositAccountCommandValidator : AbstractValidator<UpdateDepositAccountCommand>
{
    public UpdateDepositAccountCommandValidator()
    {
        RuleFor(x => x.DepositAccountId).NotEmpty().WithErrorCode("Deposits.AccountId.Required");
        RuleFor(x => x)
            .Must(x => x.Status.HasValue || x.AutoRenewal.HasValue)
            .WithErrorCode("Deposits.Update.NoChanges");
    }
}

public sealed class UpdateDepositAccountCommandHandler(
    IDepositAccountRepository repository,
    IDepositAccountMapper mapper) : ICommandHandler<UpdateDepositAccountCommand, DepositAccountDetailDto>
{
    public async Task<Result<DepositAccountDetailDto>> Handle(
        UpdateDepositAccountCommand request,
        CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.DepositAccountId, cancellationToken);

        if (account is null)
        {
            return Result.Failure<DepositAccountDetailDto>(
                Error.NotFound("Deposits.Account.NotFound", "Deposit account not found."));
        }

        try
        {
            if (request.Status.HasValue)
            {
                account.ChangeStatus(request.Status.Value);
            }

            if (request.AutoRenewal.HasValue)
            {
                account.SetAutoRenewal(request.AutoRenewal.Value);
            }
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<DepositAccountDetailDto>(
                Error.Validation("Deposits.Update.Invalid", ex.Message));
        }

        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.ToDetail(account));
    }
}
