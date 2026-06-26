using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Patsanstha.BuildingBlocks.Infrastructure.Api;
using Patsanstha.Modules.Identity.Domain.Authorization;
using Patsanstha.Modules.Recovery.Application.RecoveryCases.CreateRecoveryCase;
using Patsanstha.Modules.Recovery.Application.RecoveryCases.GetRecoveryCase;
using Patsanstha.Modules.Recovery.Application.RecoveryCases.ListRecoveryCases;
using Patsanstha.Modules.Recovery.Application.RecoveryCases.UpdateRecoveryCase;
using Patsanstha.Modules.Recovery.Domain.Enums;

namespace Patsanstha.Modules.Recovery.Api;

public static class RecoveryEndpoints
{
    public static IEndpointRouteBuilder MapRecoveryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/recovery").WithTags("Recovery");

        group.MapGet("/", async (
            ISender sender,
            int page = 1,
            int pageSize = 20,
            string? search = null,
            RecoveryCaseStatus? status = null,
            Guid? branchId = null,
            Guid? memberId = null,
            Guid? assignedToUserId = null) =>
        {
            var result = await sender.Send(new ListRecoveryCasesQuery(
                page, pageSize, search, status, branchId, memberId, assignedToUserId));
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.RecoveryRead)
        .WithName("RecoveryList");

        group.MapGet("/{recoveryCaseId:guid}", async (Guid recoveryCaseId, ISender sender) =>
        {
            var result = await sender.Send(new GetRecoveryCaseQuery(recoveryCaseId));
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.RecoveryRead)
        .WithName("RecoveryGetById");

        group.MapPost("/", async (CreateRecoveryCaseRequestBody request, ISender sender) =>
        {
            var command = new CreateRecoveryCaseCommand(
                request.LoanApplicationId,
                request.LoanNumber,
                request.MemberId,
                request.MemberNumber,
                request.MemberName,
                request.BranchId,
                request.OutstandingAmount,
                request.DaysPastDue,
                request.Notes);

            var result = await sender.Send(command);
            return result.IsSuccess
                ? result.ToCreatedResult($"/api/v1/recovery/{result.Value.Id}")
                : result.Error.ToProblemResult();
        })
        .RequireAuthorization(Permissions.RecoveryManage)
        .WithName("RecoveryCreate");

        group.MapPut("/{recoveryCaseId:guid}", async (
            Guid recoveryCaseId,
            UpdateRecoveryCaseRequestBody request,
            ISender sender) =>
        {
            var command = new UpdateRecoveryCaseCommand(
                recoveryCaseId,
                request.Status,
                request.AssignedToUserId,
                request.Notes);

            var result = await sender.Send(command);
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.RecoveryManage)
        .WithName("RecoveryUpdate");

        return app;
    }

    private sealed record CreateRecoveryCaseRequestBody(
        Guid LoanApplicationId,
        string LoanNumber,
        Guid MemberId,
        string MemberNumber,
        string MemberName,
        Guid BranchId,
        decimal OutstandingAmount,
        int DaysPastDue,
        string? Notes);

    private sealed record UpdateRecoveryCaseRequestBody(
        RecoveryCaseStatus? Status,
        Guid? AssignedToUserId,
        string? Notes);
}
