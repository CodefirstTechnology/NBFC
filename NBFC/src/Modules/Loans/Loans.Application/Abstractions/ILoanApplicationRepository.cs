using Patsanstha.Modules.Loans.Domain.Entities;
using Patsanstha.Modules.Loans.Domain.Enums;

namespace Patsanstha.Modules.Loans.Application.Abstractions;

public interface ILoanApplicationRepository
{
    Task AddAsync(LoanApplication application, CancellationToken cancellationToken = default);

    Task<LoanApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<LoanApplication> Items, int TotalCount)> ListAsync(
        ListLoanApplicationsCriteria criteria,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ILoanNumberGenerator
{
    Task<string> GenerateNextAsync(CancellationToken cancellationToken = default);
}

public interface ILoanApplicationMapper
{
    LoanApplicationSummaryDto ToSummary(LoanApplication application);

    LoanApplicationDetailDto ToDetail(LoanApplication application);
}

public static class LoanProductCatalog
{
    public static decimal GetDefaultInterestRate(LoanProductType productType) =>
        productType switch
        {
            LoanProductType.Personal => 12.0m,
            LoanProductType.Gold => 10.5m,
            LoanProductType.Business => 14.0m,
            LoanProductType.Vehicle => 11.5m,
            _ => throw new ArgumentOutOfRangeException(nameof(productType), productType, null),
        };
}

public static class LoanEmiCalculator
{
    public static decimal CalculateEmi(decimal principal, decimal annualRatePercent, int tenureMonths)
    {
        if (principal <= 0 || tenureMonths <= 0)
        {
            return 0;
        }

        var monthlyRate = annualRatePercent / 12m / 100m;

        if (monthlyRate == 0)
        {
            return Math.Round(principal / tenureMonths, 2, MidpointRounding.AwayFromZero);
        }

        var factor = (decimal)Math.Pow((double)(1 + monthlyRate), tenureMonths);
        return Math.Round(principal * monthlyRate * factor / (factor - 1), 2, MidpointRounding.AwayFromZero);
    }
}
