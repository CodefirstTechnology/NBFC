using Patsanstha.BuildingBlocks.Domain.Abstractions;
using Patsanstha.Modules.Members.Domain.Enums;
using Patsanstha.Modules.Members.Domain.Events;

namespace Patsanstha.Modules.Members.Domain.Entities;

public sealed class Member : AggregateRoot
{
    public const decimal DefaultShareFaceValue = 100m;

    private static readonly Dictionary<MemberStatus, HashSet<MemberStatus>> AllowedTransitions = new()
    {
        [MemberStatus.Pending] = [MemberStatus.Active, MemberStatus.Inactive],
        [MemberStatus.Active] = [MemberStatus.Inactive, MemberStatus.Suspended, MemberStatus.Closed],
        [MemberStatus.Inactive] = [MemberStatus.Active, MemberStatus.Closed],
        [MemberStatus.Suspended] = [MemberStatus.Active, MemberStatus.Closed],
        [MemberStatus.Closed] = [],
    };

    public Guid BranchId { get; private set; }

    public string MemberNumber { get; private set; } = string.Empty;

    public string FullName { get; private set; } = string.Empty;

    public DateOnly? DateOfBirth { get; private set; }

    public string Gender { get; private set; } = string.Empty;

    public string MobileNumber { get; private set; } = string.Empty;

    public string? Email { get; private set; }

    public string AddressLine1 { get; private set; } = string.Empty;

    public string? AddressLine2 { get; private set; }

    public string City { get; private set; } = string.Empty;

    public string State { get; private set; } = string.Empty;

    public string PinCode { get; private set; } = string.Empty;

    public string? AadhaarEncrypted { get; private set; }

    public string? PanEncrypted { get; private set; }

    public string? AadhaarHash { get; private set; }

    public string? PhotoStorageKey { get; private set; }

    public KycVerificationStatus AadhaarVerificationStatus { get; private set; } = KycVerificationStatus.Pending;

    public KycVerificationStatus PanVerificationStatus { get; private set; } = KycVerificationStatus.Pending;

    public string? PanVerifiedName { get; private set; }

    public string? NomineeName { get; private set; }

    public string? NomineeRelation { get; private set; }

    public DateOnly? NomineeDateOfBirth { get; private set; }

    public int NomineeSharePercent { get; private set; } = 100;

    public bool NomineeAddressSameAsMember { get; private set; } = true;

    public string? NomineeAddressLine1 { get; private set; }

    public string? NomineeAddressLine2 { get; private set; }

    public string? NomineeCity { get; private set; }

    public string? NomineeState { get; private set; }

    public string? NomineePinCode { get; private set; }

    public int? NumberOfShares { get; private set; }

    public decimal ShareFaceValue { get; private set; } = DefaultShareFaceValue;

    public SharePaymentMode? SharePaymentMode { get; private set; }

    public EmploymentType? EmploymentType { get; private set; }

    public string? Occupation { get; private set; }

    public string? EmployerName { get; private set; }

    public decimal? MonthlyIncome { get; private set; }

    public int OnboardingStep { get; private set; } = 1;

    public MemberStatus Status { get; private set; }

    public DateOnly JoinedOn { get; private set; }

    private readonly List<MemberDocument> _documents = [];

    public IReadOnlyCollection<MemberDocument> Documents => _documents.AsReadOnly();

    private Member()
    {
    }

