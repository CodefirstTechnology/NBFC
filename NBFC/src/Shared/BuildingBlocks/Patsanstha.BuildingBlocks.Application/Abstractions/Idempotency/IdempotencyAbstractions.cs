namespace Patsanstha.BuildingBlocks.Application.Abstractions.Idempotency;

public sealed record IdempotencyRecord(
    string Key,
    int StatusCode,
    string ContentType,
    string Body,
    DateTimeOffset StoredAt);

public interface IIdempotencyStore
{
    Task<IdempotencyRecord?> GetAsync(string key, CancellationToken cancellationToken = default);

    Task StoreAsync(IdempotencyRecord record, TimeSpan ttl, CancellationToken cancellationToken = default);
}
