using Patsanstha.BuildingBlocks.Application.Abstractions.Caching;
using Patsanstha.BuildingBlocks.Application.Abstractions.Messaging;
using Patsanstha.Modules.Loans.Application.Abstractions;
using Patsanstha.Modules.Loans.Domain.Enums;

namespace Patsanstha.Modules.Loans.Application.LoanProducts;

public sealed record LoanProductDto(LoanProductType ProductType, decimal DefaultInterestRatePercent);

public sealed record GetLoanProductsQuery : IQuery<IReadOnlyList<LoanProductDto>>;

public sealed class GetLoanProductsQueryHandler(IReferenceDataCache referenceDataCache)
    : IQueryHandler<GetLoanProductsQuery, IReadOnlyList<LoanProductDto>>
{
    public async Task<Result<IReadOnlyList<LoanProductDto>>> Handle(
        GetLoanProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await referenceDataCache.GetOrSetAsync(
            CacheKeys.LoanProducts,
            _ => Task.FromResult<IReadOnlyList<LoanProductDto>>(
            [
                new(LoanProductType.Personal, LoanProductCatalog.GetDefaultInterestRate(LoanProductType.Personal)),
                new(LoanProductType.Gold, LoanProductCatalog.GetDefaultInterestRate(LoanProductType.Gold)),
                new(LoanProductType.Business, LoanProductCatalog.GetDefaultInterestRate(LoanProductType.Business)),
                new(LoanProductType.Vehicle, LoanProductCatalog.GetDefaultInterestRate(LoanProductType.Vehicle)),
            ]),
            cancellationToken: cancellationToken);

        return Result.Success(products);
    }
}
