namespace Patsanstha.BuildingBlocks.Infrastructure.BackgroundJobs;

public sealed class BackgroundJobOptions
{
    public const string SectionName = "BackgroundJobs";

    public bool Enabled { get; set; } = true;

    public string EmiGenerationCron { get; set; } = "0 1 * * *";

    public string InterestAccrualCron { get; set; } = "0 2 * * *";

    public string AgeingRecalculationCron { get; set; } = "0 3 * * *";

    public string NotificationDispatchCron { get; set; } = "*/5 * * * *";

    public string ReportPreAggregationCron { get; set; } = "0 4 * * *";

    public Guid DefaultTenantId { get; set; } = Guid.Parse("00000000-0000-0000-0000-000000000001");
}