    public static Member StartDraft(
        Guid tenantId,
        Guid branchId,
        string memberNumber)
    {
        if (tenantId == Guid.Empty)
        {
            throw new InvalidOperationException("TenantId is required.");
        }

        if (branchId == Guid.Empty)
        {
            throw new InvalidOperationException("BranchId is required.");
        }

        if (string.IsNullOrWhiteSpace(memberNumber))
        {
            throw new InvalidOperationException("Member number is required.");
        }

        return new Member
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BranchId = branchId,
            MemberNumber = memberNumber.Trim().ToUpperInvariant(),
            Status = MemberStatus.Pending,
            JoinedOn = DateOnly.FromDateTime(DateTime.UtcNow),
            OnboardingStep = 1,
        };
    }

    public static Member Register(
        Guid tenantId,
        Guid branchId,
        string memberNumber,
        string fullName,
        DateOnly dateOfBirth,
        string gender,
        string mobileNumber,
        string? email,
        string addressLine1,
        string? addressLine2,
        string city,
        string state,
        string pinCode,
        string aadhaarEncrypted,
        string panEncrypted,
        string aadhaarHash,
        string? nomineeName,
        string? nomineeRelation,
        DateOnly joinedOn,
        string? photoStorageKey = null,
        KycVerificationStatus aadhaarVerificationStatus = KycVerificationStatus.Pending,
        KycVerificationStatus panVerificationStatus = KycVerificationStatus.Pending,
        string? panVerifiedName = null,
        DateOnly? nomineeDateOfBirth = null,
        int nomineeSharePercent = 100,
        bool nomineeAddressSameAsMember = true,
        string? nomineeAddressLine1 = null,
        string? nomineeAddressLine2 = null,
        string? nomineeCity = null,
        string? nomineeState = null,
        string? nomineePinCode = null,
        int? numberOfShares = null,
        decimal shareFaceValue = DefaultShareFaceValue,
        SharePaymentMode? sharePaymentMode = null,
        EmploymentType? employmentType = null,
        string? occupation = null,
        string? employerName = null,
        decimal? monthlyIncome = null)
    {
        if (tenantId == Guid.Empty)
        {
            throw new InvalidOperationException("TenantId is required.");
        }

        if (branchId == Guid.Empty)
        {
            throw new InvalidOperationException("BranchId is required.");
        }

        if (string.IsNullOrWhiteSpace(memberNumber))
        {
            throw new InvalidOperationException("Member number is required.");
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new InvalidOperationException("Full name is required.");
        }

        if (string.IsNullOrWhiteSpace(aadhaarEncrypted) || string.IsNullOrWhiteSpace(aadhaarHash))
        {
            throw new InvalidOperationException("Aadhaar is required.");
        }

        if (string.IsNullOrWhiteSpace(panEncrypted))
        {
            throw new InvalidOperationException("PAN is required.");
        }

        var member = new Member
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BranchId = branchId,
            MemberNumber = memberNumber.Trim().ToUpperInvariant(),
            FullName = fullName.Trim(),
            DateOfBirth = dateOfBirth,
            Gender = gender.Trim(),
            MobileNumber = mobileNumber.Trim(),
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
            AddressLine1 = addressLine1.Trim(),
            AddressLine2 = string.IsNullOrWhiteSpace(addressLine2) ? null : addressLine2.Trim(),
            City = city.Trim(),
            State = state.Trim(),
            PinCode = pinCode.Trim(),
            AadhaarEncrypted = aadhaarEncrypted,
            PanEncrypted = panEncrypted,
            AadhaarHash = aadhaarHash,
            PhotoStorageKey = string.IsNullOrWhiteSpace(photoStorageKey) ? null : photoStorageKey.Trim(),
            AadhaarVerificationStatus = aadhaarVerificationStatus,
            PanVerificationStatus = panVerificationStatus,
            PanVerifiedName = string.IsNullOrWhiteSpace(panVerifiedName) ? null : panVerifiedName.Trim(),
            NomineeName = string.IsNullOrWhiteSpace(nomineeName) ? null : nomineeName.Trim(),
            NomineeRelation = string.IsNullOrWhiteSpace(nomineeRelation) ? null : nomineeRelation.Trim(),
            NomineeDateOfBirth = nomineeDateOfBirth,
            NomineeSharePercent = nomineeSharePercent is < 1 or > 100 ? 100 : nomineeSharePercent,
            NomineeAddressSameAsMember = nomineeAddressSameAsMember,
            NomineeAddressLine1 = string.IsNullOrWhiteSpace(nomineeAddressLine1) ? null : nomineeAddressLine1.Trim(),
            NomineeAddressLine2 = string.IsNullOrWhiteSpace(nomineeAddressLine2) ? null : nomineeAddressLine2.Trim(),
            NomineeCity = string.IsNullOrWhiteSpace(nomineeCity) ? null : nomineeCity.Trim(),
            NomineeState = string.IsNullOrWhiteSpace(nomineeState) ? null : nomineeState.Trim(),
            NomineePinCode = string.IsNullOrWhiteSpace(nomineePinCode) ? null : nomineePinCode.Trim(),
            NumberOfShares = numberOfShares,
            ShareFaceValue = shareFaceValue <= 0 ? DefaultShareFaceValue : shareFaceValue,
            SharePaymentMode = sharePaymentMode,
            EmploymentType = employmentType,
            Occupation = string.IsNullOrWhiteSpace(occupation) ? null : occupation.Trim(),
            EmployerName = string.IsNullOrWhiteSpace(employerName) ? null : employerName.Trim(),
            MonthlyIncome = monthlyIncome,
            OnboardingStep = 4,
            Status = MemberStatus.Active,
            JoinedOn = joinedOn,
        };

        member.RaiseDomainEvent(new MemberRegisteredDomainEvent(
            member.Id,
            member.TenantId,
            member.BranchId,
            member.MemberNumber,
            member.FullName));

        return member;
    }

    public void UpdateOnboardingDraft(
        int onboardingStep,
        string? fullName = null,
        DateOnly? dateOfBirth = null,
        string? gender = null,
        string? mobileNumber = null,
        string? email = null,
        string? addressLine1 = null,
        string? addressLine2 = null,
        string? city = null,
        string? state = null,
        string? pinCode = null,
        string? aadhaarEncrypted = null,
        string? panEncrypted = null,
        string? aadhaarHash = null,
        string? photoStorageKey = null,
        KycVerificationStatus? aadhaarVerificationStatus = null,
        KycVerificationStatus? panVerificationStatus = null,
        string? panVerifiedName = null,
        string? nomineeName = null,
        string? nomineeRelation = null,
        DateOnly? nomineeDateOfBirth = null,
        int? nomineeSharePercent = null,
        bool? nomineeAddressSameAsMember = null,
        string? nomineeAddressLine1 = null,
        string? nomineeAddressLine2 = null,
        string? nomineeCity = null,
        string? nomineeState = null,
        string? nomineePinCode = null,
        int? numberOfShares = null,
        decimal? shareFaceValue = null,
        SharePaymentMode? sharePaymentMode = null,
        EmploymentType? employmentType = null,
        string? occupation = null,
        string? employerName = null,
        decimal? monthlyIncome = null)
    {
        EnsureNotClosed();

        if (Status != MemberStatus.Pending)
        {
            throw new InvalidOperationException("Only pending onboarding drafts can be updated.");
        }

        OnboardingStep = onboardingStep is < 1 or > 4 ? OnboardingStep : onboardingStep;

        if (!string.IsNullOrWhiteSpace(fullName))
        {
            FullName = fullName.Trim();
        }

        if (dateOfBirth.HasValue)
        {
            DateOfBirth = dateOfBirth;
        }

        if (!string.IsNullOrWhiteSpace(gender))
        {
            Gender = gender.Trim();
        }

        if (!string.IsNullOrWhiteSpace(mobileNumber))
        {
            MobileNumber = mobileNumber.Trim();
        }

        Email = string.IsNullOrWhiteSpace(email) ? Email : email.Trim();
        AddressLine1 = string.IsNullOrWhiteSpace(addressLine1) ? AddressLine1 : addressLine1.Trim();
        AddressLine2 = string.IsNullOrWhiteSpace(addressLine2) ? AddressLine2 : addressLine2.Trim();
        City = string.IsNullOrWhiteSpace(city) ? City : city.Trim();
        State = string.IsNullOrWhiteSpace(state) ? State : state.Trim();
        PinCode = string.IsNullOrWhiteSpace(pinCode) ? PinCode : pinCode.Trim();

        if (!string.IsNullOrWhiteSpace(aadhaarEncrypted))
        {
            AadhaarEncrypted = aadhaarEncrypted;
        }

        if (!string.IsNullOrWhiteSpace(panEncrypted))
        {
            PanEncrypted = panEncrypted;
        }

        if (!string.IsNullOrWhiteSpace(aadhaarHash))
        {
            AadhaarHash = aadhaarHash;
        }

        if (!string.IsNullOrWhiteSpace(photoStorageKey))
        {
            PhotoStorageKey = photoStorageKey.Trim();
        }

        if (aadhaarVerificationStatus.HasValue)
        {
            AadhaarVerificationStatus = aadhaarVerificationStatus.Value;
        }

        if (panVerificationStatus.HasValue)
        {
            PanVerificationStatus = panVerificationStatus.Value;
        }

        if (!string.IsNullOrWhiteSpace(panVerifiedName))
        {
            PanVerifiedName = panVerifiedName.Trim();
        }

        NomineeName = string.IsNullOrWhiteSpace(nomineeName) ? NomineeName : nomineeName.Trim();
        NomineeRelation = string.IsNullOrWhiteSpace(nomineeRelation) ? NomineeRelation : nomineeRelation.Trim();

        if (nomineeDateOfBirth.HasValue)
        {
            NomineeDateOfBirth = nomineeDateOfBirth;
        }

        if (nomineeSharePercent is >= 1 and <= 100)
        {
            NomineeSharePercent = nomineeSharePercent.Value;
        }

        if (nomineeAddressSameAsMember.HasValue)
        {
            NomineeAddressSameAsMember = nomineeAddressSameAsMember.Value;
        }

        NomineeAddressLine1 = string.IsNullOrWhiteSpace(nomineeAddressLine1) ? NomineeAddressLine1 : nomineeAddressLine1.Trim();
        NomineeAddressLine2 = string.IsNullOrWhiteSpace(nomineeAddressLine2) ? NomineeAddressLine2 : nomineeAddressLine2.Trim();
        NomineeCity = string.IsNullOrWhiteSpace(nomineeCity) ? NomineeCity : nomineeCity.Trim();
        NomineeState = string.IsNullOrWhiteSpace(nomineeState) ? NomineeState : nomineeState.Trim();
        NomineePinCode = string.IsNullOrWhiteSpace(nomineePinCode) ? NomineePinCode : nomineePinCode.Trim();

        if (numberOfShares.HasValue)
        {
            NumberOfShares = numberOfShares;
        }

        if (shareFaceValue is > 0)
        {
            ShareFaceValue = shareFaceValue.Value;
        }

        if (sharePaymentMode.HasValue)
        {
            SharePaymentMode = sharePaymentMode;
        }

        if (employmentType.HasValue)
        {
            EmploymentType = employmentType;
        }

        Occupation = string.IsNullOrWhiteSpace(occupation) ? Occupation : occupation.Trim();
        EmployerName = string.IsNullOrWhiteSpace(employerName) ? EmployerName : employerName.Trim();

        if (monthlyIncome.HasValue)
        {
            MonthlyIncome = monthlyIncome;
        }
    }

    public void SubmitOnboarding()
    {
        EnsureNotClosed();

        if (Status != MemberStatus.Pending)
        {
            throw new InvalidOperationException("Only pending onboarding applications can be submitted.");
        }

        if (string.IsNullOrWhiteSpace(FullName))
        {
            throw new InvalidOperationException("Full name is required.");
        }

        if (!DateOfBirth.HasValue)
        {
            throw new InvalidOperationException("Date of birth is required.");
        }

        if (string.IsNullOrWhiteSpace(Gender))
        {
            throw new InvalidOperationException("Gender is required.");
        }

        if (string.IsNullOrWhiteSpace(MobileNumber))
        {
            throw new InvalidOperationException("Mobile number is required.");
        }

        if (string.IsNullOrWhiteSpace(AddressLine1) || string.IsNullOrWhiteSpace(City) ||
            string.IsNullOrWhiteSpace(State) || string.IsNullOrWhiteSpace(PinCode))
        {
            throw new InvalidOperationException("Complete address is required.");
        }

        if (string.IsNullOrWhiteSpace(AadhaarEncrypted) || string.IsNullOrWhiteSpace(AadhaarHash))
        {
            throw new InvalidOperationException("Aadhaar is required.");
        }

        if (string.IsNullOrWhiteSpace(PanEncrypted))
        {
            throw new InvalidOperationException("PAN is required.");
        }

        if (string.IsNullOrWhiteSpace(NomineeName) || string.IsNullOrWhiteSpace(NomineeRelation))
        {
            throw new InvalidOperationException("Nominee details are required.");
        }

        if (!NomineeDateOfBirth.HasValue)
        {
            throw new InvalidOperationException("Nominee date of birth is required.");
        }

        if (!NumberOfShares.HasValue || NumberOfShares < 1)
        {
            throw new InvalidOperationException("Number of shares is required.");
        }

        if (!SharePaymentMode.HasValue)
        {
            throw new InvalidOperationException("Share payment mode is required.");
        }

        Status = MemberStatus.Active;
        OnboardingStep = 4;

        RaiseDomainEvent(new MemberRegisteredDomainEvent(
            Id,
            TenantId,
            BranchId,
            MemberNumber,
            FullName));
    }

    public void SetPhotoStorageKey(string storageKey)
    {
        EnsureNotClosed();
        PhotoStorageKey = string.IsNullOrWhiteSpace(storageKey) ? null : storageKey.Trim();
    }

    public void MarkAadhaarVerificationPending()
    {
        EnsureNotClosed();
        AadhaarVerificationStatus = KycVerificationStatus.Pending;
    }

    public void MarkAadhaarVerified()
    {
        EnsureNotClosed();
        AadhaarVerificationStatus = KycVerificationStatus.Verified;
    }

    public void MarkPanVerified(string verifiedName)
    {
        EnsureNotClosed();
        PanVerificationStatus = KycVerificationStatus.Verified;
        PanVerifiedName = string.IsNullOrWhiteSpace(verifiedName) ? null : verifiedName.Trim();
    }

    public MemberDocument AddDocument(
        MemberDocumentType documentType,
        string fileName,
        string contentType,
        string storageKey,
        long fileSizeBytes)
    {
        EnsureNotClosed();

        var existing = _documents.FirstOrDefault(d => d.DocumentType == documentType);
        if (existing is not null)
        {
            _documents.Remove(existing);
        }

        var document = MemberDocument.Create(
            TenantId,
            Id,
            documentType,
            fileName,
            contentType,
            storageKey,
            fileSizeBytes);

        _documents.Add(document);
        return document;
    }

    public void UpdateProfile(
        string fullName,
        string mobileNumber,
        string? email,
        string addressLine1,
        string? addressLine2,
        string city,
        string state,
        string pinCode,
        string? nomineeName,
        string? nomineeRelation)
    {
        EnsureNotClosed();

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new InvalidOperationException("Full name is required.");
        }

        FullName = fullName.Trim();
        MobileNumber = mobileNumber.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        AddressLine1 = addressLine1.Trim();
        AddressLine2 = string.IsNullOrWhiteSpace(addressLine2) ? null : addressLine2.Trim();
        City = city.Trim();
        State = state.Trim();
        PinCode = pinCode.Trim();
        NomineeName = string.IsNullOrWhiteSpace(nomineeName) ? null : nomineeName.Trim();
        NomineeRelation = string.IsNullOrWhiteSpace(nomineeRelation) ? null : nomineeRelation.Trim();
    }

    public void ChangeStatus(MemberStatus newStatus)
    {
        if (Status == newStatus)
        {
            return;
        }

        if (!AllowedTransitions[Status].Contains(newStatus))
        {
            throw new InvalidOperationException(
                $"Cannot transition member status from {Status} to {newStatus}.");
        }

        Status = newStatus;
    }

    private void EnsureNotClosed()
    {
        if (Status == MemberStatus.Closed)
        {
            throw new InvalidOperationException("Closed member accounts cannot be modified.");
        }
    }
}
