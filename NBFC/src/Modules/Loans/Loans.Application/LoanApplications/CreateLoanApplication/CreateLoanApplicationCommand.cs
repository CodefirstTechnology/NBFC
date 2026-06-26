using FluentValidation;
using Patsanstha.Modules.Loans.Application.Abstractions;
using Patsanstha.Modules.Loans.Domain.Entities;
using Patsanstha.Modules.Loans.Domain.Enums;

namespace Patsanstha.Modules.Loans.Application.LoanApplications.CreateLoanApplication;

public sealed record CreateLoanApplicationCommand(
    Guid MemberId,
    string MemberNumber,
    string MemberName,
    Guid BranchId,
    LoanProductType ProductType,
    decimal RequestedAmount,
    int TenureMonths,
    string Purpose) : ICommand<LoanApplicationDetailDto>;

public sealed class CreateLoanApplicationCommandValidator : AbstractValidator<CreateLoanApplicationCommand>
{
    public CreateLoanApplicationCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty().WithErrorCode("Loans.MemberId.Required");
        RuleFor(x => x.MemberNumber).NotEmpty().MaximumLength(32).WithErrorCode("Loans.MemberNumber.Required");
        RuleFor(x => x.MemberName).NotEmpty().MaximumLength(200).WithErrorCode("Loans.MemberName.Required");
        RuleFor(x => x.BranchId).NotEmpty().WithErrorCode("Loans.BranchId.Required");
        RuleFor(x => x.RequestedAmount).GreaterThan(0).WithErrorCode("Loans.RequestedAmount.Invalid");
        RuleFor(x => x.TenureMonths).InclusiveBetween(1, 360).WithErrorCode("Loans.TenureMonths.Invalid");
        RuleFor(x => x.Purpose).NotEmpty().MaximumLength(500).WithErrorCode("Loans.Purpose.Required");
    }
}

public sealed class CreateLoanApplicationCommandHandler(
    ILoanApplicationRepository repository,
    ILoanNumberGenerator loanNumberGenerator,
    ILoanApplicationMapper mapper,
    Patsanstha.BuildingBlocks.Application.Abstractions.Audit.IAuditContextAccessor auditContext)
    : ICommandHandler<CreateLoanApplicationCommand, LoanApplicationDetailDto>
{
    public async Task<Result<LoanApplicationDetailDto>> Handle(
        CreateLoanApplicationCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = auditContext.Current.TenantId;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<LoanApplicationDetailDto>(
                Error.Validation("Loans.Tenant.Required", "Tenant context is required."));
        }

        var loanNumber = await loanNumberGenerator.GenerateNextAsync(cancellationToken);
        var interestRate = LoanProductCatalog.GetDefaultInterestRate(request.ProductType);

        var application = LoanApplication.Submit(
            tenantId,
            request.MemberId,
            request.MemberNumber,
            request.MemberName,
            request.BranchId,
            loanNumber,
            request.ProductType,
            request.RequestedAmount,
            interestRate,
            request.TenureMonths,
            request.Purpose,
            DateOnly.FromDateTime(DateTime.UtcNow));

        await repository.AddAsync(application, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.ToDetail(application));
    }
}
