using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Patsanstha.BuildingBlocks.Infrastructure.Api;
using Patsanstha.BuildingBlocks.Infrastructure.Idempotency;
using Patsanstha.Modules.Deposits.Application.DepositAccounts.CreateDepositAccount;
using Patsanstha.Modules.Deposits.Application.DepositAccounts.GetDepositAccount;
using Patsanstha.Modules.Deposits.Application.DepositAccounts.ListDepositAccounts;
using Patsanstha.Modules.Deposits.Application.DepositAccounts.UpdateDepositAccount;
using Patsanstha.Modules.Deposits.Domain.Enums;
using Patsanstha.Modules.Identity.Domain.Authorization;

namespace Patsanstha.Modules.Deposits.Api;

public static class DepositsEndpoints
{
    public static IEndpointRouteBuilder MapDepositsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/deposits").WithTags("Deposits");

        group.MapGet("/", async (
            ISender sender,
            int page = 1,
            int pageSize = 20,
            string? search = null,
            DepositProductType? productType = null,
            DepositAccountStatus? status = null,
            Guid? branchId = null,
            Guid? memberId = null) =>
        {
            var result = await sender.Send(new ListDepositAccountsQuery(
                page, pageSize, search, productType, status, branchId, memberId));
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.DepositsRead)
        .WithName("DepositsList");

        group.MapGet("/{depositAccountId:guid}", async (Guid depositAccountId, ISender sender) =>
        {
            var result = await sender.Send(new GetDepositAccountQuery(depositAccountId));
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.DepositsRead)
        .WithName("DepositsGetById");

        group.MapPost("/", async (CreateDepositAccountRequestBody request, ISender sender) =>
        {
            var command = new CreateDepositAccountCommand(
                request.MemberId,
                request.MemberNumber,
                request.MemberName,
                request.BranchId,
                request.ProductType,
                request.PrincipalAmount,
                request.TenureMonths,
                request.InterestPayoutMode,
                request.AutoRenewal);

            var result = await sender.Send(command);
            return result.IsSuccess
                ? result.ToCreatedResult($"/api/v1/deposits/{result.Value.Id}")
                : result.Error.ToProblemResult();
        })
        .RequireAuthorization(Permissions.DepositsCreate)
        .RequireIdempotency()
        .WithName("DepositsCreate");

        group.MapPut("/{depositAccountId:guid}", async (
            Guid depositAccountId,
            UpdateDepositAccountRequestBody request,
            ISender sender) =>
        {
            var command = new UpdateDepositAccountCommand(
                depositAccountId,
                request.Status,
                request.AutoRenewal);

            var result = await sender.Send(command);
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.DepositsUpdate)
        .WithName("DepositsUpdate");

        return app;
    }

    private sealed record CreateDepositAccountRequestBody(
        Guid MemberId,
        string MemberNumber,
        string MemberName,
        Guid BranchId,
        DepositProductType ProductType,
        decimal PrincipalAmount,
        int? TenureMonths,
        InterestPayoutMode InterestPayoutMode,
        bool AutoRenewal);

    private sealed record UpdateDepositAccountRequestBody(
        DepositAccountStatus? Status,
        bool? AutoRenewal);
}
