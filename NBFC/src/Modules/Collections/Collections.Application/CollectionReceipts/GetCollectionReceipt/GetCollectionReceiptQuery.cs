using Patsanstha.Modules.Collections.Application.Abstractions;

namespace Patsanstha.Modules.Collections.Application.CollectionReceipts.GetCollectionReceipt;

public sealed record GetCollectionReceiptQuery(Guid CollectionReceiptId) : IQuery<CollectionReceiptDetailDto>;

public sealed class GetCollectionReceiptQueryHandler(
    ICollectionReceiptRepository repository,
    ICollectionReceiptMapper mapper) : IQueryHandler<GetCollectionReceiptQuery, CollectionReceiptDetailDto>
{
    public async Task<Result<CollectionReceiptDetailDto>> Handle(
        GetCollectionReceiptQuery request,
        CancellationToken cancellationToken)
    {
        var receipt = await repository.GetByIdAsync(request.CollectionReceiptId, cancellationToken);

        if (receipt is null)
        {
            return Result.Failure<CollectionReceiptDetailDto>(
                Error.NotFound("Collections.Receipt.NotFound", "Collection receipt not found."));
        }

        return Result.Success(mapper.ToDetail(receipt));
    }
}
