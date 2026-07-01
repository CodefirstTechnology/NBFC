using Patsanstha.Modules.Members.Domain.Enums;

namespace Patsanstha.Modules.Members.Application.Abstractions;

public sealed record MemberSummaryDto(
    Guid Id,
    string MemberNumber,
    string FullName,
    string MobileNumber,
    Guid BranchId,
    MemberStatus Status,
    DateOnly JoinedOn);

public sealed record MemberDocumentDto(
    Guid Id,
    MemberDocumentType DocumentType,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    DateTimeOffset CreatedAt);

public sealed record MemberDetailDto(
    Guid Id,
    string MemberNumber,
    string FullName,
    DateOnly? DateOfBirth,
    string Gender,
    string MobileNumber,
    string? Email,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string PinCode,
    string? AadhaarMasked,
    string? PanMasked,
    string? PhotoUrl,
    KycVerificationStatus AadhaarVerificationStatus,
    KycVerificationStatus PanVerificationStatus,
    string? PanVerifiedName,
    string? NomineeName,
    string? NomineeRelation,
    DateOnly? NomineeDateOfBirth,
    int NomineeSharePercent,
    bool NomineeAddressSameAsMember,
    string? NomineeAddressLine1,
    string? NomineeAddressLine2,
    string? NomineeCity,
    string? NomineeState,
    string? NomineePinCode,
    int? NumberOfShares,
    decimal ShareFaceValue,
    decimal? ShareTotalAmount,
    SharePaymentMode? SharePaymentMode,
    EmploymentType? EmploymentType,
    string? Occupation,
    string? EmployerName,
    decimal? MonthlyIncome,
    int OnboardingStep,
    IReadOnlyList<MemberDocumentDto> Documents,
    Guid BranchId,
    MemberStatus Status,
    DateOnly JoinedOn,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ModifiedAt);

public sealed record PagedMembersResponse(
    IReadOnlyList<MemberSummaryDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record ListMembersCriteria(
    int Page,
    int PageSize,
    string? Search,
    MemberStatus? Status,
    Guid? BranchId);

public sealed record OnboardingDraftDto(
    Guid Id,
    string MemberNumber,
    int OnboardingStep,
    MemberStatus Status,
    DateTimeOffset? ModifiedAt);

public sealed record KycVerificationResultDto(
    KycVerificationStatus Status,
    string? VerifiedName,
    string Message);
