using System.Globalization;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Patsanstha.BuildingBlocks.Infrastructure.Api;
using Patsanstha.BuildingBlocks.Infrastructure.Idempotency;
using Patsanstha.Modules.Identity.Domain.Authorization;
using Patsanstha.Modules.Members.Application.Members.CreateMember;
using Patsanstha.Modules.Members.Application.Members.GetMember;
using Patsanstha.Modules.Members.Application.Members.ListMembers;
using Patsanstha.Modules.Members.Application.Members.UpdateMember;
using Patsanstha.Modules.Members.Domain.Enums;

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
                request.NomineeRelation);

            var result = await sender.Send(command);
            return result.IsSuccess
                ? result.ToCreatedResult($"/api/v1/members/{result.Value.Id}")
                : result.Error.ToProblemResult();
        })
        .RequireAuthorization(Permissions.MembersCreate)
        .RequireIdempotency()
        .WithName("MembersCreate");

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
        string? NomineeRelation);

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
