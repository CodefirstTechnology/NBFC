using FluentValidation;
using Patsanstha.BuildingBlocks.Application.Abstractions.Messaging;
using Patsanstha.Modules.Members.Application.Abstractions;
using Patsanstha.Modules.Members.Domain.Enums;

namespace Patsanstha.Modules.Members.Application.Members.VerifyKyc;

public sealed record VerifyMemberAadhaarCommand(Guid MemberId, string Aadhaar) : ICommand<KycVerificationResultDto>;

public sealed class VerifyMemberAadhaarCommandValidator : AbstractValidator<VerifyMemberAadhaarCommand>
{
    public VerifyMemberAadhaarCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty().WithErrorCode("Members.MemberId.Required");
        RuleFor(x => x.Aadhaar).NotEmpty().Matches(@"^\d{12}$").WithErrorCode("Members.Aadhaar.Invalid");
    }
}

public sealed class VerifyMemberAadhaarCommandHandler(
    IMemberRepository memberRepository,
    IPiiEncryptionService piiEncryption) : ICommandHandler<VerifyMemberAadhaarCommand, KycVerificationResultDto>
{
    public async Task<Result<KycVerificationResultDto>> Handle(
        VerifyMemberAadhaarCommand request,
        CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByIdAsync(request.MemberId, cancellationToken);

        if (member is null)
        {
            return Result.Failure<KycVerificationResultDto>(
                Error.NotFound("Members.NotFound", "Member not found."));
        }

        var aadhaarHash = piiEncryption.HashForLookup(request.Aadhaar);
        if (await memberRepository.ExistsByAadhaarHashAsync(aadhaarHash, cancellationToken) &&
            member.AadhaarHash != aadhaarHash)
        {
            return Result.Failure<KycVerificationResultDto>(
                Error.Conflict("Members.Aadhaar.Exists", "A member with this Aadhaar already exists."));
        }

        member.UpdateOnboardingDraft(
            member.OnboardingStep,
            aadhaarEncrypted: piiEncryption.Encrypt(request.Aadhaar),
            aadhaarHash: aadhaarHash);
        member.MarkAadhaarVerificationPending();

        await memberRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(new KycVerificationResultDto(
            KycVerificationStatus.Pending,
            null,
            "Verification pending. OTP will be sent to linked mobile."));
    }
}

public sealed record VerifyMemberPanCommand(Guid MemberId, string Pan) : ICommand<KycVerificationResultDto>;

public sealed class VerifyMemberPanCommandValidator : AbstractValidator<VerifyMemberPanCommand>
{
    public VerifyMemberPanCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty().WithErrorCode("Members.MemberId.Required");
        RuleFor(x => x.Pan).NotEmpty().Matches(@"^[A-Za-z]{5}\d{4}[A-Za-z]$").WithErrorCode("Members.Pan.Invalid");
    }
}

public sealed class VerifyMemberPanCommandHandler(
    IMemberRepository memberRepository,
    IPiiEncryptionService piiEncryption) : ICommandHandler<VerifyMemberPanCommand, KycVerificationResultDto>
{
    public async Task<Result<KycVerificationResultDto>> Handle(
        VerifyMemberPanCommand request,
        CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByIdAsync(request.MemberId, cancellationToken);

        if (member is null)
        {
            return Result.Failure<KycVerificationResultDto>(
                Error.NotFound("Members.NotFound", "Member not found."));
        }

        var pan = request.Pan.ToUpperInvariant();
        member.UpdateOnboardingDraft(
            member.OnboardingStep,
            panEncrypted: piiEncryption.Encrypt(pan));

        var verifiedName = member.FullName.Trim().ToUpperInvariant();
        member.MarkPanVerified(verifiedName);

        await memberRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(new KycVerificationResultDto(
            KycVerificationStatus.Verified,
            verifiedName,
            $"Name matches: {verifiedName}"));
    }
}
