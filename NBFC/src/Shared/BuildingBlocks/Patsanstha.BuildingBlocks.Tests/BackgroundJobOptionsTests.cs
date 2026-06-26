using Patsanstha.BuildingBlocks.Infrastructure.BackgroundJobs;

namespace Patsanstha.BuildingBlocks.Tests;

public sealed class BackgroundJobOptionsTests
{
    [Fact]
    public void DefaultTenantId_matches_identity_seed_default()
    {
        var options = new BackgroundJobOptions();

        Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000001"), options.DefaultTenantId);
    }

    [Theory]
    [InlineData("0 1 * * *")]
    [InlineData("*/5 * * * *")]
    public void Default_cron_expressions_are_configured(string cron)
    {
        var options = new BackgroundJobOptions();

        Assert.Contains(cron, new[]
        {
            options.EmiGenerationCron,
            options.InterestAccrualCron,
            options.AgeingRecalculationCron,
            options.NotificationDispatchCron,
            options.ReportPreAggregationCron,
        });
    }
}
