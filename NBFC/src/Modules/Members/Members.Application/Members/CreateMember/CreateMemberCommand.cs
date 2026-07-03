using FluentValidation;
using Patsanstha.BuildingBlocks.Application.Abstractions.Messaging;
using Patsanstha.Modules.Members.Application.Abstractions;
using Patsanstha.Modules.Members.Domain.Entities;
using Patsanstha.Modules.Members.Domain.Enums;

namespace Patsanstha.Modules.Members.Application.Members.CreateMember;

public sealed record CreateMemberCommand(
    Guid BranchId,
    string FullName,
    DateOnly DateOfBirth,
    string Gender,
    string MobileNumber,
    string? Email,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string PinCode,
    string Aadhaar,
    string Pan,
    string? NomineeName,
    string? NomineeRelation,
    DateOnly? NomineeDateOfBirth = null,
    int NomineeSharePercent = 100,
    bool NomineeAddressSameAsMember = true,
    string? NomineeAddressLine1 = null,
    string? NomineeAddressLine2 = null,
    string? NomineeCity = null,
    string? NomineeState = null,
    string? NomineePinCode = null,
    int? NumberOfShares = null,
    decimal ShareFaceValue = Member.DefaultShareFaceValue,
    SharePaymentMode? SharePaymentMode = null,
    EmploymentType? EmploymentType = null,
    string? Occupation = null,
    string? EmployerName = null,
    decimal? MonthlyIncome = null,
    KycVerificationStatus AadhaarVerificationStatus = KycVerificationStatus.Pending,
    KycVerificationStatus PanVerificationStatus = KycVerificationStatus.Pending,
    string? PanVerifiedName = null) : ICommand<MemberDetailDto>;

public sealed class CreateMemberCommandValidator : AbstractValidator<CreateMemberCommand>
{
    public CreateMemberCommandValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty().WithErrorCode("Members.BranchId.Required");
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200).WithErrorCode("Members.FullName.Required");
        RuleFor(x => x.DateOfBirth).LessThan(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithErrorCode("Members.DateOfBirth.Invalid");
        RuleFor(x => x.Gender).NotEmpty().MaximumLength(20).WithErrorCode("Members.Gender.Required");
        RuleFor(x => x.MobileNumber).NotEmpty().Matches(@"^[6-9]\d{9}$").WithErrorCode("Members.Mobile.Invalid");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithErrorCode("Members.Email.Invalid");
        RuleFor(x => x.AddressLine1).NotEmpty().MaximumLength(300).WithErrorCode("Members.Address.Required");
        RuleFor(x => x.City).NotEmpty().MaximumLength(100).WithErrorCode("Members.City.Required");
        RuleFor(x => x.State).NotEmpty().MaximumLength(100).WithErrorCode("Members.State.Required");
        RuleFor(x => x.PinCode).NotEmpty().Matches(@"^\d{6}$").WithErrorCode("Members.PinCode.Invalid");
        RuleFor(x => x.Aadhaar).NotEmpty().Matches(@"^\d{12}$").WithErrorCode("Members.Aadhaar.Invalid");
        RuleFor(x => x.Pan).NotEmpty().Matches(@"^[A-Za-z]{5}\d{4}[A-Za-z]$").WithErrorCode("Members.Pan.Invalid");
        RuleFor(x => x.NomineeSharePercent).InclusiveBetween(1, 100)
            .WithErrorCode("Members.NomineeSharePercent.Invalid");
    }
}

public sealed class CreateMemberCommandHandler(
    IMemberRepository memberRepository,
    IMemberNumberGenerator memberNumberGenerator,
    IPiiEncryptionService piiEncryption,
    IMemberMapper memberMapper,
    Patsanstha.BuildingBlocks.Application.Abstractions.Audit.IAuditContextAccessor auditContext)
    : ICommandHandler<CreateMemberCommand, MemberDetailDto>
{
    public async Task<Result<MemberDetailDto>> Handle(
        CreateMemberCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = auditContext.Current.TenantId;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<MemberDetailDto>(
                Error.Validation("Members.Tenant.Required", "Tenant context is required."));
        }

        var aadhaarHash = piiEncryption.HashForLookup(request.Aadhaar);

        if (await memberRepository.ExistsByAadhaarHashAsync(aadhaarHash, cancellationToken))
        {
            return Result.Failure<MemberDetailDto>(
                Error.Conflict("Members.Aadhaar.Exists", "A member with this Aadhaar already exists."));
        }

        var memberNumber = await memberNumberGenerator.GenerateNextAsync(cancellationToken);

        var member = Member.Register(
            tenantId,
            request.BranchId,
            memberNumber,
            request.FullName,
            request.DateOfBirth,
            request.Gender,
            request.MobileNumber,
            request.Email,
            request.AddressLine1,
            request.AddressLine2,
            request.City,
            request.State,
            request.PinCode,
            piiEncryption.Encrypt(request.Aadhaar),
            piiEncryption.Encrypt(request.Pan.ToUpperInvariant()),
            aadhaarHash,
            request.NomineeName,
            request.NomineeRelation,
            DateOnly.FromDateTime(DateTime.UtcNow),
            photoStorageKey: null,
            aadhaarVerificationStatus: request.AadhaarVerificationStatus,
            panVerificationStatus: request.PanVerificationStatus,
            panVerifiedName: request.PanVerifiedName,
            nomineeDateOfBirth: request.NomineeDateOfBirth,
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

        await memberRepository.AddAsync(member, cancellationToken);
        await memberRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(memberMapper.ToDetail(member));
    }
}
