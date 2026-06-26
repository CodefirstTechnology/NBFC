using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Patsanstha.BuildingBlocks.Domain.Abstractions;

namespace Patsanstha.BuildingBlocks.Infrastructure.Api;

public static class ResultExtensions
{
    public static IResult ToHttpResult(this Result result) =>
        result.IsSuccess
            ? Results.NoContent()
            : result.Error.ToProblemResult();

    public static IResult ToHttpResult<T>(this Result<T> result) =>
        result.IsSuccess
            ? Results.Ok(result.Value)
            : result.Error.ToProblemResult();

    public static IResult ToCreatedResult<T>(this Result<T> result, string location) =>
        result.IsSuccess
            ? Results.Created(location, result.Value)
            : result.Error.ToProblemResult();

    public static IResult ToProblemResult(this Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.IdempotencyReplay => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status422UnprocessableEntity,
        };

        return Results.Problem(
            statusCode: statusCode,
            title: error.Code,
            detail: error.Message,
            type: $"https://patsanstha.local/errors/{error.Type.ToString().ToLowerInvariant()}");
    }
}

public static class IdempotencyConstants
{
    public const string HeaderName = "Idempotency-Key";
}
