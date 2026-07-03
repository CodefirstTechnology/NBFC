using System.Globalization;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Patsanstha.BuildingBlocks.Infrastructure.Api;
using Patsanstha.BuildingBlocks.Infrastructure.Idempotency;
using Patsanstha.Modules.Identity.Domain.Authorization;
using Patsanstha.Modules.Members.Application.Members.CreateMember;
using Patsanstha.Modules.Members.Application.Members.GetMember;
using Patsanstha.Modules.Members.Application.Members.ListMembers;
using Patsanstha.Modules.Members.Application.Members.SaveOnboardingDraft;
using Patsanstha.Modules.Members.Application.Members.SubmitOnboarding;
using Patsanstha.Modules.Members.Application.Members.UpdateMember;
using Patsanstha.Modules.Members.Application.Members.UploadDocument;
using Patsanstha.Modules.Members.Application.Members.VerifyKyc;
using Patsanstha.Modules.Members.Domain.Entities;
using Patsanstha.Modules.Members.Domain.Enums;
using Patsanstha.Modules.Members.Infrastructure.Options;
using Patsanstha.Modules.Members.Infrastructure.Storage;
using Microsoft.Extensions.Options;

namespace Patsanstha.Modules.Members.Api;

public static class MembersEndpoints
{
    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000010");

    private static readonly string[] SupportedDateFormats =
    [
        "yyyy-MM-dd",
        "dd-MM-yyyy",
        "dd/MM/yyyy",
        "MM/dd/yyyy",
    ];

    public static IEndpointRouteBuilder MapMembersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/members").WithTags("Members");

