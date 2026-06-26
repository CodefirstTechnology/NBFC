using Patsanstha.BuildingBlocks.Domain.Primitives;

namespace Patsanstha.BuildingBlocks.Tests;

public sealed class MoneyTests
{
    [Fact]
    public void Add_combines_amounts_with_same_currency()
    {
        var total = new Money(100m).Add(new Money(50m));

        Assert.Equal(150m, total.Amount);
        Assert.Equal("INR", total.Currency);
    }

    [Fact]
    public void Subtract_combines_amounts_with_same_currency()
    {
        var remainder = new Money(100m).Subtract(new Money(30m));

        Assert.Equal(70m, remainder.Amount);
    }

    [Fact]
    public void Add_throws_when_currencies_differ()
    {
        var inr = new Money(100m, "INR");
        var usd = new Money(50m, "USD");

        Assert.Throws<InvalidOperationException>(() => inr.Add(usd));
    }

    [Fact]
    public void FromPaise_converts_correctly()
    {
        var money = Money.FromPaise(12345);

        Assert.Equal(123.45m, money.Amount);
    }

    [Theory]
    [InlineData(-1, true, false, false)]
    [InlineData(0, false, true, false)]
    [InlineData(1, false, false, true)]
    public void Sign_helpers_work(decimal amount, bool negative, bool zero, bool positive)
    {
        var money = new Money(amount);

        Assert.Equal(negative, money.IsNegative);
        Assert.Equal(zero, money.IsZero);
        Assert.Equal(positive, money.IsPositive);
    }
}
