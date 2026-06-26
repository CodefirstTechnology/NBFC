using FluentValidation;
using Patsanstha.BuildingBlocks.Application.Abstractions.Messaging;
using Patsanstha.Modules.Identity.Application.Abstractions;

namespace Patsanstha.Modules.Identity.Application.Auth.TwoFactor;

public sealed record VerifyTwoFactorLoginCommand(
    string ChallengeToken,
    string VerificationCode,
    string? IpAddress) : ICommand<AuthLoginResponse>;

public sealed class VerifyTwoFactorLoginCommandValidator : AbstractValidator<VerifyTwoFactorLoginCommand>
{
    public VerifyTwoFactorLoginCommandValidator()
    {
        RuleFor(x => x.ChallengeToken).NotEmpty().WithErrorCode("Auth.ChallengeToken.Required");
        RuleFor(x => x.VerificationCode).NotEmpty().Length(6).WithErrorCode("Auth.VerificationCode.Required");
    }
}

public sealed class VerifyTwoFactorLoginCommandHandler(ITwoFactorService twoFactorService)
    : ICommandHandler<VerifyTwoFactorLoginCommand, AuthLoginResponse>
{
    public Task<Result<AuthLoginResponse>> Handle(
        VerifyTwoFactorLoginCommand request,
        CancellationToken cancellationToken) =>
        twoFactorService.CompleteLoginAsync(
            request.ChallengeToken,
            request.VerificationCode,
            request.IpAddress,
            cancellationToken);
}

public sealed record SetupTwoFactorCommand(Guid UserId) : ICommand<TwoFactorSetupResponse>;

public sealed class SetupTwoFactorCommandHandler(ITwoFactorService twoFactorService)
    : ICommandHandler<SetupTwoFactorCommand, TwoFactorSetupResponse>
{
    public Task<Result<TwoFactorSetupResponse>> Handle(
        SetupTwoFactorCommand request,
        CancellationToken cancellationToken) =>
        twoFactorService.BeginSetupAsync(request.UserId, cancellationToken);
}

public sealed record ConfirmTwoFactorCommand(
    Guid UserId,
    string VerificationCode) : ICommand;

public sealed class ConfirmTwoFactorCommandValidator : AbstractValidator<ConfirmTwoFactorCommand>
{
    public ConfirmTwoFactorCommandValidator()
    {
        RuleFor(x => x.VerificationCode).NotEmpty().Length(6).WithErrorCode("Auth.VerificationCode.Required");
    }
}

public sealed class ConfirmTwoFactorCommandHandler(ITwoFactorService twoFactorService)
    : ICommandHandler<ConfirmTwoFactorCommand>
{
    public Task<Result> Handle(ConfirmTwoFactorCommand request, CancellationToken cancellationToken) =>
        twoFactorService.ConfirmSetupAsync(request.UserId, request.VerificationCode, cancellationToken);
}

public sealed record DisableTwoFactorCommand(
    Guid UserId,
    string VerificationCode) : ICommand;

public sealed class DisableTwoFactorCommandValidator : AbstractValidator<DisableTwoFactorCommand>
{
    public DisableTwoFactorCommandValidator()
    {
        RuleFor(x => x.VerificationCode).NotEmpty().Length(6).WithErrorCode("Auth.VerificationCode.Required");
    }
}

public sealed class DisableTwoFactorCommandHandler(ITwoFactorService twoFactorService)
    : ICommandHandler<DisableTwoFactorCommand>
{
    public Task<Result> Handle(DisableTwoFactorCommand request, CancellationToken cancellationToken) =>
        twoFactorService.DisableAsync(request.UserId, request.VerificationCode, cancellationToken);
}
