using FluentValidation;
using Patsanstha.BuildingBlocks.Application.Abstractions.Messaging;
using Patsanstha.Modules.Identity.Application.Abstractions;

namespace Patsanstha.Modules.Identity.Application.Auth.Refresh;

public sealed record RefreshTokenCommand(
    string RefreshToken,
    string? IpAddress) : ICommand<TokenPairResponse>;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().WithErrorCode("Auth.RefreshToken.Required");
    }
}

public sealed class RefreshTokenCommandHandler(ITokenService tokenService)
    : ICommandHandler<RefreshTokenCommand, TokenPairResponse>
{
    public Task<Result<TokenPairResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken) =>
        tokenService.RotateRefreshTokenAsync(
            request.RefreshToken,
            request.IpAddress,
            cancellationToken);
}
