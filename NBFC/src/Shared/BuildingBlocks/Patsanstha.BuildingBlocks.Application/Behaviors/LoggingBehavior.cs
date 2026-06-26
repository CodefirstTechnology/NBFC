using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Patsanstha.BuildingBlocks.Domain.Abstractions;

namespace Patsanstha.BuildingBlocks.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation(
            "Handling {RequestName} {@Request}",
            requestName,
            request);

        var response = await next();

        if (response is Result { IsFailure: true } result)
        {
            logger.LogWarning(
                "Request {RequestName} failed with error {ErrorCode}: {ErrorMessage}",
                requestName,
                result.Error.Code,
                result.Error.Message);
        }
        else
        {
            logger.LogInformation("Handled {RequestName}", requestName);
        }

        return response;
    }
}
