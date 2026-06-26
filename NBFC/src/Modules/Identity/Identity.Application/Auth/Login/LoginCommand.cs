using FluentValidation;
using Patsanstha.BuildingBlocks.Application.Abstractions.Messaging;
using Patsanstha.Modules.Identity.Application.Abstractions;

namespace Patsanstha.Modules.Identity.Application.Auth.Login;

public sealed record LoginCommand(
    string Email,
    string Password,
    string? IpAddress) : ICommand<AuthLoginResponse>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithErrorCode("Auth.Email.Required");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).WithErrorCode("Auth.Password.Required");
    }
}

public sealed class LoginCommandHandler(
    ISignInService signInService,
    ITokenService tokenService,
    ITwoFactorService twoFactorService) : ICommandHandler<LoginCommand, AuthLoginResponse>
{
    public async Task<Result<AuthLoginResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var signInResult = await signInService.SignInAsync(
            request.Email,
            request.Password,
            cancellationToken);

        if (signInResult.IsFailure)
        {
            return Result.Failure<AuthLoginResponse>(signInResult.Error);
        }

        var user = signInResult.Value;

        if (await twoFactorService.IsTwoFactorEnabledAsync(user.UserId, cancellationToken))
        {
            var challengeResult = await signInService.CreateTwoFactorChallengeAsync(
                user.UserId,
                cancellationToken);

            if (challengeResult.IsFailure)
            {
                return Result.Failure<AuthLoginResponse>(challengeResult.Error);
            }

            return Result.Success(AuthLoginResponse.TwoFactorRequired(
                challengeResult.Value.Token,
                challengeResult.Value.ExpiresAt));
        }

        var tokenResult = await tokenService.IssueTokensAsync(
            user.UserId,
            user.Email,
            user.FullName,
            user.TenantId,
            user.BranchId,
            user.Roles,
            user.Permissions,
            request.IpAddress,
            cancellationToken);

        return tokenResult.IsSuccess
            ? Result.Success(AuthLoginResponse.Complete(tokenResult.Value))
            : Result.Failure<AuthLoginResponse>(tokenResult.Error);
    }
}

public interface ISignInService
{
    Task<Result<AuthenticatedUserResponse>> SignInAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<Result<TwoFactorChallengeResponse>> CreateTwoFactorChallengeAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

public sealed record TwoFactorChallengeResponse(
    string Token,
    DateTimeOffset ExpiresAt);