        group.MapGet("/", async (
            ISender sender,
            int page = 1,
            int pageSize = 20,
            string? search = null,
            MemberStatus? status = null,
            Guid? branchId = null) =>
        {
            var result = await sender.Send(new ListMembersQuery(page, pageSize, search, status, branchId));
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.MembersRead)
        .WithName("MembersList");

        group.MapGet("/{memberId:guid}", async (Guid memberId, ISender sender) =>
        {
            var result = await sender.Send(new GetMemberQuery(memberId));
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.MembersRead)
        .WithName("MembersGetById");

        group.MapPost("/", async (CreateMemberRequestBody request, ISender sender) =>
        {
            if (!TryResolveBranchId(request.BranchId, out var branchId, out var branchError))
            {
                return branchError!;
            }

            if (!TryParseDateOfBirth(request.DateOfBirth, out var dateOfBirth, out var dateError))
            {
                return dateError!;
            }

            DateOnly? nomineeDob = null;
            if (!string.IsNullOrWhiteSpace(request.NomineeDateOfBirth))
            {
                if (!TryParseDateOfBirth(request.NomineeDateOfBirth, out var parsedNomineeDob, out var nomineeDateError))
                {
                    return nomineeDateError!;
                }

                nomineeDob = parsedNomineeDob;
            }

            var command = new CreateMemberCommand(
                branchId,
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
                request.Aadhaar,
                request.Pan,
                request.NomineeName,
                request.NomineeRelation,
                nomineeDob,
                request.NomineeSharePercent ?? 100,
                request.NomineeAddressSameAsMember ?? true,
                request.NomineeAddressLine1,
                request.NomineeAddressLine2,
                request.NomineeCity,
                request.NomineeState,
                request.NomineePinCode,
                request.NumberOfShares,
                request.ShareFaceValue ?? Member.DefaultShareFaceValue,
                request.SharePaymentMode,
                request.EmploymentType,
                request.Occupation,
                request.EmployerName,
                request.MonthlyIncome,
                request.AadhaarVerificationStatus ?? KycVerificationStatus.Pending,
                request.PanVerificationStatus ?? KycVerificationStatus.Pending,
                request.PanVerifiedName);

            var result = await sender.Send(command);
            return result.IsSuccess
                ? result.ToCreatedResult($"/api/v1/members/{result.Value.Id}")
                : result.Error.ToProblemResult();
        })
        .RequireAuthorization(Permissions.MembersCreate)
        .RequireIdempotency()
        .WithName("MembersCreate");

        group.MapPost("/onboarding/draft", async (SaveOnboardingDraftRequestBody request, ISender sender) =>
        {
            if (!TryResolveBranchId(request.BranchId, out var branchId, out var branchError))
            {
                return branchError!;
            }

            var command = new SaveOnboardingDraftCommand(
                request.MemberId,
                branchId,
                request.OnboardingStep,
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
                request.Aadhaar,
                request.Pan,
                request.NomineeName,
                request.NomineeRelation,
                request.NomineeDateOfBirth,
                request.NomineeSharePercent,
                request.NomineeAddressSameAsMember,
                request.NomineeAddressLine1,
                request.NomineeAddressLine2,
                request.NomineeCity,
                request.NomineeState,
                request.NomineePinCode,
                request.NumberOfShares,
                request.ShareFaceValue,
                request.SharePaymentMode,
                request.EmploymentType,
                request.Occupation,
                request.EmployerName,
                request.MonthlyIncome);

            var result = await sender.Send(command);
            return result.IsSuccess
                ? (request.MemberId.HasValue
                    ? result.ToHttpResult()
                    : result.ToCreatedResult($"/api/v1/members/{result.Value.Id}"))
                : result.Error.ToProblemResult();
        })
        .RequireAuthorization(Permissions.MembersCreate)
        .WithName("MembersSaveOnboardingDraft");

        group.MapPost("/{memberId:guid}/submit", async (Guid memberId, ISender sender) =>
        {
            var result = await sender.Send(new SubmitOnboardingCommand(memberId));
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.MembersCreate)
        .WithName("MembersSubmitOnboarding");

        group.MapPost("/{memberId:guid}/documents", async (
            Guid memberId,
            [FromForm] UploadMemberDocumentRequest request,
            ISender sender) =>
        {
            if (request.File is null || request.File.Length == 0)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "validation_error",
                    detail: "Document file is required.",
                    type: "https://patsanstha.local/errors/validation");
            }

            await using var stream = request.File.OpenReadStream();
            var command = new UploadMemberDocumentCommand(
                memberId,
                request.DocumentType,
                request.File.FileName,
                request.File.ContentType,
                stream,
                request.File.Length);

            var result = await sender.Send(command);
            return result.ToHttpResult();
        })
        .DisableAntiforgery()
        .RequireAuthorization(Permissions.MembersCreate)
        .WithName("MembersUploadDocument");

        group.MapPost("/{memberId:guid}/kyc/verify-aadhaar", async (
            Guid memberId,
            VerifyAadhaarRequestBody request,
            ISender sender) =>
        {
            var result = await sender.Send(new VerifyMemberAadhaarCommand(memberId, request.Aadhaar));
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.MembersCreate)
        .WithName("MembersVerifyAadhaar");

        group.MapPost("/{memberId:guid}/kyc/verify-pan", async (
            Guid memberId,
            VerifyPanRequestBody request,
            ISender sender) =>
        {
            var result = await sender.Send(new VerifyMemberPanCommand(memberId, request.Pan));
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.MembersCreate)
        .WithName("MembersVerifyPan");

        group.MapGet("/files/{*storageKey}", (
            string storageKey,
            IOptions<MemberDocumentStorageOptions> options) =>
        {
            var storage = new LocalMemberDocumentStorage(options);
            var physicalPath = storage.GetPhysicalPath(storageKey);

            if (!File.Exists(physicalPath))
            {
                return Results.NotFound();
            }

            var contentType = GetContentType(physicalPath);
            return Results.File(physicalPath, contentType, enableRangeProcessing: true);
        })
        .RequireAuthorization(Permissions.MembersRead)
        .WithName("MembersGetDocumentFile");

        group.MapPut("/{memberId:guid}", async (
            Guid memberId,
            UpdateMemberRequestBody request,
            ISender sender) =>
        {
            var command = new UpdateMemberCommand(
                memberId,
                request.FullName,
                request.MobileNumber,
                request.Email,
                request.AddressLine1,
                request.AddressLine2,
                request.City,
                request.State,
                request.PinCode,
                request.NomineeName,
                request.NomineeRelation,
                request.Status);

            var result = await sender.Send(command);
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.MembersUpdate)
        .WithName("MembersUpdate");

        return app;
    }

    private sealed record CreateMemberRequestBody(
        string? BranchId,
        string FullName,
        string DateOfBirth,
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
        string? NomineeDateOfBirth = null,
        int? NomineeSharePercent = null,
        bool? NomineeAddressSameAsMember = null,
        string? NomineeAddressLine1 = null,
        string? NomineeAddressLine2 = null,
        string? NomineeCity = null,
        string? NomineeState = null,
        string? NomineePinCode = null,
        int? NumberOfShares = null,
        decimal? ShareFaceValue = null,
        SharePaymentMode? SharePaymentMode = null,
        EmploymentType? EmploymentType = null,
        string? Occupation = null,
        string? EmployerName = null,
        decimal? MonthlyIncome = null,
        KycVerificationStatus? AadhaarVerificationStatus = null,
        KycVerificationStatus? PanVerificationStatus = null,
        string? PanVerifiedName = null);

    private sealed record SaveOnboardingDraftRequestBody(
        Guid? MemberId,
        string? BranchId,
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
        decimal? MonthlyIncome);

    private sealed record VerifyAadhaarRequestBody(string Aadhaar);

    private sealed record VerifyPanRequestBody(string Pan);

    private sealed class UploadMemberDocumentRequest
    {
        public MemberDocumentType DocumentType { get; set; }

        public IFormFile? File { get; set; }
    }

    private static bool TryResolveBranchId(string? branchId, out Guid resolved, out IResult? error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(branchId))
        {
            resolved = DefaultBranchId;
            return true;
        }

        if (Guid.TryParse(branchId, out resolved) && resolved != Guid.Empty)
        {
            return true;
        }

        error = Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "validation_error",
            detail: "Branch ID must be a valid GUID.",
            type: "https://patsanstha.local/errors/validation");
        resolved = Guid.Empty;
        return false;
    }

    private static bool TryParseDateOfBirth(
        string dateOfBirth,
        out DateOnly parsed,
        out IResult? error)
    {
        error = null;
        parsed = default;

        if (string.IsNullOrWhiteSpace(dateOfBirth))
        {
            error = Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "validation_error",
                detail: "Date of birth is required (YYYY-MM-DD).",
                type: "https://patsanstha.local/errors/validation");
            return false;
        }

        if (DateOnly.TryParseExact(
                dateOfBirth.Trim(),
                SupportedDateFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out parsed))
        {
            return true;
        }

        if (DateOnly.TryParse(dateOfBirth.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
        {
            return true;
        }

        error = Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "validation_error",
            detail: "Date of birth must be a valid date (YYYY-MM-DD).",
            type: "https://patsanstha.local/errors/validation");
        return false;
    }

    private static string GetContentType(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream",
        };
    }

    private sealed record UpdateMemberRequestBody(
        string FullName,
        string MobileNumber,
        string? Email,
        string AddressLine1,
        string? AddressLine2,
        string City,
        string State,
        string PinCode,
        string? NomineeName,
        string? NomineeRelation,
        MemberStatus? Status);
}
