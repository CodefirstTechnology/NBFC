using FluentValidation;
using Patsanstha.Modules.Collections.Application.Abstractions;

namespace Patsanstha.Modules.Collections.Application.CollectionReceipts.ReverseCollectionReceipt;

public sealed record ReverseCollectionReceiptCommand(Guid CollectionReceiptId) : ICommand<CollectionReceiptDetailDto>;

public sealed class ReverseCollectionReceiptCommandValidator : AbstractValidator<ReverseCollectionReceiptCommand>
{
    public ReverseCollectionReceiptCommandValidator()
    {
        RuleFor(x => x.CollectionReceiptId).NotEmpty().WithErrorCode("Collections.ReceiptId.Required");
    }
}

public sealed class ReverseCollectionReceiptCommandHandler(
    ICollectionReceiptRepository repository,
    ICollectionReceiptMapper mapper) : ICommandHandler<ReverseCollectionReceiptCommand, CollectionReceiptDetailDto>
{
    public async Task<Result<CollectionReceiptDetailDto>> Handle(
        ReverseCollectionReceiptCommand request,
        CancellationToken cancellationToken)
    {
        var receipt = await repository.GetByIdAsync(request.CollectionReceiptId, cancellationToken);

        if (receipt is null)
        {
            return Result.Failure<CollectionReceiptDetailDto>(
                Error.NotFound("Collections.Receipt.NotFound", "Collection receipt not found."));
        }

        try
        {
            receipt.Reverse();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<CollectionReceiptDetailDto>(
                Error.Validation("Collections.Reverse.Invalid", ex.Message));
        }

        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.ToDetail(receipt));
    }
}
