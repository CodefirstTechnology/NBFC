using FluentValidation;
using Patsanstha.BuildingBlocks.Application.Abstractions.Messaging;
using Patsanstha.Modules.Members.Application.Abstractions;
using Patsanstha.Modules.Members.Domain.Entities;
using Patsanstha.Modules.Members.Domain.Enums;

namespace Patsanstha.Modules.Members.Application.Members.SaveOnboardingDraft;

public sealed record SaveOnboardingDraftCommand(
    Guid? MemberId,
    Guid BranchId,
    int OnboardingStep,
    string? FullName,
    string? DateOfBirth,
    string? Gender,
    string? MobileNumber,
    string? Email,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? State,
    string? PinCode,
    string? Aadhaar,
    string? Pan,
    string? NomineeName,
    string? NomineeRelation,
    string? NomineeDateOfBirth,
    int? NomineeSharePercent,
    bool? NomineeAddressSameAsMember,
    string? NomineeAddressLine1,
    string? NomineeAddressLine2,
    string? NomineeCity,
    string? NomineeState,
    string? NomineePinCode,
    int? NumberOfShares,
    decimal? ShareFaceValue,
    SharePaymentMode? SharePaymentMode,
    EmploymentType? EmploymentType,
    string? Occupation,
    string? EmployerName,
    decimal? MonthlyIncome) : ICommand<MemberDetailDto>;

public sealed class SaveOnboardingDraftCommandValidator : AbstractValidator<SaveOnboardingDraftCommand>
{
    public SaveOnboardingDraftCommandValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty().WithErrorCode("Members.BranchId.Required");
        RuleFor(x => x.OnboardingStep).InclusiveBetween(1, 4).WithErrorCode("Members.OnboardingStep.Invalid");
        RuleFor(x => x.MobileNumber).Matches(@"^[6-9]\d{9}$")
            .When(x => !string.IsNullOrWhiteSpace(x.MobileNumber))
            .WithErrorCode("Members.Mobile.Invalid");
        RuleFor(x => x.PinCode).Matches(@"^\d{6}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PinCode))
            .WithErrorCode("Members.PinCode.Invalid");
        RuleFor(x => x.Aadhaar).Matches(@"^\d{12}$")
            .When(x => !string.IsNullOrWhiteSpace(x.Aadhaar))
            .WithErrorCode("Members.Aadhaar.Invalid");
        RuleFor(x => x.Pan).Matches(@"^[A-Za-z]{5}\d{4}[A-Za-z]$")
            .When(x => !string.IsNullOrWhiteSpace(x.Pan))
            .WithErrorCode("Members.Pan.Invalid");
    }
}

public sealed class SaveOnboardingDraftCommandHandler(
    IMemberRepository memberRepository,
    IMemberNumberGenerator memberNumberGenerator,
    IPiiEncryptionService piiEncryption,
    IMemberMapper memberMapper,
    Patsanstha.BuildingBlocks.Application.Abstractions.Audit.IAuditContextAccessor auditContext)
    : ICommandHandler<SaveOnboardingDraftCommand, MemberDetailDto>
{
    public async Task<Result<MemberDetailDto>> Handle(
        SaveOnboardingDraftCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = auditContext.Current.TenantId;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<MemberDetailDto>(
                Error.Validation("Members.Tenant.Required", "Tenant context is required."));
        }

        Member? member = null;

        if (request.MemberId.HasValue)
        {
            member = await memberRepository.GetByIdWithDocumentsAsync(request.MemberId.Value, cancellationToken);
            if (member is null)
            {
                return Result.Failure<MemberDetailDto>(
                    Error.NotFound("Members.NotFound", "Member draft not found."));
            }
        }

        string? aadhaarEncrypted = null;
        string? panEncrypted = null;
        string? aadhaarHash = null;

        if (!string.IsNullOrWhiteSpace(request.Aadhaar))
        {
            aadhaarHash = piiEncryption.HashForLookup(request.Aadhaar);
            if (member is null || member.AadhaarHash != aadhaarHash)
            {
                if (await memberRepository.ExistsByAadhaarHashAsync(aadhaarHash, cancellationToken))
                {
                    return Result.Failure<MemberDetailDto>(
                        Error.Conflict("Members.Aadhaar.Exists", "A member with this Aadhaar already exists."));
                }
            }

            aadhaarEncrypted = piiEncryption.Encrypt(request.Aadhaar);
        }

        if (!string.IsNullOrWhiteSpace(request.Pan))
        {
            panEncrypted = piiEncryption.Encrypt(request.Pan.ToUpperInvariant());
        }

        DateOnly? dateOfBirth = TryParseDate(request.DateOfBirth);
        DateOnly? nomineeDateOfBirth = TryParseDate(request.NomineeDateOfBirth);

        if (member is null)
        {
            var memberNumber = await memberNumberGenerator.GenerateNextAsync(cancellationToken);
            member = Member.StartDraft(tenantId, request.BranchId, memberNumber);
            await memberRepository.AddAsync(member, cancellationToken);
        }

        try
        {
            member.UpdateOnboardingDraft(
                request.OnboardingStep,
                request.FullName,
                dateOfBirth,
                request.Gender,
                request.MobileNumber,
                request.Email,
                request.AddressLine1,
                request.AddressLine2,
                request.City,
                request.State,
                request.PinCode,
                aadhaarEncrypted,
                panEncrypted,
                aadhaarHash,
                nomineeName: request.NomineeName,
                nomineeRelation: request.NomineeRelation,
                nomineeDateOfBirth: nomineeDateOfBirth,
                nomineeSharePercent: request.NomineeSharePercent,
                nomineeAddressSameAsMember: request.NomineeAddressSameAsMember,
                nomineeAddressLine1: request.NomineeAddressLine1,
                nomineeAddressLine2: request.NomineeAddressLine2,
                nomineeCity: request.NomineeCity,
                nomineeState: request.NomineeState,
                nomineePinCode: request.NomineePinCode,
                numberOfShares: request.NumberOfShares,
                shareFaceValue: request.ShareFaceValue,
                sharePaymentMode: request.SharePaymentMode,
                employmentType: request.EmploymentType,
                occupation: request.Occupation,
                employerName: request.EmployerName,
                monthlyIncome: request.MonthlyIncome);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<MemberDetailDto>(
                Error.Validation("Members.Draft.Invalid", ex.Message));
        }

        await memberRepository.SaveChangesAsync(cancellationToken);

        var saved = await memberRepository.GetByIdWithDocumentsAsync(member.Id, cancellationToken);
        return Result.Success(memberMapper.ToDetail(saved!));
    }

    private static DateOnly? TryParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateOnly.TryParse(value.Trim(), out var parsed) ? parsed : null;
    }
}
