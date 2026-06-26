namespace Patsanstha.BuildingBlocks.Application.Abstractions.Observability;

public static class CorrelationIdConstants
{
    public const string HeaderName = "X-Correlation-Id";
    public const string HttpContextItemKey = "CorrelationId";
    public const string LogPropertyName = "CorrelationId";
}
