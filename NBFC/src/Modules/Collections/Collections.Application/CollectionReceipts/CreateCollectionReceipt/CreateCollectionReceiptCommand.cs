using FluentValidation;
using Patsanstha.Modules.Collections.Application.Abstractions;
using Patsanstha.Modules.Collections.Domain.Entities;
using Patsanstha.Modules.Collections.Domain.Enums;

namespace Patsanstha.Modules.Collections.Application.CollectionReceipts.CreateCollectionReceipt;

public sealed record CreateCollectionReceiptCommand(
    Guid MemberId,
    string MemberNumber,
    string MemberName,
    Guid LoanApplicationId,
    string LoanNumber,
    Guid BranchId,
    decimal Amount,
    PaymentMode PaymentMode,
    string? ReferenceNumber,
    DateOnly CollectedOn) : ICommand<CollectionReceiptDetailDto>;

public sealed class CreateCollectionReceiptCommandValidator : AbstractValidator<CreateCollectionReceiptCommand>
{
    public CreateCollectionReceiptCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty().WithErrorCode("Collections.MemberId.Required");
        RuleFor(x => x.MemberNumber).NotEmpty().MaximumLength(32).WithErrorCode("Collections.MemberNumber.Required");
        RuleFor(x => x.MemberName).NotEmpty().MaximumLength(200).WithErrorCode("Collections.MemberName.Required");
        RuleFor(x => x.LoanApplicationId).NotEmpty().WithErrorCode("Collections.LoanApplicationId.Required");
        RuleFor(x => x.LoanNumber).NotEmpty().MaximumLength(32).WithErrorCode("Collections.LoanNumber.Required");
        RuleFor(x => x.BranchId).NotEmpty().WithErrorCode("Collections.BranchId.Required");
        RuleFor(x => x.Amount).GreaterThan(0).WithErrorCode("Collections.Amount.Invalid");
        RuleFor(x => x.ReferenceNumber).MaximumLength(64).When(x => x.ReferenceNumber is not null)
            .WithErrorCode("Collections.ReferenceNumber.Invalid");
    }
}

public sealed class CreateCollectionReceiptCommandHandler(
    ICollectionReceiptRepository repository,
    IReceiptNumberGenerator receiptNumberGenerator,
    ICollectionReceiptMapper mapper,
    Patsanstha.BuildingBlocks.Application.Abstractions.Audit.IAuditContextAccessor auditContext)
    : ICommandHandler<CreateCollectionReceiptCommand, CollectionReceiptDetailDto>
{
    public async Task<Result<CollectionReceiptDetailDto>> Handle(
        CreateCollectionReceiptCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = auditContext.Current.TenantId;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<CollectionReceiptDetailDto>(
                Error.Validation("Collections.Tenant.Required", "Tenant context is required."));
        }

        var receiptNumber = await receiptNumberGenerator.GenerateNextAsync(cancellationToken);

        CollectionReceipt receipt;
        try
        {
            receipt = CollectionReceipt.Record(
                tenantId,
                request.MemberId,
                request.MemberNumber,
                request.MemberName,
                request.LoanApplicationId,
                request.LoanNumber,
                request.BranchId,
                receiptNumber,
                request.Amount,
                request.PaymentMode,
                request.ReferenceNumber,
                request.CollectedOn);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<CollectionReceiptDetailDto>(
                Error.Validation("Collections.Create.Invalid", ex.Message));
        }

        await repository.AddAsync(receipt, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.ToDetail(receipt));
    }
}
