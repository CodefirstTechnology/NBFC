using Patsanstha.Modules.Deposits.Domain.Enums;

namespace Patsanstha.Modules.Deposits.Application.Abstractions;

public static class DepositProductCatalog
{
    public static decimal GetDefaultInterestRate(DepositProductType productType) =>
        productType switch
        {
            DepositProductType.Savings => 4.0m,
            DepositProductType.RecurringDeposit => 6.5m,
            DepositProductType.FixedDeposit => 7.2m,
            _ => throw new ArgumentOutOfRangeException(nameof(productType), productType, null),
        };

    public static bool RequiresTenure(DepositProductType productType) =>
        productType is DepositProductType.RecurringDeposit or DepositProductType.FixedDeposit;
}
