using Patsanstha.Modules.Collections.Domain.Entities;

namespace Patsanstha.Modules.Collections.Application.Abstractions;

public interface ICollectionReceiptRepository
{
    Task AddAsync(CollectionReceipt receipt, CancellationToken cancellationToken = default);

    Task<CollectionReceipt?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<CollectionReceipt> Items, int TotalCount)> ListAsync(
        ListCollectionReceiptsCriteria criteria,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IReceiptNumberGenerator
{
    Task<string> GenerateNextAsync(CancellationToken cancellationToken = default);
}

public interface ICollectionReceiptMapper
{
    CollectionReceiptSummaryDto ToSummary(CollectionReceipt receipt);

    CollectionReceiptDetailDto ToDetail(CollectionReceipt receipt);
}
