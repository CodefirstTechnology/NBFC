using System.Diagnostics;

namespace Patsanstha.BuildingBlocks.Infrastructure.Observability;

public static class PatsansthaTelemetry
{
    public const string ServiceName = "Patsanstha.Api";
    public const string ActivitySourceName = "Patsanstha.BuildingBlocks";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}
