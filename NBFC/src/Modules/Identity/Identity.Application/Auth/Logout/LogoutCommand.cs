using FluentValidation;
using Patsanstha.BuildingBlocks.Application.Abstractions.Messaging;
using Patsanstha.Modules.Identity.Application.Abstractions;

namespace Patsanstha.Modules.Identity.Application.Auth.Logout;

public sealed record LogoutCommand(string RefreshToken) : ICommand;

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().WithErrorCode("Auth.RefreshToken.Required");
    }
}

public sealed class LogoutCommandHandler(ITokenService tokenService) : ICommandHandler<LogoutCommand>
{
    public Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken) =>
        tokenService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);
}
