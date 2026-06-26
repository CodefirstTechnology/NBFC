namespace Patsanstha.BuildingBlocks.Domain.Abstractions;

public enum ErrorType
{
    Validation = 0,
    NotFound = 1,
    Conflict = 2,
    Unauthorized = 3,
    Forbidden = 4,
    Failure = 5,
    IdempotencyReplay = 6,
}

public sealed record Error(string Code, string Message, ErrorType Type = ErrorType.Validation)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Validation);

    public static Error Validation(string code, string message) =>
        new(code, message, ErrorType.Validation);

    public static Error NotFound(string code, string message) =>
        new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) =>
        new(code, message, ErrorType.Conflict);

    public static Error Unauthorized(string code, string message) =>
        new(code, message, ErrorType.Unauthorized);

    public static Error Forbidden(string code, string message) =>
        new(code, message, ErrorType.Forbidden);

    public static Error Failure(string code, string message) =>
        new(code, message, ErrorType.Failure);

    public static Error IdempotencyReplay(string code, string message) =>
        new(code, message, ErrorType.IdempotencyReplay);

    public bool IsNone => Type == ErrorType.Validation && Code == string.Empty && Message == string.Empty;
}
