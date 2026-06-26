using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Patsanstha.BuildingBlocks.Infrastructure.Api;
using Patsanstha.Modules.Collections.Application.CollectionReceipts.CreateCollectionReceipt;
using Patsanstha.Modules.Collections.Application.CollectionReceipts.GetCollectionReceipt;
using Patsanstha.Modules.Collections.Application.CollectionReceipts.ListCollectionReceipts;
using Patsanstha.Modules.Collections.Application.CollectionReceipts.ReverseCollectionReceipt;
using Patsanstha.Modules.Collections.Domain.Enums;
using Patsanstha.Modules.Identity.Domain.Authorization;

namespace Patsanstha.Modules.Collections.Api;

public static class CollectionsEndpoints
{
    public static IEndpointRouteBuilder MapCollectionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/collections").WithTags("Collections");

        group.MapGet("/", async (
            ISender sender,
            int page = 1,
            int pageSize = 20,
            string? search = null,
            Guid? branchId = null,
            Guid? memberId = null,
            string? loanNumber = null) =>
        {
            var result = await sender.Send(new ListCollectionReceiptsQuery(
                page, pageSize, search, branchId, memberId, loanNumber));
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.CollectionsRead)
        .WithName("CollectionsList");

        group.MapGet("/{collectionReceiptId:guid}", async (Guid collectionReceiptId, ISender sender) =>
        {
            var result = await sender.Send(new GetCollectionReceiptQuery(collectionReceiptId));
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.CollectionsRead)
        .WithName("CollectionsGetById");

        group.MapPost("/", async (CreateCollectionReceiptRequestBody request, ISender sender) =>
        {
            var command = new CreateCollectionReceiptCommand(
                request.MemberId,
                request.MemberNumber,
                request.MemberName,
                request.LoanApplicationId,
                request.LoanNumber,
                request.BranchId,
                request.Amount,
                request.PaymentMode,
                request.ReferenceNumber,
                request.CollectedOn);

            var result = await sender.Send(command);
            return result.IsSuccess
                ? result.ToCreatedResult($"/api/v1/collections/{result.Value.Id}")
                : result.Error.ToProblemResult();
        })
        .RequireAuthorization(Permissions.CollectionsCollect)
        .WithName("CollectionsCreate");

        group.MapPut("/{collectionReceiptId:guid}/reverse", async (
            Guid collectionReceiptId,
            ISender sender) =>
        {
            var result = await sender.Send(new ReverseCollectionReceiptCommand(collectionReceiptId));
            return result.ToHttpResult();
        })
        .RequireAuthorization(Permissions.CollectionsCollect)
        .WithName("CollectionsReverse");

        return app;
    }

    private sealed record CreateCollectionReceiptRequestBody(
        Guid MemberId,
        string MemberNumber,
        string MemberName,
        Guid LoanApplicationId,
        string LoanNumber,
        Guid BranchId,
        decimal Amount,
        PaymentMode PaymentMode,
        string? ReferenceNumber,
        DateOnly CollectedOn);
}
