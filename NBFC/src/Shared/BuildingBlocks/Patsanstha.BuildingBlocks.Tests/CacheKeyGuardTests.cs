using Patsanstha.BuildingBlocks.Infrastructure.Caching;

namespace Patsanstha.BuildingBlocks.Tests;

public sealed class CacheKeyGuardTests
{
    [Theory]
    [InlineData("ref:permissions")]
    [InlineData("ref:loan-products")]
    [InlineData("kpi:tenant:branch")]
    public void EnsureAllowed_permits_reference_data_keys(string key)
    {
        var exception = Record.Exception(() => CacheKeyGuard.EnsureAllowed(key));
        Assert.Null(exception);
    }

    [Theory]
    [InlineData("member:balance:123")]
    [InlineData("loan-outstanding:456")]
    [InlineData("emi-due:789")]
    [InlineData("account-balance:001")]
    public void EnsureAllowed_rejects_financial_keys(string key)
    {
        var exception = Assert.Throws<InvalidOperationException>(() => CacheKeyGuard.EnsureAllowed(key));
        Assert.Contains("must never be cached", exception.Message);
    }
}
