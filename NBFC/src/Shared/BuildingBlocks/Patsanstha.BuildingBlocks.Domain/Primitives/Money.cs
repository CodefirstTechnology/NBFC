namespace Patsanstha.BuildingBlocks.Domain.Primitives;

public readonly record struct Money(decimal Amount, string Currency = "INR")
{
    public static Money Zero => new(0m);

    public static Money FromPaise(long paise, string currency = "INR") =>
        new(paise / 100m, currency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    public bool IsNegative => Amount < 0m;

    public bool IsZero => Amount == 0m;

    public bool IsPositive => Amount > 0m;

    private void EnsureSameCurrency(Money other)
    {
        if (!string.Equals(Currency, other.Currency, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Cannot operate on money with different currencies: {Currency} vs {other.Currency}.");
        }
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}
