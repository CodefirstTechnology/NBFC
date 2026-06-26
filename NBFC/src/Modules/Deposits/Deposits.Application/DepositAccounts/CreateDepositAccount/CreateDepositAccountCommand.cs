using FluentValidation;
using Patsanstha.Modules.Deposits.Application.Abstractions;
using Patsanstha.Modules.Deposits.Domain.Entities;
using Patsanstha.Modules.Deposits.Domain.Enums;

namespace Patsanstha.Modules.Deposits.Application.DepositAccounts.CreateDepositAccount;

public sealed record CreateDepositAccountCommand(
    Guid MemberId,
    string MemberNumber,
    string MemberName,
    Guid BranchId,
    DepositProductType ProductType,
    decimal PrincipalAmount,
    int? TenureMonths,
    InterestPayoutMode InterestPayoutMode,
    bool AutoRenewal) : ICommand<DepositAccountDetailDto>;

public sealed class CreateDepositAccountCommandValidator : AbstractValidator<CreateDepositAccountCommand>
{
    public CreateDepositAccountCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty().WithErrorCode("Deposits.MemberId.Required");
        RuleFor(x => x.MemberNumber).NotEmpty().MaximumLength(32).WithErrorCode("Deposits.MemberNumber.Required");
        RuleFor(x => x.MemberName).NotEmpty().MaximumLength(200).WithErrorCode("Deposits.MemberName.Required");
        RuleFor(x => x.BranchId).NotEmpty().WithErrorCode("Deposits.BranchId.Required");
        RuleFor(x => x.PrincipalAmount).GreaterThan(0).WithErrorCode("Deposits.PrincipalAmount.Invalid");

        RuleFor(x => x.TenureMonths)
            .NotNull()
            .GreaterThan(0)
            .When(x => DepositProductCatalog.RequiresTenure(x.ProductType))
            .WithErrorCode("Deposits.TenureMonths.Required");

        RuleFor(x => x.TenureMonths)
            .Null()
            .When(x => x.ProductType == DepositProductType.Savings)
            .WithErrorCode("Deposits.TenureMonths.NotAllowed");
    }
}

public sealed class CreateDepositAccountCommandHandler(
    IDepositAccountRepository repository,
    IDepositAccountNumberGenerator accountNumberGenerator,
    IDepositAccountMapper mapper,
    Patsanstha.BuildingBlocks.Application.Abstractions.Audit.IAuditContextAccessor auditContext)
    : ICommandHandler<CreateDepositAccountCommand, DepositAccountDetailDto>
{
    public async Task<Result<DepositAccountDetailDto>> Handle(
        CreateDepositAccountCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = auditContext.Current.TenantId;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<DepositAccountDetailDto>(
                Error.Validation("Deposits.Tenant.Required", "Tenant context is required."));
        }

        var accountNumber = await accountNumberGenerator.GenerateNextAsync(request.ProductType, cancellationToken);
        var interestRate = DepositProductCatalog.GetDefaultInterestRate(request.ProductType);

        var account = DepositAccount.Open(
            tenantId,
            request.MemberId,
            request.MemberNumber,
            request.MemberName,
            request.BranchId,
            accountNumber,
            request.ProductType,
            request.PrincipalAmount,
            interestRate,
            request.TenureMonths,
            request.InterestPayoutMode,
            request.AutoRenewal,
            DateOnly.FromDateTime(DateTime.UtcNow));

        await repository.AddAsync(account, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.ToDetail(account));
    }
}
